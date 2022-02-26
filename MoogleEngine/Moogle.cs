using DocumentModel;
using System.Text;

namespace MoogleEngine;

public static class Moogle
{


    public static void StartIndex()
    {
        // Console.WriteLine(Environment.GetEnvironmentVariables()["CONTENT_PATH"]);

        Document.BuildDic("../sinonimos.json");

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
        List<string[]> nearTerms = new List<string[]>();

        for (int idx = 0; idx < unescapedWord.Length; idx++)
        {
            string term = unescapedWord[idx];

            if (term.Length == 0) continue;

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

        foreach (string cad in unescapedWord)
        {
            string[] nearby = cad.Split("~").Distinct().ToArray();

            if (nearby.Length == 1) continue;

            StringBuilder auxCad = new StringBuilder();

            foreach (string term in nearby)
            {
                auxCad.Append(term + " ");
            }

            query = query.Replace(cad, auxCad.ToString());

            nearTerms.Add(nearby);
        }

        excludedTerms = excludedTerms.Distinct().ToList();
        mandatoryTerms = mandatoryTerms.Distinct().ToList();
        relevantTerms = relevantTerms.Distinct().ToList();

        //Tokenized query terms
        string[] terms = Document.Tokenize(query);

        var result = Document.queryVector(terms, excludedTerms, mandatoryTerms, relevantTerms, nearTerms);

        SearchItem[] items = new SearchItem[result.Count];

        for (int i = 0; i < result.Count; i++)
        {
            Tuple<string, string, double> t = result[i];

            items[i] = new SearchItem(t.Item1, t.Item2, t.Item3);
        }


        StringBuilder newQuery = new StringBuilder();

        bool flag = false;

        foreach (string term in terms)
        {
            string misspell = Document.GetMisspell(term);


            int misspellFreq = 0;
            int termFreq = 0;
            if (Document.s_globalFreq.ContainsKey(term))
            {

                termFreq = Document.s_globalFreq[term];
            }
            else
            {
                newQuery.Append(misspell + " ");
                flag = true;
                continue;
            }


            if (Document.s_globalFreq.ContainsKey(misspell) && misspellFreq > termFreq && termFreq < 5)
            {
                misspellFreq = Document.s_globalFreq[misspell];
                newQuery.Append(misspell + " ");
                flag = true;
            }
            else newQuery.Append(term + " ");


        }

        if (newQuery.Length != 0)
            newQuery.Remove(newQuery.Length - 1, 1);
        Array.Sort(items);

        return new SearchResult(items, (flag) ? newQuery.ToString() : "");
    }
}
