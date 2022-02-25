
namespace DocumentModel;

using Stemmer;
using System.Text.Json;
using System.Collections;

public class Document
{

    private static Dictionary<string, List<int>> SynonomusPositions = new Dictionary<string, List<int>>();

    private static List<List<string>> Syns = new List<List<string>>();

    string[] trimmedWords;

    //Get the misspell for a given term
    public static string getMisspell(string term)
    {

        //TODO:Establecer un minimo de cambios que puedo hacer en dependencia de el tamaño de la palabra, ej: leon máximo dos cambios
        //med stands for minimum edit distance
        double med = 0;
        string word = "";


        foreach (string key in s_globalFreq.Keys)
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
            if (token.Length == 0) continue;
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
    static int DocumentsCnt { get { return DocumentCollection.Count; } set { } }

    public Dictionary<string, TermData> Data;

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
            string root = Stemmer.Stemm(term);

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

            if (root != term)
            {
                if (Data.ContainsKey(root))
                {
                    Data[root].frequency++;


                    int wordFreq = Data[root].frequency;

                    if (wordFreq > maxFrequency)
                    {
                        maxFrequency = wordFreq;
                    }
                }
                else
                {
                    Data.Add(root, new TermData(0, 1));

                    if (s_globalFreq.ContainsKey(root))
                    {
                        s_globalFreq[root]++;
                    }
                    else
                    {
                        s_globalFreq.Add(root, 1);
                    }

                }
            }
        }

