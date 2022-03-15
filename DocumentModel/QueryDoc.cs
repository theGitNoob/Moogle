using System.Text;

namespace DocumentModel
{
    public class Query : Document
    {

        private string[] Terms = new string[0];

        private string[] ExcludedTerms = new string[0];

        private string[] MandatoryTerms = new string[0];

        private Dictionary<string, int> TermRelevance = new Dictionary<string, int>();

        List<string[]> NearTerms = new List<string[]>();

        //Used to check wheter the normal form of a term is given on the query
        private HashSet<String> NormalWords = new HashSet<string>();

        //Used to check wheter the root of a term is given on the query
        private HashSet<String> StemmedWords = new HashSet<string>();

        //Used to check wheter the synonomous of a term is given on the query
        private HashSet<String> RelatedWords = new HashSet<string>();


        /// <summary>
        ///     Generates the list of Terms that documents must not include
        /// </summary>
        private void GenExcludeList(string[] unescapedWords)
        {
            //Terms to be excluded
            ExcludedTerms = unescapedWords.Where((elem) => elem[0] == '!').
                            Select((term) => TermUtils.Trim(term)).Distinct().ToArray();
        }


        /// <summary>
        ///     Generates the list of Terms that documents must include
        /// </summary>
        private void GenIncludeList(string[] unescapedWords)
        {
            //Terms to be included
            MandatoryTerms = unescapedWords.Where((elem) => elem[0] == '^').
                             Select((term) => TermUtils.Trim(term)).Distinct().ToArray();

        }
        /// <summary>
        ///    Saves the importance of each term according to the relevance operator
        /// </summary>
        private void GenImportantList(string[] unescapedWords)
        {

            this.TermRelevance = new Dictionary<string, int>();

            for (int idx = 0; idx < unescapedWords.Length; idx++)
            {
                string term = unescapedWords[idx];

                if (term.Length == 0) continue;

                string trimmed = TermUtils.Trim(term);

                if (term[0] == '*')
                {
                    int counter = 0;

                    for (int j = 0; j < term.Length; j++)
                    {
                        if (term[j] == '*') counter++;
                        else break;
                    }

                    if (TermRelevance.ContainsKey(trimmed))
                    {
                        TermRelevance[trimmed] += counter;
                    }
                    else
                    {
                        TermRelevance.Add(trimmed, counter);
                    }

                }
            }
        }
        /// <summary>
        ///    Generates the list of the terms that are relatet trough the clossenes operator
        /// </summary>
        private void GenNearbyList(string[] unescapedWords, ref string query)
        {

            foreach (string cad in unescapedWords)
            {
                string[] nearby = cad.Split("~").Distinct().ToArray();

                if (nearby.Length == 1) continue;

                StringBuilder auxCad = new StringBuilder();

                foreach (string term in nearby)
                {
                    auxCad.Append(term + " ");
                }

                query = query.Replace(cad, auxCad.ToString());

                NearTerms.Add(nearby);
            }
        }


        /// <summary>
        ///     Adds each term of the query
        /// </summary>
        private void SaveTerms(string[] wordList)
        {
            foreach (string term in wordList)
            {
                AddTerm(term);
                NormalWords.Add(term);
            }

            this.Terms = wordList;
        }


        /// <summary>
        ///     Saves the root of each term on the query
        /// </summary>
        private void SaveRoots(string[] wordlist)
        {
            foreach (string term in wordlist)
            {
                string root = Stemmer.Stemmer.Stemm(term);

                if (DocumentCollection.Contains(root) && root != term)
                {
                    AddTerm(root);

                    if (!NormalWords.Contains(root))
                        StemmedWords.Add(root);
                }
            }
        }

        /// <summary>
        ///     Saves the synonomus of each term on the query
        /// </summary>
        private void SaveSynonomus(string[] wordlist)
        {
            foreach (string term in wordlist)
            {
                string[] syns = SynonomusDB.GetSynonomus(term);

                foreach (string syn in syns)
                {
                    if (DocumentCollection.Contains(syn))
                    {
                        AddTerm(syn);
                        if (!NormalWords.Contains(syn) && !StemmedWords.Contains(term))
                            RelatedWords.Add(syn);

                        //Augment relevance of the synonomus but less than relevance of original words
                        if (TermRelevance.ContainsKey(term))
                        {
                            if (TermRelevance.ContainsKey(syn))
                            {
                                TermRelevance[syn] += TermRelevance[term] - 1;
                            }
                            else
                            {
                                TermRelevance.Add(syn, TermRelevance[term] - 1);
                            }
                        }
                    }
                }
            }
        }



