namespace DocumentModel;

public class Document
{

    string[] trimmedWords;

    //Get the misspell for a given term
    public static string getMisspell(string term)
    {
        //med stands for minimum edit distance
        double med = 0;
        string word = "";


        foreach (string key in s_termSet)
        {
            int ed = EditDistance(term, key);
            if (ed == 0) continue;
            double f = 1 / (double)((ed));

            double score = f * CalcIDF(key);
            if (score > med)
            {
                med = score;
                word = key;
            }
        }
        return word;
    }

    //Calcs the EditDistance between two words
    public static int EditDistance(string a, string b)
    {

        //TODO:Change edit distance to add weigth according to
        //operations and the distance between letters on the keyboard
        int aLen = a.Length;
        int bLen = b.Length;



        if (aLen == 0) return bLen;
        if (bLen == 0) return aLen;

        //Fills the DP table with base cases
        int[,] distance = new int[aLen + 1, bLen + 1];


        distance[0, 0] = 0;

        for (int i = 1; i <= aLen; i++)
        {
            distance[i, 0] = i;
        }

        for (int j = 0; j <= bLen; j++)
        {
            distance[0, j] = j;
        }



        //Computes the edit distance
        for (int i = 1; i <= aLen; i++)
        {
            for (int j = 1; j <= bLen; j++)
            {
                int x = distance[i - 1, j - 1] + ((a[i - 1] == b[j - 1]) ? 0 : 1);
                int y = distance[i - 1, j] + 1;
                int z = distance[i, j - 1] + 1;
                distance[i, j] = Math.Min(x, Math.Min(y, z));

            }
        }

        return distance[aLen, bLen];
    }


    public static string[] RemoveScape(string text)
    {
        return text.Split(new char[] { ' ', '\n', '\t' });
    }

    public static string Trim(string term)
    {
        int lIndex = 0;
        while (lIndex < term.Length && (char.IsLetterOrDigit(term[lIndex]) == false))
        {
            lIndex++;
        }

        int rIndex = term.Length - 1;
        while (rIndex > lIndex && char.IsLetterOrDigit(term[rIndex]) == false)
        {
            rIndex--;
        }

        return term.Substring(lIndex, rIndex - lIndex + 1);
    }
    //A method used for tokenize both queries and text
    public static string[] Tokenize(string text)
    {
        string[] terms = RemoveScape(text);

        string[] tokens = new string[terms.Length];

        int counter = 0;

        foreach (string term in terms)
        {
            string token = Trim(term);
            tokens[counter++] = token;
        }

        Array.Resize(ref tokens, counter);

        return tokens;

    }
    private string fullText;
    public string Title { get; private set; }



    //Max frequency of a a term in the document;
    int maxFrequency = 1;

    //Here is stored the ammount of documents that contains a given term
    public static Dictionary<String, int> s_globalFreq = new Dictionary<String, int>();




    //The collections of all the documents
    public static List<Document> DocumentCollection = new List<Document>();

    //Cardinal of the document collecion
    static int documentsCnt = 0;




    public Dictionary<string, TermData> Data;


    //The set of all words in the corpus
    static HashSet<string> s_termSet = new HashSet<String>();
    public Document(string title, string fullText)
    {

        this.Data = new Dictionary<string, TermData>();

        this.fullText = fullText;

        this.trimmedWords = RemoveScape(fullText);

        fullText = fullText.ToLower();


        string[] wordList = Tokenize(fullText);

        this.Title = title;


        foreach (string term in wordList)
        {
            s_termSet.Add(term);

            if (Data.ContainsKey(term))
            {
                Data[term].frequency++;


                int wordFreq = Data[term].frequency;

                if (wordFreq > maxFrequency)
                {
                    maxFrequency = wordFreq;
                }
            }
            else
            {
                Data.Add(term, new TermData(0, 1));

                if (s_globalFreq.ContainsKey(term))
                {
                    s_globalFreq[term]++;
                }
                else
                {
                    s_globalFreq.Add(term, 1);
                }

            }
        }


        FillPostingList(wordList);

        documentsCnt++;

        foreach (string term in Data.Keys)
        {
            Data[term].TF = (Double)Data[term].frequency / this.maxFrequency;
        }
        calcTF();

        DocumentCollection.Add(this);

    }

    public double GetTF(string term)
    {
        if (Data.ContainsKey(term))
            return Data[term].TF;
        return 0;
    }
    // Calcs the Term Frequency
    public void calcTF()
    {
        foreach (string key in Data.Keys)
        {
            Data[key].TF = (Double)Data[key].frequency / this.maxFrequency;
        }
    }

    //Calcs the Inverse Document Frequency
    public static double CalcIDF(string term)
    {
        int cnt = 0;

        if (s_globalFreq.ContainsKey(term))
        {
            cnt = s_globalFreq[term];
            return Math.Log2((double)documentsCnt / cnt);
        }

        return 0;
    }

    public double GetWeigth(string term)
    {
        return GetTF(term) * CalcIDF(term);
    }


