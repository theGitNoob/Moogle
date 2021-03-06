using System.Text;
namespace DocumentModel;
using Stemmer;
public class Document
{

    //
    // Summary:
    //      The sum of the components
    //
    protected double Sum { get; set; }

    //
    // Summary:
    //      The norm of the vectorized document
    //
    public double Norm { get { return Math.Sqrt(Sum); } }

    //
    // Summary:
    //      All the terms of the document
    //
    private string[] _fullTerms;

    //
    // Summary:
    //      The Title of the document
    //
    public string Title { get; set; }

    //
    // Summary:
    //      Max frequency of all terms in the document
    //
    protected int MaxFrequency { get; set; }

    //
    // Summary:
    //      Holds the information of each term on the document
    //
    protected Dictionary<string, TermData> Data;


    //
    // Summary:
    //      Default constructor for 
    //
    protected Document()
    {
        this.Data = new Dictionary<string, TermData>();
        this.Title = "";
        this._fullTerms = new String[0];
    }

    //
    // Summary:
    //      Creates a new document
    //
    // Parameters:
    //   title:
    //     The title of the document
    //   fullText:
    //     The full text of the document 
    //   collection:
    //      The collection of all the documents
    public Document(string title, string fullText)
    {

        this.Data = new Dictionary<string, TermData>();

        string[] wordList = TermUtils.Tokenize(fullText);

        this._fullTerms = new string[wordList.Length];

        this.Title = title;

        SaveOriginalTerms(fullText);

        fullText = fullText.ToLower();

        Index(wordList);

        FillPostionsList();

        CalcTF();

    }
    /// <summary>
    ///     Adds each term of the document
    /// </summary>
    private void Index(string[] wordList)
    {
        foreach (string term in wordList)
        {
            string root = Stemmer.Stemm(term);

            AddTerm(term);

            if (root != term)
            {
                AddTerm(root);
            }
        }
    }


    /// <summary>
    ///     Save the original terms before changing them
    /// </summary>
    private void SaveOriginalTerms(string text)
    {
        string[] terms = TermUtils.RemoveScape(text);

        int counter = 0;

        foreach (string term in terms)
        {
            string token = TermUtils.Trim(term);

            if (token.Length == 0) continue;

            this._fullTerms![counter++] = term;
        }

    }

    //
    // Summary:
    //      Adds the term to the document and to the DocumentCollectin, also update the max frequency
    //
    private void AddTerm(string term)
    {
        if (Data.ContainsKey(term))
        {
            Data[term].Frequency++;

            int wordFreq = Data[term].Frequency;

            if (wordFreq > MaxFrequency)
            {
                MaxFrequency = wordFreq;
            }
        }
        else
        {
            Data.Add(term, new TermData(0, 1));

            DocumentCollection.AddTerm(term);

        }
    }

    //
    // Summary:
    //      Retrieves the stored TF
    //
    protected double GetTF(string term)
    {
        if (this.ContainsTerm(term))
            return Data[term].TF;

        return 0;
    }

    //
    // Summary:
    //      Calcs the Term Frequency
    //
    protected void CalcTF()
    {
        foreach (string key in Data.Keys)
        {
            Data[key].TF = (Double)Data[key].Frequency / this.MaxFrequency;
        }
    }

    //
    // Summary: 
    //      Adds the weigth of the term
    //
    public void AddWeigth(string term, double weigth)
    {
        this.Sum += weigth * weigth;
        this.Data[term].Weigth = weigth;
    }

    //
    // Summary:
    //     Calcs the weight of a given term
    // Returns:
    //      A double representing the weigth of the term
    //
    public double CalcWeigth(string term, double idf)
    {
        return GetTF(term) * idf;
    }


    //
    // Summary:
    //     Merges two sorted lists, resulting on a third sorted list
    // Returns:
    //      A sorted list `List<Tuple<int, int>>` 
    //
    private static List<Tuple<int, int>> MergeLists(List<Tuple<int, int>> l1, List<Tuple<int, int>> l2)
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

