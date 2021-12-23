using System.Text;
using System.Collections.Generic;
namespace MoogleEngine;
public static class Moogle
{

    static Dictionary<String, int> wordList = new Dictionary<String, int>();

    public static void StartIndex()
    {
        Console.WriteLine(Environment.GetEnvironmentVariables()["CONTENT_PATH"]);

        var files = Directory.EnumerateFiles("../Content", "*.txt");

        foreach (string file in files)
        {
            ReadFile(file);
        }

        Console.WriteLine(wordList.Count);

    }


    private static void ReadFile(string filename)
    {
        StreamReader reader = new StreamReader(filename);

        StreamWriter writer = new StreamWriter("pepe.txt");

        String fullText = reader.ReadToEnd();

        String[] words = fullText.Split(new char[] { ' ', ',', '.', ';' });

        foreach (string word in words)
        {
            String lowerWord = word.ToLower();
            if (wordList.ContainsKey(lowerWord))
            {
                wordList[lowerWord]++;
            }
            else
            {
                wordList.Add(lowerWord, 1);
            }
        }

        // foreach (KeyValuePair<string, int> pair in wordList)
        // {
        //     Console.WriteLine("The word: {0} appears {1} times on the collection", pair.Key, wordList[pair.Key]);
        // }

    }
    public static SearchResult Query(string query)
    {
        // Modifique este método para responder a la búsqueda

        SearchItem[] items = new SearchItem[3] {
            new SearchItem("Rae.txt", "Rae score is 21.2", 21.2f),
            new SearchItem("rockyou.txt", "My rockyou.txt score is 0.5", 0.5f),
            new SearchItem("pepe.txt", "My pepe.txt score is 0.1", 0.1f),
        };

        return new SearchResult(items, query);
    }
}