    public static List<Tuple<string, string, double>> queryVector(string[] terms, List<string> excludedTerms,
    List<string> mandatoryTerms,
    List<Tuple<string, int>> relevantTerms)
    {

        //This dictionary sholds the frequency of each term on the query
        Dictionary<String, int> queryFreq = new Dictionary<String, int>();

        int maxL = 1;

        for (int i = 0; i < terms.Length; i++)
        {
            string term = terms[i];

            if (queryFreq.ContainsKey(term))
            {
                queryFreq[term]++;

                maxL = (maxL < queryFreq[term]) ? queryFreq[term] : maxL;
            }
            else
            {
                queryFreq.Add(term, 1);
            }

        }

        List<Tuple<string, string, double>> results = new List<Tuple<string, string, double>>();


        foreach (Document doc in DocumentCollection)
        {
            bool skip = false;
            foreach (string term in excludedTerms)
            {
                if (doc.ContainsTerm(term))
                {
                    skip = true;
                    break;

                }
            }
            if (skip) continue;

            foreach (string term in mandatoryTerms)
            {
                if (!doc.ContainsTerm(term))
                {
                    skip = true;
                    break;
                }
            }

            if (skip) continue;

            double dotProd = 0;

            double queryNorm = 0;
            double docNorm = 0;

            foreach (string term in doc.Data.Keys)
            {
                double w = doc.GetWeigth(term);
                docNorm += w * w;
            }

            docNorm = Math.Sqrt(docNorm);

            foreach (string term in queryFreq.Keys)
            {

                double freq = queryFreq[term];

                double query_tf = freq / maxL;

                double query_idf = CalcIDF(term);

                double query_weigth = query_tf * query_idf;

                double doc_weigth = doc.GetWeigth(term);

                dotProd += (doc_weigth * query_weigth);

                queryNorm += (query_weigth * query_weigth);

            }

            queryNorm = Math.Sqrt(queryNorm);

            double normProd = (docNorm * queryNorm);

            double cosin = (double)dotProd / normProd;

            if (!double.IsNaN(cosin) && cosin != 0)
            {
                Tuple<string, string, double> tuple = new Tuple<string, string, double>(doc.Title, doc.GetSnippet(terms), (cosin));
                results.Add(tuple);
            }

        }

        return results;

    }


    private List<Tuple<int, int>> MergeList(List<Tuple<int, int>> l1, List<Tuple<int, int>> l2)
    {
        List<Tuple<int, int>> sortedList = new List<Tuple<int, int>>();

        int i = 0;
        int j = 0;

        while (i < l1.Count && j < l2.Count)
        {
            if (l1[i].Item1 <= l2[j].Item1)
            {
                sortedList.Add(l1[i++]);
            }
            else
            {
                sortedList.Add(l2[j++]);
            }
        }
        while (i < l1.Count)
        {
            sortedList.Add(l1[i++]);
        }

        while (j < l2.Count)
        {
            sortedList.Add(l2[j++]);
        }

        return sortedList;

    }

    private string GetSnippet(string[] terms)
    {
        //Remove duplicate elements??
        terms = terms.Distinct().ToArray();


        List<Tuple<int, int>> SortedList = new List<Tuple<int, int>>();


        int counter = 0;

        foreach (string term in terms)
        {
            if (Data.ContainsKey(term))
            {
                List<Tuple<int, int>> auxList = new List<Tuple<int, int>>();

                List<int> termList = Data[term].Positions;

                foreach (int index in termList)
                {
                    auxList.Add(new Tuple<int, int>(index, counter));
                }

                SortedList = MergeList(SortedList, auxList);

            }

            counter++;
        }


        int[] freq = new int[counter];


        Queue<Tuple<int, int>> queue = new Queue<Tuple<int, int>>();


        int startIdx = 0;
        int bestCnt = 0;

        int onQueue = 0;

        foreach (Tuple<int, int> item in SortedList)
        {
            int idx = item.Item1;
            int id = item.Item2;

            if (freq[id] == 0) onQueue++;

            freq[id]++;

            queue.Enqueue(item);

            while (true)
            {
                System.Diagnostics.Debug.Assert(queue.Count != 0, "Hola Mundo");
                Tuple<int, int> auxItem = queue.Peek();
                if (freq[auxItem.Item2] > 1)
                {
                    freq[auxItem.Item2]--;
                    queue.Dequeue();
                }
                else break;
            }

            if (onQueue > bestCnt && queue.Count <= 20)
            {
                bestCnt = onQueue;
                startIdx = queue.Peek().Item1;
            }

        }


        string snippet = "";


        int endIdx = (fullText.Length - startIdx < 40) ? fullText.Length - startIdx : 40;



        int addedWords = 0;
        for (; startIdx < trimmedWords.Length && addedWords <= 20; startIdx++)
        {
            snippet += trimmedWords[startIdx] + " ";
            addedWords++;

        }

        return snippet;

    }
    public static void FillWeigths()
    {
        foreach (string term in Document.s_globalFreq.Keys)
        {
            foreach (Document doc in Document.DocumentCollection)
            {
                if (doc.Data.ContainsKey(term))
                {
                    doc.Data[term].Weigth = doc.GetWeigth(term);
                }

            }
        }
    }

    public bool ContainsTerm(string term)
    {
        return this.Data.ContainsKey(term);
    }
    private void FillPostingList(string[] wordlist)
    {
        for (int index = 0; index < wordlist.Length; index++)
        {
            string term = wordlist[index];

            Data[term].AddPos(index);

        }
    }
}

public class TermData
{
    public double tf;
    public double TF { get { return tf; } set { tf = value; } }



    public double Weigth { get; set; }

    public int frequency;

    public List<int> Positions;

    public TermData(double tf = 0, int frequency = 0)
    {
        this.TF = tf;
        this.frequency = frequency;
        this.Positions = new List<int>();
    }

    public void AddPos(int position)
    {
        this.Positions.Add(position);

    }
}