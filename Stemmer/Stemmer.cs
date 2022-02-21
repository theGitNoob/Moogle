namespace Stemmer;

public static class Stemmer
{

    private static bool IsVowel(char letter)
    {
        return SnowBallData.Vowels.Contains(letter);
    }

    private static bool IsConsonant(char letter)
    {
        return Char.IsLetter(letter) && !IsVowel(letter);
    }


    private static int GetRV(string term)
    {
        if (IsConsonant(term[1]))
        {
            for (int index = 2; index < term.Length; index++)
                if (IsVowel(term[index])) return index + 1;
        }
        else if (IsVowel(term[0]) && IsVowel(term[1]))
        {
            for (int index = 2; index < term.Length; index++)
            {
                if (IsConsonant(term[index])) return index + 1;
            }
        }
        else if (IsConsonant(term[0]) && IsVowel(term[1]) && term.Length > 3)
        {
            return 3;
        }

        return -1;
    }

    private static int GetR1(string term)
    {
        int index = 1;
        for (; index < term.Length; index++)
        {
            if (IsConsonant(term[index]) && IsVowel(term[index - 1])) { index++; break; };
        }
        return (index == term.Length) ? -1 : index;
    }
    private static int GetR2(string term)
    {
        int index = GetR1(term) + 1;

        if (index == -1) return -1;

        for (; index < term.Length; index++)
        {
            if (IsConsonant(term[index]) && IsVowel(term[index - 1])) { index++; break; };
        }
        return (index == term.Length) ? -1 : index;
    }

    private static string Normalize(string term)
    {
        string normalized = term;

        normalized = normalized.Replace("\u00E1", "a");
        normalized = normalized.Replace("\u00E9", "e");
        normalized = normalized.Replace("\u00ED", "i");
        normalized = normalized.Replace("\u00F3", "o");
        normalized = normalized.Replace("\u00FA", "u");

        return normalized;
    }

    private static string Step0(string term)
    {
        string stem = "";
        int rv = GetRV(term);
        foreach (string suffix in SnowBallData.Step0)
        {
            if (term.EndsWith(suffix))
            {
                foreach (string lSuffix in SnowBallData.AfterStep0)
                {
                    if (term.EndsWith(lSuffix + suffix) && rv <= term.Length - (lSuffix.Length + suffix.Length))
                    {
                        if (lSuffix == "yendo")
                        {
                            if (term[term.Length - (lSuffix.Length + suffix.Length + 1)] == 'u')
                                stem = term.Remove(term.Length - suffix.Length);
                        }
                        else
                        {
                            stem = term.Remove(term.Length - suffix.Length);
                        }
                    }
                }
            }
        }
        return (stem == "") ? term : Normalize(stem);
    }

    private static bool LiesOnInterval(int region, int index)
    {
        return region <= index;
    }


