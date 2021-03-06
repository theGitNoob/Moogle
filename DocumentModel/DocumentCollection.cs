namespace DocumentModel
{
    //
    // Summary:
    //     Class containing the collection of all indexed documents
    //     and all the methos related to the terms on the collection
    //
    static public class DocumentCollection
    {
        //
        // Summary:
        //     A list with all the index documents
        //
        static public List<Document> Docs { get; private set; } = new List<Document>();

        //
        // Summary:
        //    A Dictionary with the frequency of each term on the document collection
        //
        static private Dictionary<String, int> s_globalFreq = new Dictionary<string, int>();

        //
        // Summary:
        //     The size of the document collection
        //
        static public int s_Size { get; private set; }

        //
        // Summary:
        //     Create a new instance of the DocumentCollection class
        //
        // public DocumentCollection()
        // {
        //     Docs = new List<Document>();
        //     GlobalFreq = new Dictionary<string, int>();
        // }
        //
        // Summary:
        //     Adds a document to the collection and increase the size of the collection by one
        //
        static public void Add(Document doc)
        {
            Docs.Add(doc);
            s_Size++;
        }


        //
        // Summary:
        //     Checks for the existence of a term on the collection
        //  Returns:
        //     true if term exits, false otherwise
        static public bool Contains(string term)
        {
            return s_globalFreq.ContainsKey(term);

        }

        //
        // Summary:
        //     Gives the global frequecy of the term
        //  Returns:
        //     0 if term not exists, the global frequency otherwise

        static public int GetGlobalFrequency(string term)
        {
            if (Contains(term))
                return s_globalFreq[term];

            return 0;
        }
        //
        // Summary:
        //     Augment the frequency of the term on the collection
        //
        static public void AddTerm(string term)
        {
            if (Contains(term))
            {
                s_globalFreq[term]++;
            }
            else
            {
                s_globalFreq.Add(term, 1);
            }
        }


        //
        // Summary:
        //      Calcs the Inverse Document Frequency
        // Returns:
        //      A double representing the idf of the term
        //
        static public double CalcIDF(string term)
        {
            int cnt = 0;

            if (Contains(term))
            {
                cnt = s_globalFreq[term];

                return Math.Log2((double)s_Size / cnt);
            }

            return 0;
        }


        //
        // Summary:
        //     Fills the weight of each term on each document using tf-idf
        //
        static public void FillWeigths()
        {
            foreach (string term in s_globalFreq.Keys)
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
        static public string GetMisspell(string term)
        {

            if (term.Length <= 2) return "";

            if (!TermUtils.IsAlpha(term)) return "";

            int minEditDistance = int.MaxValue;

            string suggestion = "";

            int maxAllowedDiff = term.Length / 3;

            foreach (string key in s_globalFreq.Keys)
            {
                if (key == term || Math.Abs(key.Length - term.Length) > maxAllowedDiff || !TermUtils.IsAlpha(key)) continue;

                int editDistance = TermUtils.EditDistance(term, key);

                if (editDistance < minEditDistance)
                {
                    minEditDistance = editDistance;
                    suggestion = key;
                }
                if (editDistance == minEditDistance)
                {
                    if (GetGlobalFrequency(suggestion) < GetGlobalFrequency(key))
                    {
                        suggestion = key;
                    }
                }
            }
            return suggestion;
        }
    }
}