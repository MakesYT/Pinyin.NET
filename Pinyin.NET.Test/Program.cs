using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Pinyin.NET;

struct MyClass
{
   public string Name { get; set; }
   public IEnumerable<IEnumerable<string>> Pinyin { get; set; }
}
class Program
{
    
    static void Main(string[] args)
    {
        var list = new List<MyClass>();
        PinyinProcessor pinyinProcessor = new PinyinProcessor();
        // list.Add(new MyClass
        // {
        //     Name = "1.1.2.5内测版5.0",
        //     Pinyin = pinyinProcessor.GetPinyin("1.1.2.5内测版5.0")});
        // var enumerable = pinyinProcessor.GetPinyin("尝试111到底是");
        // list.Add(new MyClass
        // {
        //     Name = "尝试111到底是",
        //     Pinyin = enumerable});
        //
         list.Add(new MyClass
        {
            Name = "但是等待",
            Pinyin = pinyinProcessor.GetPinyin("但是等待")});
        PinyinSearcher pinyinSearcher = new PinyinSearcher(list, "Pinyin");
        var search = pinyinSearcher.Search("dansden");
        foreach (var searchResult in search)
        {
            Console.WriteLine($" {searchResult.Weight}  {((MyClass)searchResult.Source).Name}");
            
        }
    }
    private void preProcess()
    {
        var readAllText = File.ReadAllText("D:\\WPF.net\\Pinyin.NET\\Pinyin.NET\\char_common_base.json");
        var deserialize = JsonSerializer.Deserialize<CharModel[]>(readAllText,new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            IncludeFields = true
        });
        File.WriteAllText("D:\\WPF.net\\Pinyin.NET\\Pinyin.NET\\char_common_base_a.json", JsonSerializer.Serialize(deserialize, new JsonSerializerOptions
        {
            WriteIndented = false
        }));
    }
}