    private static string LongestSuffix(string term, List<string> l)
    {

        string longestSuffix = "";

        foreach (string suffix in l)
        {
            if (term.EndsWith(suffix) && longestSuffix.Length < suffix.Length)
            {
                longestSuffix = suffix;
            }

        }
        return longestSuffix;
    }
    private static bool Step1(ref string term)
    {
        int r1 = GetR1(term);
        int r2 = GetR2(term);

        string stem = "";

        for (int index = 0; index < SnowBallData.Step1.Count && stem == ""; index++)
        {
            List<string> l = SnowBallData.Step1[index];

            switch (index)
            {
                case 0:
                    if (r2 == -1) break;
                    string longestSuffix = LongestSuffix(term, l);

                    if (longestSuffix != "" && LiesOnInterval(r2, term.Length - longestSuffix.Length))
                    {
                        term = term.Remove(term.Length - longestSuffix.Length);
                        return true;
                    }
                    break;

                case 1:
                    if (r2 == -1) break;
                    longestSuffix = LongestSuffix(term, l);

                    if (longestSuffix != "" && LiesOnInterval(r2, term.Length - longestSuffix.Length))
                    {
                        if (term.EndsWith("ic" + longestSuffix) && LiesOnInterval(r2, term.Length - (longestSuffix.Length + 2)))
                            term = term.Remove(term.Length - (longestSuffix.Length + 2));
                        else
                            term = term.Remove(term.Length - longestSuffix.Length);

                        return true;
                    }
                    break;

                case 2:

                    if (r2 == -1) break;

                    longestSuffix = LongestSuffix(term, l);

                    if (longestSuffix != "" && LiesOnInterval(r2, term.Length - longestSuffix.Length))
                    {
                        term = term.Remove(term.Length - longestSuffix.Length);
                        term += "log";
                        return true;
                    }
                    break;
                case 3:

                    if (r2 == -1) break;

                    longestSuffix = LongestSuffix(term, l);

                    if (longestSuffix != "" && LiesOnInterval(r2, term.Length - longestSuffix.Length))
                    {
                        term = term.Remove(term.Length - longestSuffix.Length);
                        term += "u";
                        return true;
                    }

                    break;
                case 4:

                    if (r2 == -1) break;
                    longestSuffix = LongestSuffix(term, l);


                    if (longestSuffix != "" && LiesOnInterval(r2, term.Length - longestSuffix.Length))
                    {
                        term = term.Remove(term.Length - longestSuffix.Length);
                        term += "ente";
                        return true;
                    }

                    break;
                case 5:

                    if (r1 == -1) break;

                    longestSuffix = LongestSuffix(term, l);


                    if (longestSuffix != "" && LiesOnInterval(r1, term.Length - longestSuffix.Length))
                    {
                        if (term.EndsWith("iv" + longestSuffix) && LiesOnInterval(r2, term.Length - (longestSuffix.Length + 2)))
                        {
                            if (term.EndsWith("ativ" + longestSuffix) && LiesOnInterval(r2, term.Length - (longestSuffix.Length + 4)))
                            {
                                term = term.Remove(term.Length - (longestSuffix.Length + 4));
                            }
                            else
                            {
                                term = term.Remove(term.Length - (longestSuffix.Length + 2));
                            }

                        }
                        else if (LiesOnInterval(r2, term.Length - (longestSuffix.Length + 2)) && (term.EndsWith("os" + longestSuffix) || term.EndsWith("ic" + longestSuffix) || term.EndsWith("ad" + longestSuffix)))
                        {
                            term = term.Remove(term.Length - (longestSuffix.Length + 2));
                        }
                        else
                        {
                            term = term.Remove(term.Length - longestSuffix.Length);
                        }
                        return true;
                    }
                    break;

                case 6:

                    if (r2 == -1) break;
                    longestSuffix = LongestSuffix(term, l);


                    if (longestSuffix != "" && LiesOnInterval(r2, term.Length - longestSuffix.Length))
                    {
                        if (LiesOnInterval(r2, term.Length - (longestSuffix.Length + 4)) && (term.EndsWith("ante" + longestSuffix) || term.EndsWith("able" + longestSuffix) || term.EndsWith("ible" + longestSuffix)))
                        {
                            term = term.Remove(term.Length - (longestSuffix.Length + 4));
                        }
                        else
                        {
                            term = term.Remove(term.Length - longestSuffix.Length);
                        }
                        return true;
                    }
                    break;
                case 7:
                    if (r2 == -1) break;
                    longestSuffix = LongestSuffix(term, l);


                    if (longestSuffix != "" && LiesOnInterval(r2, term.Length - longestSuffix.Length))
                    {
                        if (LiesOnInterval(r2, term.Length - (longestSuffix.Length + 2)) && (term.EndsWith("ic" + longestSuffix) || term.EndsWith("iv" + longestSuffix)))
                        {
                            term = term.Remove(term.Length - (longestSuffix.Length + 2));
                        }
                        else if (LiesOnInterval(r2, term.Length - (longestSuffix.Length + 4)) && (term.EndsWith("abil" + longestSuffix)))
                        {
                            term = term.Remove(term.Length - (longestSuffix.Length + 4));
                        }
                        else
                        {
                            term = term.Remove(term.Length - longestSuffix.Length);
                        }
                        return true;
                    }
                    break;
                case 8:

                    if (r2 == -1) break;
                    longestSuffix = LongestSuffix(term, l);


                    if (longestSuffix != "" && LiesOnInterval(r2, term.Length - longestSuffix.Length))
                    {
                        if (LiesOnInterval(r2, term.Length - (longestSuffix.Length + 2)) && term.EndsWith("at" + longestSuffix))
                        {
                            term = term.Remove(term.Length - (longestSuffix.Length + 2));
                        }
                        else
                        {
                            term = term.Remove(term.Length - longestSuffix.Length);
                        }
                        return true;
                    }
                    break;
            }

        }

        return false;
    }

    private static bool Step2a(ref string term)
    {
        string longestSuffix = LongestSuffix(term, SnowBallData.Step2_a);

        int rv = GetRV(term);

        if (LiesOnInterval(rv, term.Length - longestSuffix.Length) && term.EndsWith("u" + longestSuffix))
        {
            term = term.Remove(term.Length - longestSuffix.Length);
            return true;
        }

        return false;

    }
    private static bool Step2b(ref string term)
    {
        int rv = GetRV(term);

        string longestSuffix = LongestSuffix(term, SnowBallData.Step2_b1);

        if (LiesOnInterval(rv, term.Length - longestSuffix.Length))
        {
            if (term.EndsWith("gu" + longestSuffix))
                term = term.Remove(term.Length - (longestSuffix.Length + 2));
            else
                term = term.Remove(term.Length - longestSuffix.Length);

            return true;
        }

        longestSuffix = LongestSuffix(term, SnowBallData.Step2_b2);

        if (LiesOnInterval(rv, term.Length - longestSuffix.Length))
        {
            term = term.Remove(term.Length - longestSuffix.Length);
            return true;

        }

        return false;

    }

    public static void Step3(ref string term)
    {
        int rv = GetRV(term);
        string longestSuffix = LongestSuffix(term, SnowBallData.Step3_a);

        if (LiesOnInterval(rv, term.Length - longestSuffix.Length))
        {
            term = term.Remove(term.Length - longestSuffix.Length);
            return;
        }

        longestSuffix = LongestSuffix(term, SnowBallData.Step3_b);

        if (LiesOnInterval(rv, term.Length - longestSuffix.Length))
        {
            if (term.EndsWith("gu" + longestSuffix) && LiesOnInterval(rv, term.Length - (longestSuffix.Length + 1)))
                term = term.Remove(term.Length - (longestSuffix.Length + 1));
            else
                term.Remove(term.Length, term.Length - longestSuffix.Length);

            return;

        }

    }
    public static string Stemm(string term)
    {

        term = Step0(term);

        bool flag = Step1(ref term);

        if (!flag)
        {
            flag = Step2a(ref term);
            if (!flag)
            {
                flag = Step2b(ref term);
            }

        }

        Step3(ref term);

        return term;
    }


}
