namespace MoogleEngine;

public class SearchItem : IComparable
{

    public int CompareTo(Object obj)
    {
        if (obj == null) return 1;

        SearchItem aux = obj as SearchItem;

        return (this.Score >= aux.Score) ? -1 : 1;

    }
    public SearchItem(string title, string snippet, double score)
    {
        this.Title = title;
        this.Snippet = snippet;
        this.Score = score;
    }

    public static bool operator <=(SearchItem a, SearchItem b)
    {

        return a.Score <= b.Score;

    }
    public static bool operator >(SearchItem a, SearchItem b)
    {
        return a.Score > b.Score;
    }

    public static bool operator <(SearchItem a, SearchItem b)
    {

        return a.Score < b.Score;

    }
    public static bool operator >=(SearchItem a, SearchItem b)
    {
        return a.Score >= b.Score;
    }

    public string Title { get; private set; }

    public string Snippet { get; private set; }

    public double Score { get; private set; }
}
