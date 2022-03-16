namespace MoogleEngine;

using DocumentModel;

public static class Moogle
{
    public static void StartIndex()
    {
        string ContentPath = Environment.GetEnvironmentVariable("CONTENT_PATH") ?? Directory.GetParent(Directory.GetCurrentDirectory())!.ToString() + "/Content/";

        SynonomusDB.BuildDic("../sinonimos.json");

        TermUtils.InitDistance();

        var files = Directory.EnumerateFiles(ContentPath, "*.txt");

        files = files.OrderBy(file => file);

        foreach (string fileName in files)
        {
            string fullText = ReadFile(fileName);

            Document doc = new Document(Path.GetFileNameWithoutExtension(fileName), fullText);

            DocumentCollection.Add(doc);
        }

        DocumentCollection.FillWeigths();

    }


    private static string ReadFile(string fileName)
    {
        StreamReader reader = new StreamReader(fileName);

        String fullText = reader.ReadToEnd();

        return fullText;

    }
    public static SearchResult Query(string query)
    {

        Query QueryItem = new Query(query);

        var result = QueryItem.GetResults();

        SearchItem[] items = new SearchItem[result.Count];

        for (int i = 0; i < result.Count; i++)
        {
            Tuple<string, string, double> t = result[i];

            items[i] = new SearchItem(t.Item1, t.Item2, t.Item3);
        }

        Array.Sort(items);

        string suggestion = QueryItem.GetSugestion();

        return new SearchResult(items, suggestion.Trim() == query.ToLower().Trim() ? "" : suggestion);
    }
}
