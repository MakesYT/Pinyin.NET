namespace Pinyin.NET;

public class PinyinItem
{
    public string[]? SplitWords { get; set; }
    public bool[]? CharMatchResults { get; set; }
    public List<List<string>>? Keys { get; set; }
}