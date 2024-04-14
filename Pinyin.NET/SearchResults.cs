namespace Pinyin.NET;

public struct SearchResults
{
    public object Source { get; set; }
    public int Weight { get; set; }
    public bool[] CharMatchResults { get; set; }
}