        /// <summary>
        ///      Excludes documents acording to the exclude operator
        /// </summary>
        private bool ExcludeDoc(Document doc)
        {
            foreach (string term in ExcludedTerms)
            {
                if (doc.ContainsTerm(term))
                {
                    return true;
                }
            }

            return false;
        }



        /// <summary>
        ///      Exclude documents if they no contains mandatory terms
        /// </summary>
        private bool IncludeDoc(Document doc)
        {
            foreach (string term in MandatoryTerms)
            {
                if (!doc.ContainsTerm(term))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///      Adds the term to the document and to the DocumentCollectin, also update the max frequency
        /// </summary>
        private void AddTerm(string term)
        {
            if (this.Data.ContainsKey(term))
            {
                this.Data[term].frequency++;

                int wordFreq = this.Data[term].frequency;

                if (wordFreq > MaxFrequency)
                {
                    this.MaxFrequency = wordFreq;
                }
            }
            else
            {
                Data.Add(term, new TermData(0, 1));
            }
        }


        /// <summary>
        ///      Calc the weigth of each term using tf-idf and
        ///      stores it
        /// </summary>
        private void SaveWeight()
        {
            foreach (string term in Data.Keys)
            {
                double idf = DocumentCollection.CalcIDF(term);

                double weight = CalcWeigth(term, idf);

                AddWeigth(term, weight);
            }

        }


        /// <summary>
        ///     The  Query class constructor
        /// </summary>
        public Query(string query)
        {
            query = query.ToLower();

            string[] unescapedWords = TermUtils.RemoveScape(query);

            GenExcludeList(unescapedWords);

            GenIncludeList(unescapedWords);

            GenImportantList(unescapedWords);

            GenNearbyList(unescapedWords, ref query);

            string[] terms = TermUtils.Tokenize(query);

            MaxFrequency = 1;

            SaveTerms(terms);

            SaveRoots(terms);

            SaveSynonomus(terms);

            CalcTF();

            SaveWeight();


        }

        /// <summary>
        ///     Generates a array of terms that should be included on snippet,
        ///      a synonomus will not be included if the corresponding term is 
        ///      already included 
        ///
        /// </summary>

        //
        private string[] GenSnippetTerms(Document doc)
        {
            List<string> TermList = new List<string>();

            foreach (string key in NormalWords)
            {
                if (doc.ContainsTerm(key))
                {
                    TermList.Add(key);
                }
                else
                {
                    TermList.AddRange(SynonomusDB.GetSynonomus(key));
                }
            }

            return TermList.ToArray();
        }

        /// <summary>
        ///    Passes across each document on the collection and finds
        ///    the angle between them and the vectorized query
        /// </summary>
        public List<Tuple<string, string, double>> GetResults()
        {

            bool augmentQuery = NearTerms.Count == 0;

            List<Tuple<string, string, double>> results = new List<Tuple<string, string, double>>();

            foreach (Document doc in DocumentCollection.Docs)
            {
                if (ExcludeDoc(doc) || !IncludeDoc(doc)) continue;

                double dotProd = 0;

                double queryNorm = this.Norm;

                double docNorm = doc.Norm;

                foreach (string term in this.Data.Keys)
                {
                    double relevance = TermRelevance.ContainsKey(term) ? TermRelevance[term] + 1 : 1;

                    double query_idf = DocumentCollection.CalcIDF(term);

                    bool isRoot = StemmedWords.Contains(term) == true;

                    bool isSyn = isRoot == false && RelatedWords.Contains(term) == true;

                    double query_weigth = CalcWeigth(term, query_idf) * (relevance);

                    if (isRoot)
                    {
                        query_weigth *= 0.5;

                    }
                    else if (isSyn)
                    {
                        query_weigth *= 0.1;
                    }

                    double doc_weigth = doc.CalcWeigth(term, DocumentCollection.CalcIDF(term));

                    dotProd += (doc_weigth * query_weigth);

                }

                int multiplier = doc.FindClosestTerms(NearTerms);

                queryNorm = Math.Sqrt(queryNorm);

                double normProd = (docNorm * queryNorm);

                double cosin = (double)dotProd / normProd;

                cosin *= multiplier;

                if (!double.IsNaN(cosin) && cosin != 0)
                {
                    Tuple<string, string, double> tuple = new Tuple<string, string, double>(doc.Title, doc.GetSnippet(GenSnippetTerms(doc)), (cosin));
                    results.Add(tuple);
                }

            }

            return results;

        }

    }
}