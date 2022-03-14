
using System.Text.Json;

namespace DocumentModel
{
    //
    // Summary:
    //     Class holding a database for synonomus
    //
    public static class SynonomusDB
    {

        private static Dictionary<string, List<int>> SynonomusPositions = new Dictionary<string, List<int>>();

        private static List<List<string>> Syns = new List<List<string>>();

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


        //
        // Summary: 
        //      Obtains all the synonomus of a term
        // Returns:
        //      An string array with all the synonomus
        //
        public static string[] GetSynonomus(string term)
        {
            if (!SynonomusPositions.ContainsKey(term)) return new string[0];

            List<int> positions = SynonomusPositions[term];

            List<string> syns = new List<string>();

            foreach (int pos in positions)
            {
                syns.AddRange(Syns[pos]);
            }

            syns = syns.Distinct().ToList();
            syns.Remove(term);

            return syns.ToArray();
        }
    }

}