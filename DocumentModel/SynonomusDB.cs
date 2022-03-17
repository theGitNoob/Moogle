
using System.Text.Json;

namespace DocumentModel
{
    //
    // Summary:
    //     Class holding a database for synonomus
    //
    public static class SynonomusDB
    {

        //Holds the position of each term on the distincts groups of synonomus
        private static Dictionary<string, List<int>> s_synonomusPositions = new Dictionary<string, List<int>>();

        //Holds the distinct groups of synonomus
        private static List<List<string>> s_syns = new List<List<string>>();

        //
        // Summary: 
        //      Builds the  synonomus dictionary
        //
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
                    string auxTerm = TermUtils.Trim(syn.ToString()).ToLower();

                    l.Add(auxTerm);

                    if (s_synonomusPositions.ContainsKey(auxTerm))
                    {
                        s_synonomusPositions[auxTerm].Add(index);
                    }
                    else
                    {
                        s_synonomusPositions.Add(auxTerm, new List<int> { index });
                    }
                }

                s_syns.Add(l);

                index++;
            }
        }


        //
        // Summary: 
        //      Obtains all the synonomus of a term
        // Returns:
        //      An string array with all the synonomus
        //
        public static string[] GetSynonomus(string term)
        {
            if (!s_synonomusPositions.ContainsKey(term)) return new string[0];

            List<int> positions = s_synonomusPositions[term];

            List<string> syns = new List<string>();

            foreach (int pos in positions)
            {
                syns.AddRange(s_syns[pos]);
            }

            syns = syns.Distinct().ToList();
            syns.Remove(term);

            return syns.ToArray();
        }
    }

}