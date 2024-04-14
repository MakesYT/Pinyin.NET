# Pinyin.NET
Pinyin.NET 汉字转换为拼音 拼音模糊搜索
## 1. 汉字转换为拼音
将汉字转换为带音调或不带音调的拼音
返回值为IEnumerable<IEnumerable<string>>，每个IEnumerable<string>为一个汉字的拼音,每个string为一个发音
如果输入的字符不是汉字，则返回值其本身


例如"到底"返回值为[["dao"], ["di","de"]]
```csharp
PinyinProcessor pinyinProcessor = new PinyinProcessor();//不带音调
//PinyinProcessor pinyinProcessor = new PinyinProcessor(PinyinFormat.WithToneMark); //带音调
pinyinProcessor.GetPinyin("到底");//[["dao"], ["di","de"]]
```

## 2. 拼音模糊搜索
在给的的数据源中的指定属性上进行拼音模糊搜索
```csharp
PinyinProcessor pinyinProcessor = new PinyinProcessor(PinyinFormat.WithToneMark);
var enumerable = pinyinProcessor.GetPinyin("尝试111到底是");
list.Add(new MyClass
{
    Name = "尝试111到底是",
    Pinyin = enumerable});
list.Add(new MyClass
{
    Name = "但是等待",
    Pinyin = pinyinProcessor.GetPinyin("但是等待")});
PinyinSearcher pinyinSearcher = new PinyinSearcher(list, "Pinyin");
var search = pinyinSearcher.Search("ddi");
foreach (var searchResult in search)
{
    Console.WriteLine($" {searchResult.Weight}  {((MyClass)searchResult.Source).Name}");
}
//输出 6  尝试111到底是
```