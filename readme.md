# Pinyin.NET
[![NuGet](https://img.shields.io/nuget/v/PinyinM.NET?style=for-the-badge&logo=nuget&label=release)](https://www.nuget.org/packages/PinyinM.NET/)
[![NuGet](https://img.shields.io/nuget/dt/PinyinM.NET?label=downloads&style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/PinyinM.NET)  
Pinyin.NET 汉字转换为拼音 拼音模糊搜索
## 1. 汉字转换为拼音
将汉字转换为带音调或不带音调的拼音  
返回值为IEnumerable<IEnumerable<string>>，  
每个IEnumerable<string>为一个汉字的拼音,  
每个string为一个发音
如果输入的字符不是汉字，则返回值其本身


例如"到底"返回值为[["dao"], ["di","de"]]
例如"Stea mSt2等2待"返回值为[ ["stea"], ["m"], ["st2"], ["deng"],["2"], ["dai"]]
```csharp
PinyinProcessor pinyinProcessor = new PinyinProcessor();//不带音调
//PinyinProcessor pinyinProcessor = new PinyinProcessor(PinyinFormat.WithToneMark); //带音调
pinyinProcessor.GetPinyin("到底");//[["dao"], ["di","de"]]
```

## 2. 拼音模糊搜索
在给的的数据源中的指定属性上进行拼音模糊搜索  
支持拼音全拼 首字母 全拼和首字母混合搜索 (支持多音字)
```csharp
list.Add(new MyClass
{
    Name = "但是等待",
    Pinyin = pinyinProcessor.GetPinyin("但是等待")
});
PinyinSearcher<MyClass> pinyinSearcher = new PinyinSearcher<MyClass>(list, "Pinyin");
var search = pinyinSearcher.Search("dansden");
foreach (var searchResult in search)
{
    Console.WriteLine($" {searchResult.Weight}  {searchResult.Source.Name}");
}
//输出 6  尝试111到底是
```

## 更新日志
### 1.0.3
1. 重构 搜索
### 1.0.2
1. 优化 混合字符转拼音,现在会自动以大写字母和部分分隔符分割
### 1.0.1
1. 优化 包含英文的字母的拼音转换
1. 新增 泛型搜索类型
1. 优化 添加对字典以及KeyValuePair支持
1. 优化 权重计算