        FillPostingList(wordList);

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
            return Math.Log2((double)DocumentsCnt / cnt);
        }

        return 0;
    }

    public double GetWeigth(string term)
    {
        return GetTF(term) * CalcIDF(term);
    }


    public static List<Tuple<string, string, double>> queryVector(string[] terms, List<string> excludedTerms,
    List<string> mandatoryTerms,
    List<Tuple<string, int>> relevantTerms, List<string[]> nearTerms)
    {

        bool showSyns = nearTerms.Count == 0;

        //Augment relevant terms
        List<Tuple<string, int>> auxList = new List<Tuple<string, int>>();


        if (showSyns)
        {
            foreach (Tuple<string, int> item in relevantTerms)
            {
                string[] syns = GetSynonomus(item.Item1);

                foreach (string syn in syns)
                {
                    if (s_globalFreq.ContainsKey(syn) && item.Item2 > 1)
                    {
                        auxList.Add(Tuple.Create(syn, item.Item2 - 1));
                    }
                }
            }
        }

        //Remove elements not in corpus
        relevantTerms.RemoveAll((elem) => !s_globalFreq.ContainsKey(elem.Item1));
        relevantTerms.AddRange(auxList.Distinct());

        //This dictionary holds the frequency of each term on the query
        Dictionary<String, int> queryFreq = new Dictionary<String, int>();

        int maxL = 1;


        //Used to check wheter the normal form of a term is given on the query
        HashSet<String> normalWords = new HashSet<string>();

        //Used to check wheter the root of a term is given on the query
        HashSet<String> stemmedWords = new HashSet<string>();

        //Used to check wheter the synonomous of a term is given on the query
        HashSet<String> relatedWords = new HashSet<string>();

        foreach (string term in terms)
        {
            if (s_globalFreq.ContainsKey(term))
            {
                if (queryFreq.ContainsKey(term))
                {
                    queryFreq[term]++;
                    maxL = (maxL < queryFreq[term]) ? queryFreq[term] : maxL;
                }
                else
                {
                    queryFreq.Add(term, 1);
                    normalWords.Add(term);
                }
            }
        }

        //Augment the query with the root of each term if thet exists on corpus if closeness operator is not present
        if (showSyns)
        {
            foreach (string term in terms)
            {
                string root = Stemmer.Stemm(term);

                if (s_globalFreq.ContainsKey(root))
                {
                    if (queryFreq.ContainsKey(root))
                    {
                        queryFreq[root]++;
                        maxL = (maxL < queryFreq[root]) ? queryFreq[root] : maxL;
                    }
                    else
                    {
                        queryFreq.Add(root, 1);
                        if (!normalWords.Contains(root))
                            stemmedWords.Add(term);
                    }
                }
            }
        }

        //Populate with synonyms if they exists on corpus if closeness operator is not present
        if (showSyns)
        {
            foreach (string term in terms)
            {
                string[] syns = GetSynonomus(term);

                foreach (string syn in syns)
                {
                    if (s_globalFreq.ContainsKey(syn))
                    {
                        if (queryFreq.ContainsKey(syn))
                        {
                            queryFreq[syn]++;
                            maxL = (maxL < queryFreq[syn]) ? queryFreq[syn] : maxL;
                        }
                        else
                        {
                            queryFreq.Add(syn, 1);
                            if (normalWords.Contains(syn) == false && stemmedWords.Contains(term) == false)
                                relatedWords.Add(syn);
                        }
                    }
                }
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

                int idx = relevantTerms.FindIndex((val) => val.Item1 == term);

                double relevance = idx == -1 ? 1 : relevantTerms[idx].Item2 + 1;

                double freq = queryFreq[term];

                double query_tf = freq / maxL;

                double query_idf = CalcIDF(term);

                bool isRoot = normalWords.Contains(term) == false && stemmedWords.Contains(term) == true;
                bool isSyn = isRoot == false && normalWords.Contains(term) == false && relatedWords.Contains(term) == true;

                double query_weigth = query_tf * query_idf * (relevance);

                if (isRoot)
                {
                    query_weigth *= 0.5;
                }
                else if (isSyn)
                {
                    query_weigth *= 0.4;
                }

                double doc_weigth = doc.GetWeigth(term);

                dotProd += (doc_weigth * query_weigth);

                queryNorm += (query_weigth * query_weigth);
            }


            int multiplier = FindClosestScore(nearTerms, doc);

            queryNorm = Math.Sqrt(queryNorm);

            double normProd = (docNorm * queryNorm);

            double cosin = (double)dotProd / normProd;

            cosin *= multiplier;

            if (!double.IsNaN(cosin) && cosin != 0)
            {
                Tuple<string, string, double> tuple = new Tuple<string, string, double>(doc.Title, doc.GetSnippet(terms), (cosin));
                results.Add(tuple);
            }

        }

        return results;

    }


    private static List<Tuple<int, int>> MergeList(List<Tuple<int, int>> l1, List<Tuple<int, int>> l2)
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

    private static int FindClosestScore(List<string[]> nearTerms, Document doc)
    {
        int MaxDistance = 0;
        int totalDistance = 0;

        foreach (var items in nearTerms)
        {
            int counter = 0;

            var dic = items.ToDictionary((key) => key, (key) => counter++);

            List<Tuple<int, int>> positions = new List<Tuple<int, int>>();

            bool calcDistance = true;

            foreach (var item in dic)
            {

                string term = item.Key;

                if (!doc.Data.ContainsKey(term))
                {
                    calcDistance = false;
                    break;
                }

                MaxDistance += 20;

                List<int> termPositions = doc.Data[term].Positions;

                List<Tuple<int, int>> newPositions = new List<Tuple<int, int>>();

                foreach (int pos in termPositions)
                {
                    newPositions.Add(Tuple.Create(pos, item.Value));
                }

                positions = MergeList(positions, newPositions);
            }
            if (!calcDistance) continue;

            int[] onQueue = new int[counter];

            Queue<Tuple<int, int>> queue = new Queue<Tuple<int, int>>();

            int best = int.MaxValue;

            int cnt = 0;

            foreach (var item in positions)
            {
                int position = item.Item1;

                int id = item.Item2;

                queue.Enqueue(item);


                if (onQueue[id] == 0) cnt++;

                onQueue[id]++;

                while (true)
                {
                    int firsItemId = queue.Peek().Item2;

                    if (onQueue[firsItemId] > 1)
                    {
                        queue.Dequeue();
                        onQueue[firsItemId]--;
                    }
                    else break;
                }
                if (cnt == counter)
                {
                    best = Math.Min(best, position - queue.Peek().Item1);
                }
            }
            totalDistance += best;
        }

        return (totalDistance == 0) ? 1 : MaxDistance / totalDistance;

    }
    private void FillPostingList(string[] wordlist)
    {
        for (int index = 0; index < wordlist.Length; index++)
        {
            string term = wordlist[index];

            Data[term].AddPos(index);

        }
    }


    //Here is constructed the synonomus dictionary
    public static void BuildDic(string path)
    {
        StreamReader reader = new StreamReader(path);

        JsonDocument document = JsonDocument.Parse(reader.ReadToEnd());

        JsonElement root = document.RootElement;

        int index = 0;

        foreach (JsonElement arr in root.EnumerateArray())
        {
            List<string> l = new List<string>();

            foreach (JsonElement syn in arr.EnumerateArray())
            {
                string auxTerm = Trim(syn.ToString()).ToLower();

                l.Add(auxTerm);

                if (SynonomusPositions.ContainsKey(auxTerm))
                {
                    SynonomusPositions[auxTerm].Add(index);
                }
                else
                {
                    SynonomusPositions.Add(auxTerm, new List<int> { index });
                }
            }

            Syns.Add(l);

            index++;
        }
    }

    public static string[] GetSynonomus(string term)
    {
        if (!SynonomusPositions.ContainsKey(term)) return new string[0];

        List<int> positions = SynonomusPositions[term];

        List<string> syns = new List<string>();

        foreach (int pos in positions)
        {
            syns.AddRange(Syns[pos]);
        }

        return syns.ToArray();
    }
}