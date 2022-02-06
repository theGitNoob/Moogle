using DocumentModel;
using System.Text;
namespace MoogleEngine;

public static class Moogle
{


    public static void StartIndex()
    {
        // Console.WriteLine(Environment.GetEnvironmentVariables()["CONTENT_PATH"]);

        var files = Directory.EnumerateFiles("../Content", "*.txt");

        files = files.OrderBy(file => file);

        foreach (string file in files)
        {
            ReadFile(file);
        }


        Document.FillWeigths();

    }

    private static void ReadFile(string filePath)
    {
        StreamReader reader = new StreamReader(filePath);

        String fullText = reader.ReadToEnd();

        Document doc = new Document(Path.GetFileName(filePath), fullText);

    }
    public static SearchResult Query(string query)
    {
        query = query.ToLower();

        string[] unescapedWord = Document.RemoveScape(query);

        List<string> excludedTerms = new List<string>();
        List<string> mandatoryTerms = new List<string>();
        List<Tuple<string, int>> relevantTerms = new List<Tuple<string, int>>();

        //TODO:Cercania
        // List<List<String>> cl

        for (int idx = 0; idx < unescapedWord.Length; idx++)
        {
            string term = unescapedWord[idx];

            string trimmed = Document.Trim(term);
            switch (term[0])
            {
                case '!':
                    excludedTerms.Add(trimmed);
                    break;
                case '*':
                    int counter = 0;
                    for (int j = 0; j < term.Length; j++)
                    {
                        if (term[j] == '*') counter++;
                        else break;
                    }
                    relevantTerms.Add(Tuple.Create(trimmed, counter));
                    break;
                case '^':
                    mandatoryTerms.Add(trimmed);
                    break;
            }
        }


        //Tokenized query terms
        string[] terms = Document.Tokenize(query);

        var result = Document.queryVector(terms, excludedTerms, mandatoryTerms, relevantTerms);

        SearchItem[] items = new SearchItem[result.Count];

        for (int i = 0; i < result.Count; i++)
        {
            Tuple<string, string, double> t = result[i];

            items[i] = new SearchItem(t.Item1, t.Item2, t.Item3);
        }


        StringBuilder newQuery = new StringBuilder();

        foreach (string term in terms)
        {
            string misspell = Document.getMisspell(term);


            int misspellFreq = 0;
            int termFreq = 0;
            if (Document.s_globalFreq.ContainsKey(term))
            {

                termFreq = Document.s_globalFreq[term];
            }
            else
            {
                newQuery.Append(misspell + " ");
                continue;
            }


            if (Document.s_globalFreq.ContainsKey(misspell) && misspellFreq > termFreq && termFreq < 5)
            {
                misspellFreq = Document.s_globalFreq[misspell];
                newQuery.Append(misspell + " ");
            }
            else newQuery.Append(term + " ");


        }

        newQuery.Remove(newQuery.Length - 1, 1);
        Array.Sort(items);

        return new SearchResult(items, newQuery.ToString() == query ? "" : newQuery.ToString());
    }
}
