
namespace DocumentModel
{

    //
    // Summary:
    //     Class that contains utils to work with strings and terms
    //
    static public class TermUtils
    {
        //
        // Summary:
        //      Holds the distance between keyboard letters
        private static int[,]? s_keyDistance;

        //
        // Summary:
        //      Remove unwanted characters
        // Returns:
        //      An array without escape characters or whitespaces
        public static string[] RemoveScape(string text)
        {
            return text.Split(new char[] { ' ', '\n', '\t' }).Where((elem) => !String.IsNullOrWhiteSpace(elem)).ToArray();
        }

        //
        // Summary:
        //      Remove characeters at beginning and end of the term
        // Returns:
        //      A string with only alphanumeric characters on both of the extremes
        public static string Trim(string term)
        {
            int lIndex = 0;
            while (lIndex < term.Length && (char.IsLetterOrDigit(term[lIndex]) == false))
            {
                lIndex++;
            }

            int rIndex = term.Length - 1;
            while (rIndex > lIndex && char.IsLetterOrDigit(term[rIndex]) == false)
            {
                rIndex--;
            }

            return term.Substring(lIndex, rIndex - lIndex + 1);
        }

        //
        // Summary:
        //      Determine when a string consists only of letters
        // Returns:
        //      true when only letters are found, false otherwise
        public static bool IsAlpha(string term)
        {
            foreach (char letter in term)
            {
                if (!char.IsLetter(letter)) return false;
            }
            return true;
        }

        //
        // Summary:
        //     Return a given ID for letters in ascii range
        public static int GetId(char letter)
        {
            if (Char.IsAscii(letter))
                return letter - 'a';

            switch (letter)
            {
                case 'á':
                    return 26;
                case 'é':
                    return 27;
                case 'í':
                    return 28;
                case 'ó':
                    return 29;
                case 'ú':
                    return 30;
            }
            return -1;
        }

        //
        // Summary:
        //     Gets the cost between two given letters acording to their position on the `querty` keyboard layout
        public static int GetCost(char letter1, char letter2)
        {
            int id1 = GetId(letter1);
            int id2 = GetId(letter2);
            if (id1 == -1 || id2 == -1) return 2;

            return s_keyDistance![GetId(letter1), GetId(letter2)];

        }
        //
        // Summary:
        //     Calcs the cost between each possible pair of alphabet letters

        public static void InitDistance()
        {
            Dictionary<char, char[]> neighbors_of = new Dictionary<char, char[]>();

            neighbors_of['q'] = new char[] { 'w', 'a' };
            neighbors_of['w'] = new char[] { 'e', 's', 'a', 'q' };
            neighbors_of['e'] = new char[] { 'r', 'd', 's', 'w', 'é' };
            neighbors_of['r'] = new char[] { 't', 'f', 'd', 'e' };
            neighbors_of['t'] = new char[] { 'y', 'g', 'f', 'r' };
            neighbors_of['y'] = new char[] { 'u', 'h', 'g', 't' };
            neighbors_of['u'] = new char[] { 'i', 'j', 'h', 'y', 'ú' };
            neighbors_of['i'] = new char[] { 'o', 'k', 'j', 'u', 'í' };
            neighbors_of['o'] = new char[] { 'p', 'l', 'k', 'i', 'ó' };
            neighbors_of['p'] = new char[] { 'l', 'o' };

            neighbors_of['a'] = new char[] { 'q', 'w', 's', 'z', 'á' };
            neighbors_of['s'] = new char[] { 'w', 'e', 'd', 'x', 'z', 'a' };
            neighbors_of['d'] = new char[] { 'e', 'r', 'f', 'c', 'x', 's' };
            neighbors_of['f'] = new char[] { 'r', 't', 'g', 'v', 'c', 'd' };
            neighbors_of['g'] = new char[] { 't', 'y', 'h', 'b', 'v', 'f' };
            neighbors_of['h'] = new char[] { 'y', 'u', 'j', 'n', 'b', 'g' };
            neighbors_of['j'] = new char[] { 'u', 'i', 'k', 'm', 'n', 'h' };
            neighbors_of['k'] = new char[] { 'i', 'o', 'l', 'm', 'j' };
            neighbors_of['l'] = new char[] { 'o', 'p', 'k' };

            neighbors_of['z'] = new char[] { 'a', 's', 'x' };
            neighbors_of['x'] = new char[] { 's', 'd', 'c', 'z' };
            neighbors_of['c'] = new char[] { 'd', 'f', 'v', 'x' };
            neighbors_of['v'] = new char[] { 'f', 'g', 'b', 'c' };
            neighbors_of['b'] = new char[] { 'g', 'h', 'n', 'v' };
            neighbors_of['n'] = new char[] { 'h', 'j', 'm', 'b' };
            neighbors_of['m'] = new char[] { 'j', 'k', 'n' };

            neighbors_of['á'] = new char[] { 'a' };
            neighbors_of['é'] = new char[] { 'e' };
            neighbors_of['í'] = new char[] { 'i' };
            neighbors_of['ó'] = new char[] { 'o' };
            neighbors_of['ú'] = new char[] { 'u' };


            int count = neighbors_of.Keys.Count;

            s_keyDistance = new int[count, count];

            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    if (i != j) s_keyDistance[i, j] = 2;
                }
            }

            foreach (char key in neighbors_of.Keys)
            {
                foreach (char neighbor in neighbors_of[key])
                {
                    s_keyDistance[GetId(key), GetId(neighbor)] = 1;
                }
            }

        }
        //
        // Summary:
        //     Calcs the EditDistance between two words
        // Returns:
        //     The minimun cost of the allowed operations to make de two terms equals
        //
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
                distance[i, 0] = i * 2;
            }

            for (int j = 0; j <= bLen; j++)
            {
                distance[0, j] = j * 2;
            }

            //Computes the edit distance
            for (int i = 1; i <= aLen; i++)
            {
                for (int j = 1; j <= bLen; j++)
                {
                    int x = distance[i - 1, j - 1] + GetCost(a[i - 1], b[j - 1]);
                    int y = distance[i - 1, j] + 2;
                    int z = distance[i, j - 1] + 2;
                    distance[i, j] = Math.Min(x, Math.Min(y, z));

                }
            }

            return distance[aLen, bLen];
        }

        //
        // Summary:
        //      Tokenize the text
        //
        // Parameters:
        //   text:
        //     The text to be tokenized
        //
        // Returns:
        //     A string array with tokenized terms
        //
        public static string[] Tokenize(string text)
        {
            string[] terms = TermUtils.RemoveScape(text);

            string[] tokens = new string[terms.Length];

            int counter = 0;

            foreach (string term in terms)
            {
                string token = TermUtils.Trim(term).ToLower();

                if (token.Length == 0) continue;

                tokens[counter++] = token;
            }

            Array.Resize(ref tokens, counter);

            return tokens;
        }

    }


}