    //
    // Summary:
    //     Sorts the all the position lists containing the positions on the terms to be retrieved
    //      on the snipper
    // Returns:
    //      An integer representing the amount of different
    //
    private void SortPositions(string[] terms, ref List<Tuple<int, int>> SortedList)
    {
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

                SortedList = MergeLists(SortedList, auxList);

            }
            counter++;
        }
    }

    //
    // Summary:
    //     Retrives a snippet of the document given certains terms
    // Returns:
    //      A string with at most 20 term containing the maximun amount of terms without the lose
    //      of document consistence
    //
    public string GetSnippet(string[] terms)
    {
        //Remove duplicate elements
        terms = terms.Distinct().ToArray();

        List<Tuple<int, int>> SortedList = new List<Tuple<int, int>>();

        SortPositions(terms, ref SortedList);

        //Stores the amount of elements with the same id on the queue

        int[] freq = new int[terms.Length];

        Queue<Tuple<int, int>> queue = new Queue<Tuple<int, int>>();


        int startIdx = 0;

        int bestCnt = 0;

        int onQueue = 0;

        //Adds elements to the queue and only extracts them while teir frequency is bigger than `one`
        foreach (Tuple<int, int> item in SortedList)
        {
            int idx = item.Item1;

            int id = item.Item2;

            if (freq[id] == 0) onQueue++;

            freq[id]++;

            queue.Enqueue(item);

            while (true)
            {
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


        StringBuilder snippet = new StringBuilder();

        int addedWords = 0;

        // If possible adds the previos 5 characters to get some context
        if (startIdx - 5 >= 0) startIdx -= 5;

        for (; startIdx < _fullTerms.Length && addedWords <= 20; startIdx++)
        {
            string word = _fullTerms[startIdx];

            string trimmed = TermUtils.Trim(word).ToLower();

            string root = Stemmer.Stemm(trimmed);

            if (terms.Contains(trimmed) || terms.Contains(root))
            {
                snippet.Append($"{word}$$ ");
            }
            else
            {
                snippet.Append($"{word} ");
            }
            addedWords++;

        }

        return snippet.ToString();

    }

    //
    // Summary: 
    //      Checks if a term exist on the document
    // Returns:
    //      true is term is present, false otherwise
    //
    public bool ContainsTerm(string term)
    {
        return this.Data.ContainsKey(term);
    }


    //
    // Summary: 
    //      Given a list of term that should appear together it
    //      performs a search on the document an find te minimun window that contains
    //      all the terms
    // Returns:
    //      The size of the minimun window that contains all of the terms
    //

    public int FindClosestTerms(List<string[]> nearTerms)
    {

        //Determines wheter a the terms are close enough
        int maxDistance = 0;


        //The sum of all distances
        int totalDistance = 0;

        //Holds the frequency
        Dictionary<string, int> freq = new Dictionary<string, int>();

        Dictionary<int, int> freqById = new Dictionary<int, int>();

        //Procces each group individually
        foreach (var items in nearTerms)
        {
            int counter = 0;

            foreach (string term in items)
            {
                if (freq.ContainsKey(term))
                {
                    freq[term]++;
                }
                else
                {
                    freq.Add(term, 1);
                }
            }

            //Assigns an unique id
            var dic = items.Distinct().ToDictionary((key) => key, (key) => counter++);

            List<Tuple<int, int>> positions = new List<Tuple<int, int>>();

            bool calcDistance = true;

            foreach (var item in dic)
            {
                string term = item.Key;

                int id = item.Value;

                //Does not calculate the distance if any term of them doesn't exist on the document
                if (!this.Data.ContainsKey(term))
                {
                    calcDistance = false;
                    break;
                }


                //Only adds because each id are unique
                freqById.Add(id, freq[term]);

                maxDistance += 40;

                List<int> termPositions = this.Data[term].Positions;

                List<Tuple<int, int>> newPositions = new List<Tuple<int, int>>();

                foreach (int pos in termPositions)
                {
                    newPositions.Add(Tuple.Create(pos, id));
                }

                positions = MergeLists(positions, newPositions);
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

                    if (onQueue[firsItemId] > freqById[firsItemId])
                    {
                        queue.Dequeue();
                        onQueue[firsItemId]--;
                    }
                    else break;
                }
                if (cnt == counter)
                {
                    int currDistance = position - queue.Peek().Item1;

                    if (currDistance != 0)
                        best = Math.Min(best, currDistance);

                }
            }

            totalDistance += best;
        }

        return (totalDistance == 0 || maxDistance / totalDistance <= 0) ? 1 : maxDistance / totalDistance;

    }
    //
    // Summary: 
    //      Store the position of each term on the original document for later use
    //
    private void FillPostionsList()
    {
        for (int index = 0; index < this._fullTerms.Length; index++)
        {
            string term = this._fullTerms[index].ToLower();

            string trimmed = TermUtils.Trim(term);

            Data[Stemmer.Stemm(trimmed)].AddPos(index);
            Data[trimmed].AddPos(index);

        }
    }


}