namespace Stemmer;

public static class SnowBallData
{
    public static List<char> Vowels = new List<char>()
    {
        'a', 'e', 'i', 'o', 'u', 'á' ,'é' ,'í' ,'ó' ,'ú' ,'ü'
    };
    public static List<string> Step0 = new List<string>()
    {
        "selas", "selos","sela", "selo", "las", "les", "los", "nos","me", "se", "la", "le", "lo"
    };
    public static List<string> AfterStep0 = new List<string>()
    {
        "iéndo", "ándo", "ár", "ér", "ír", "ando", "iendo", "ar", "er", "ir", "yendo"
    };
    public static List<List<string>> Step1 = new List<List<string>>()
    {
        new List<string>(){"anza","anzas","ico","ica","icos","icas","ismo","ismos","able","ables","ible","ibles","ista","istas","oso","osa","osos","osas","amiento","amientos","imiento","imientos"},
        new List<string>(){"adora" ,"ador","ación" ,"adoras" ,"adores" ,"aciones" ,"ante" ,"antes" ,"ancia" ,"ancias"},
        new List<string>(){"logía", "logías"},
        new List<string>(){"ución", "uciones"},
        new List<string>(){"encia", "encias"},
        new List<string>(){"amente"},
        new List<string>(){"mente"},
        new List<string>(){"idad", "idades"},
        new List<string>(){"iva", "ivo", "ivas", "ivos"}
    };
    public static List<string> Step2_a = new List<string>()
    {
        "yeron", "yendo", "yamos", "yais", "yan", "yen", "yas", "yes", "ya", "ye", "yo", "yó"
    };
    public static List<string> Step2_b1 = new List<string>()
    {
        "en", "es", "éis", "emos"
    };
    public static List<string> Step2_b2 = new List<string>()
    {
        "arían","arías","arán","arás","aríais","aría","aréis","aríamos","aremos","ará","aré","erían","erías","erán","erás","eríais","ería","eréis","eríamos","eremos","erá","eré","irían","irías","irán","irás","iríais","iría","iréis","iríamos","iremos","irá","iré","aba","ada","ida","ía","ara","iera","ad","ed","id","ase","iese","aste","iste","an","aban","ían","aran","ieran","asen","iesen","aron","ieron","ado","ido","ando","iendo","ió","ar","er","ir","as","abas","adas","idas","ías","aras","ieras","ases","ieses","ís","áis","abais","íais","arais","ierais","  aseis","ieseis","asteis","isteis","ados","idos","amos","ábamos","íamos","imos","áramos","iéramos","iésemos","ásemos"
    };
    public static List<string> Step3_a = new List<string>()
    {
        "os", "a", "o", "á", "í", "ó"
    };
    public static List<string> Step3_b = new List<string>()
    {
        "e", "é"
    };
}
