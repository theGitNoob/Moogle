namespace DocumentModel
{
    //
    // Summary:
    //     Class containing the collection of all indexed documents
    //     and all the methos related to the terms on the collection
    //
    public class DocumentCollection
    {
        //
        // Summary:
        //     A list with all the index documents
        //
        public List<Document> Docs { get; private set; }

        //
        // Summary:
        //    A Dictionary with the frequency of each term on the document collection
        //
        private Dictionary<String, int> GlobalFreq;

        //
        // Summary:
        //     The size of the document collection
        //
        static public int s_Size { get; private set; }

        //
        // Summary:
        //     Create a new instance of the DocumentCollection class
        //
        public DocumentCollection()
        {
            Docs = new List<Document>();
            GlobalFreq = new Dictionary<string, int>();

        }
        //
        // Summary:
        //     Adds a document to the collection and increase the size of the collection by one
        //
        public void Add(Document doc)
        {
            Docs.Add(doc);
            s_Size++;
        }


        //
        // Summary:
        //     Checks for the existence of a term on the collection
        //  Returns:
        //     true if term exits, false otherwise
        public bool Contains(string term)
        {
            return GlobalFreq.ContainsKey(term);

        }

        //
        // Summary:
        //     Gives the global frequecy of the term
        //  Returns:
        //     0 if term not exists, the global frequency otherwise

        public int GetGlobalFrequency(string term)
        {
            if (Contains(term))
                return GlobalFreq[term];

            return 0;
        }
        //
        // Summary:
        //     Augment the frequency of the term on the collection
        //
        public void AddTerm(string term)
        {
            if (Contains(term))
            {
                GlobalFreq[term]++;
            }
            else
            {
                GlobalFreq.Add(term, 1);
            }
        }


        //
        // Summary:
        //      Calcs the Inverse Document Frequency
        // Returns:
        //      A double representing the idf of the term
        //
        public double CalcIDF(string term)
        {
            int cnt = 0;

            if (this.Contains(term))
            {
                cnt = GlobalFreq[term];

                return Math.Log2((double)s_Size / cnt);
            }

            return 0;
        }


        //
        // Summary:
        //     Fills the weight of each term on each document using tf-idf
        //
        public void FillWeigths()
        {
            foreach (string term in GlobalFreq.Keys)
            {
                foreach (Document doc in Docs)
                {
                    if (doc.ContainsTerm(term))
                    {
                        double termIdf = CalcIDF(term);

                        double termWeigth = doc.CalcWeigth(term, termIdf);

                        doc.AddWeigth(term, termWeigth);
                    }

                }
            }
        }

        //
        // Summary: 
        //      Get the misspell for a given term according to ther similarity
        // Returns:
        //      A string that is the most probabbly correct term
        //
        public string GetMisspell(string term)
        {

            if (term.Length <= 2) return "";

            if (!TermUtils.IsAlpha(term)) return "";

            int med = int.MaxValue;

            string word = "";

            int maxAllowedDiff = term.Length / 3;

            foreach (string key in GlobalFreq.Keys)
            {
                if (key == term || Math.Abs(key.Length - term.Length) > maxAllowedDiff || !TermUtils.IsAlpha(key)) continue;

                int ed = TermUtils.EditDistance(term, key);

                if (ed < med)
                {
                    med = ed;
                    word = key;
                }
                if (ed == med)
                {
                    if (GetGlobalFrequency(word) > GetGlobalFrequency(key))
                    {
                        word = key;
                    }
                }
            }
            return word;
        }
    }
}