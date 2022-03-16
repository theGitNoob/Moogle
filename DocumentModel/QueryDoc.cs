namespace DocumentModel;

using System.Text;
using Stemmer;

public class Query : Document
{

    private string[] _terms = new string[0];

    private string[] _excludedTerms = new string[0];

    private string[] _mandatoryTerms = new string[0];

    private Dictionary<string, int> _termRelevance = new Dictionary<string, int>();

    private List<string[]> _nearTerms = new List<string[]>();

    //Used to check wheter the normal form of a term is given on the query
    private HashSet<String> _normalWords = new HashSet<string>();

    //Used to check wheter the root of a term is given on the query
    private HashSet<String> _stemmedWords = new HashSet<string>();

    //Used to check wheter the synonomous of a term is given on the query
    private HashSet<String> _relatedWords = new HashSet<string>();


    /// <summary>
    ///     Generates the list of Terms that documents must not include
    /// </summary>
    private void GenExcludeList(string[] unescapedWords)
    {
        //Terms to be excluded
        _excludedTerms = unescapedWords.Where((elem) => elem[0] == '!').
                        Select((term) => TermUtils.Trim(term)).Distinct().ToArray();
    }


    /// <summary>
    ///     Generates the list of Terms that documents must include
    /// </summary>
    private void GenIncludeList(string[] unescapedWords)
    {
        //Terms to be included
        _mandatoryTerms = unescapedWords.Where((elem) => elem[0] == '^').
                         Select((term) => TermUtils.Trim(term)).Distinct().ToArray();

    }
    /// <summary>
    ///    Saves the importance of each term according to the relevance operator
    /// </summary>
    private void GenImportantList(string[] unescapedWords)
    {

        this._termRelevance = new Dictionary<string, int>();

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

                if (_termRelevance.ContainsKey(trimmed))
                {
                    _termRelevance[trimmed] += counter;
                }
                else
                {
                    _termRelevance.Add(trimmed, counter);
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

            _nearTerms.Add(nearby);
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
            _normalWords.Add(term);
        }

        this._terms = wordList;
    }


    /// <summary>
    ///     Saves the root of each term on the query
    /// </summary>
    private void SaveRoots(string[] wordlist)
    {
        foreach (string term in wordlist)
        {
            string root = Stemmer.Stemm(term);

            if (DocumentCollection.Contains(root) && root != term)
            {
                AddTerm(root);

                if (!_normalWords.Contains(root))
                    _stemmedWords.Add(root);
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
                    if (!_normalWords.Contains(syn) && !_stemmedWords.Contains(term))
                        _relatedWords.Add(syn);

                    //Augment relevance of the synonomus but less than relevance of original words
                    if (_termRelevance.ContainsKey(term))
                    {
                        if (_termRelevance.ContainsKey(syn))
                        {
                            _termRelevance[syn] += _termRelevance[term] - 1;
                        }
                        else
                        {
                            _termRelevance.Add(syn, _termRelevance[term] - 1);
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
        foreach (string term in _excludedTerms)
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
        foreach (string term in _mandatoryTerms)
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

        foreach (string key in _normalWords)
        {
            if (doc.ContainsTerm(key))
            {
                TermList.Add(key);
            }
            else
            {
                TermList.Add(Stemmer.Stemm(key));
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

        bool augmentQuery = _nearTerms.Count == 0;

        List<Tuple<string, string, double>> results = new List<Tuple<string, string, double>>();

        foreach (Document doc in DocumentCollection.Docs)
        {
            if (ExcludeDoc(doc) || !IncludeDoc(doc)) continue;

            double dotProd = 0;

            double queryNorm = this.Norm;

            double docNorm = doc.Norm;

            foreach (string term in this.Data.Keys)
            {

                double relevance = _termRelevance.ContainsKey(term) ? _termRelevance[term] + 1 : 1;

                double query_idf = DocumentCollection.CalcIDF(term);

                bool isRoot = _stemmedWords.Contains(term) == true;

                bool isSyn = isRoot == false && _relatedWords.Contains(term) == true;

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

            int multiplier = doc.FindClosestTerms(_nearTerms);

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

    /// <summary>
    ///    Builds a new query if the search result doesn't give much information
    /// </summary>

    public string GetSugestion()
    {
        StringBuilder newQuery = new StringBuilder();

        foreach (string term in this._terms)
        {
            string misspell = DocumentCollection.GetMisspell(term);

            int misspellFreq = 0;

            int termFreq = 0;

            //Only appends suggestion if is in the Document collection
            if (DocumentCollection.Contains(term))
            {
                termFreq = DocumentCollection.GetGlobalFrequency(term);
            }
            else
            {
                newQuery.Append(misspell + " ");
                continue;
            }


            //If the suggestion is on the document collection 
            if (DocumentCollection.Contains(misspell) && misspellFreq > termFreq && termFreq < 5)
            {
                misspellFreq = DocumentCollection.GetGlobalFrequency(misspell);

                newQuery.Append(misspell + " ");
            }
            else
            {
                newQuery.Append(term + " ");
            }
        }

        if (newQuery.Length != 0)
            newQuery.Remove(newQuery.Length - 1, 1);

        return newQuery.ToString();
    }

}
