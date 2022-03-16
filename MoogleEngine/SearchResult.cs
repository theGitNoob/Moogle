namespace MoogleEngine;

public class SearchResult
{
    private SearchItem[] _items;

    public SearchResult(SearchItem[] items, string suggestion = "")
    {
        if (items == null)
        {
            throw new ArgumentNullException("items");
        }
        Array.Sort(items);

        this._items = items;
        this.Suggestion = suggestion;
    }

    public SearchResult() : this(new SearchItem[0])
    {

    }

    public string Suggestion { get; private set; }

    public IEnumerable<SearchItem> Items()
    {
        return this._items;
    }

    public int Count { get { return this._items.Length; } }
}
