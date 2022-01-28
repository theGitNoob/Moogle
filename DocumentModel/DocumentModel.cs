namespace DocumentModel;

public class Document
{


    //Get the misspell for a given term
    public static string getMisspell(string term)
    {
        //med stands for minimum edit distance
        double med = 0;
        string word = "";


        foreach (string key in termSet)
        {
            int ed = EditDistance(term, key);
            double f = 1 / (double)(ed + 1);

            double score = f * getIDF(key);
            if (key == "cuba" || key == "numa")
                Console.WriteLine($"IDF for term {key} is: {getIDF(key)}\nAnd score is {f}");
            if (score > med)
            {
                med = score;
                word = key;
            }
        }
        Console.WriteLine($"Best score is: {med} and word is: {word}");
        return word;
    }

    //Calcs the EditDistance between two words
    public static int EditDistance(string a, string b)
    {
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

    //A method used for tokenize both queries and text
    public static string[] Tokenize(string text)
    {
        string[] words = text.Split(new char[] { ' ', '\n', '\t', '.' });

        string[] tokens = new string[words.Length];

        int counter = 0;
        foreach (string word in words)
        {
            int lIndex = 0;
            while (lIndex < word.Length && (char.IsLetterOrDigit(word[lIndex]) == false))
            {
                lIndex++;
            }

            if (lIndex == word.Length) continue;

            int rIndex = word.Length - 1;
            while (rIndex > lIndex && char.IsLetterOrDigit(word[rIndex]) == false)
            {
                rIndex--;
            }

            string token = word.Substring(lIndex, rIndex - lIndex + 1);
            tokens[counter++] = token;
        }

        Array.Resize(ref tokens, counter);

        return tokens;

    }
    private const int maxSnippetLength = 20;
    private string fullText;
    public string Title { get; private set; }

    private Dictionary<String, List<int>> wordPos = new Dictionary<string, List<int>>();

    //The collections of all the documents
    public static List<Document> documentCollection = new List<Document>();

    //Max frequency of a a term in the document;
    int maxFrequency = 1;


    //Here is stored the ammount of documents that contains a given term
    public static Dictionary<String, int> termFreq = new Dictionary<String, int>();


    //Cardinal of the document collecion
    static int documentsCnt = 0;


    //Here is stored the frequency of a given term on the current document
    public Dictionary<string, int> frequency = new Dictionary<String, int>();



    //The set of all words in the corpus
    static HashSet<string> termSet = new HashSet<String>();
    public Document(string title, string fullText)
    {
        this.fullText = fullText;

        fullText = fullText.ToLower();

        string[] wordList = Tokenize(fullText);

        Title = title;


        foreach (string term in wordList)
        {
            termSet.Add(term);

            if (frequency.ContainsKey(term))
            {
                frequency[term]++;

                int wordFreq = frequency[term];

                if (wordFreq > maxFrequency)
                {
                    maxFrequency = wordFreq;
                }
            }
            else
            {
                if (termFreq.ContainsKey(term))
                {
                    termFreq[term]++;
                }
                else
                {
                    termFreq.Add(term, 1);
                }

                frequency.Add(term, 1);
            }
        }


        FillPostingList(wordList);

        foreach (string key in postingList.Keys)
        {

            foreach (int item in postingList[key])
            {
                System.Console.Write(item + " ");
            }
            Console.WriteLine();
        }

        documentsCnt++;
        documentCollection.Add(this);
    }


    // Calcs the Term Frequency
    public double calcTF(string term)
    {
        double freq = 0;


        //Gets the frequency of the term ${term} on the document if exists
        // otherwise returns 0;
        if (this.frequency.ContainsKey(term))
        {
            freq = this.frequency[term];
        }
        else
        {
            return 0;
        }

        return freq / this.maxFrequency;
    }

    //Calcs the Inverse Document Frequency
    public static double getIDF(string term)
    {
        int cnt = 0;

        if (termFreq.ContainsKey(term))
        {
            cnt = termFreq[term];
            //Change to Log2
            return Math.Log2((double)documentsCnt / cnt);
        }

        return 0;
    }


    public double getWeigth(string term)
    {
        return calcTF(term) * getIDF(term);
    }


    public static List<Tuple<string, string, double>> queryVector(string[] terms)
    {

        //This dictionary sholds the frequency of each term on the query
        Dictionary<String, int> qTF = new Dictionary<String, int>();

        int counter = 0;
        string[] auxArr = new string[terms.Length + 1];

        double[] qVector = new double[terms.Length + 1];

        int maxL = 1;
        for (int i = 0; i < terms.Length; i++)
        {

            string term = terms[i];

            // double a;

            if (qTF.ContainsKey(term))
            {
                qTF[term]++;

                //Change latter;
                maxL = (maxL < qTF[term]) ? qTF[term] : maxL;
            }
            else
            {
                auxArr[counter++] = term;
                qTF.Add(term, 1);
            }

        }

        string[] singleTerms = new string[counter];

        for (int i = 0; i < counter; i++)
        {
            singleTerms[i] = auxArr[i];
        }



        List<Tuple<string, string, double>> results = new List<Tuple<string, string, double>>();


        foreach (Document doc in documentCollection)
        {

            double dotProd = 0;

            double qNorm = 0;
            double docNorm = 0;

            foreach (string term in doc.frequency.Keys)
            {
                double w = doc.getWeigth(term);
                docNorm += w * w;
            }



            for (int i = 0; i < counter; i++)
            {
                string term = singleTerms[i];

                double freq = qTF[term];

                double query_tf = freq / maxL;

                double query_idf = getIDF(term);

                double query_weigth = query_tf * query_idf;

                double doc_weigth = doc.getWeigth(term);

                dotProd += (doc_weigth * query_weigth);

                qNorm += (query_weigth * query_weigth);

                int x = (doc.frequency.ContainsKey(term)) ? doc.frequency[term] : 0;



            }



            double normProd = (Math.Sqrt(docNorm) * Math.Sqrt(qNorm));



            double angle = (double)dotProd / normProd;



            if (!double.IsNaN(angle) && angle != 0)
            {
                Tuple<string, string, double> tuple = new Tuple<string, string, double>(doc.Title, doc.getSnippet(terms), (angle));
                results.Add(tuple);
            }

        }
        return results;

    }



    private string getSnippet(string[] terms)
    {
        string snippet = "";

        int counter = 0;
        int idx = -1;


        foreach (string term in terms)
        {
            idx = this.fullText.IndexOf(term, StringComparison.OrdinalIgnoreCase);
            if (idx != -1)
            {
                break;
            }
        }

        for (; idx < this.fullText.Length && counter < 20; idx++)
        {
            string auxWord = "";
            int j = idx;
            while (j < this.fullText.Length)
            {
                char currChar = this.fullText[j];

                auxWord += currChar;
                if (!Char.IsLetterOrDigit(currChar))
                {
                    break;
                }
                j++;
            }

            counter++;
            idx = j;
            snippet += auxWord;
        }

        if (idx < this.fullText.Length)
        {
            snippet += " ...";
        }
        return snippet;

    }

    //
    private Dictionary<string, List<int>> postingList = new Dictionary<string, List<int>>();

    private void FillPostingList(string[] wordlist)
    {
        for (int index = 0; index < wordlist.Length; index++)
        {
            string term = wordlist[index];

            if (postingList.ContainsKey(term))
            {
                postingList[term].Append(index);

            }
            else
            {
                postingList.Add(term, new List<int> { 1 });
            }
        }
    }
}
