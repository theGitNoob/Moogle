using System;
using System.Collections.Generic;

namespace DocumentModel;


public class Document
{

    //The collections of all the documents
    public static List<Document> documentCollection = new List<Document>();
    String fullText = "";

    //Max frequency of a a term in the document;
    int maxFrequency = 1;


    //Here is stored the ammount of documents that contains a given term
    private static Dictionary<String, int> termFreq = new Dictionary<String, int>();


    //Cardinal of the document collecion
    static int documentsCnt = 0;


    //Here is stored the frequency of a given term on the current document
    public Dictionary<string, int> frequency = new Dictionary<String, int>();

    public Document(string[] wordList)
    {


        foreach (string term in wordList)
        {
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


        documentsCnt++;
        documentCollection.Add(this);
    }

    public double calcTF(string term)
    {
        double freq = 0;


        //Gets the frequency of the term ${term} on the document if exists
        // otherwise returns 0;
        if (frequency.ContainsKey(term))
        {
            freq = frequency[term];
        }
        else
        {
            return 0;
        }
        return freq / maxFrequency;
    }

    public static double calcIDF(string term)
    {
        int cnt = 0;

        if (termFreq.ContainsKey(term))
        {
            cnt = termFreq[term];
        }


        //Change to Log2
        return Math.Log10((double)documentsCnt / cnt);
    }

    public double calcTFIDF(string term)
    {
        return calcTF(term) * calcIDF(term);
    }


}
