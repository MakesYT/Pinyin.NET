namespace Pinyin.NET;
public struct SearchResults 
{
    public object Source { get; set; }
    public double Weight { get; set; }
    public bool[] CharMatchResults { get; set; }
}
public struct SearchResults<T> 
{
    public T Source { get; set; }
    public double Weight { get; set; }
    public bool[] CharMatchResults { get; set; }
}