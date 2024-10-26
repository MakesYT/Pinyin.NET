using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Pinyin.NET;

struct MyClass
{
   public string Name { get; set; }
   public PinyinItem Pinyin { get; set; }
}
class Program
{
    
    static void Main(string[] args)
    {
        var list = new List<MyClass>();
        PinyinProcessor pinyinProcessor = new PinyinProcessor();
        var enumerable = pinyinProcessor.GetPinyin("JetBrainsToolbox",true);
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
        // list.Add(new MyClass
        // {
        //     Name = "慕讯加速器",
        //     Pinyin = pinyinProcessor.GetPinyin("慕讯加速器")});
        list.Add(new MyClass
        {
            Name = "高级安全WindowsDefender防火墙",
            Pinyin = pinyinProcessor.GetPinyin("高级安全WindowsDefender防火墙",true)});
        // list.Add(new MyClass
        // {
        //     Name = "ic_fluent_text_sort_mescending_24_regular",
        //     Pinyin = pinyinProcessor.GetPinyin("ic_fluent_text_sort_mescending_24_regular")});
        PinyinSearcher<MyClass> pinyinSearcher = new PinyinSearcher<MyClass>(list, "Pinyin");
        var search = pinyinSearcher.Search("janq");
        foreach (var searchResult in search)
        {
            Console.WriteLine($" {searchResult.Weight}  {searchResult.Source.Name}");
            
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