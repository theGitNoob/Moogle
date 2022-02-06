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

        //Tokenized query terms
        string[] terms = Document.Tokenize(query);

        var result = Document.queryVector(terms);

        SearchItem[] items = new SearchItem[result.Count];

        for (int i = 0; i < result.Count; i++)
        {
            Tuple<string, string, double> t = result[i];

            items[i] = new SearchItem(t.Item1, t.Item2, t.Item3);
        }


        StringBuilder newQuery = new StringBuilder();

        foreach (string term in terms)
        {
            // Console.WriteLine($"Get misspell of {term}");
            string misspell = Document.getMisspell(term);


            int misspellFreq = 0;
            int termFreq = 0;
            if (Document.s_globalFreq.ContainsKey(term))
            {

                termFreq = Document.s_globalFreq[term];
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
