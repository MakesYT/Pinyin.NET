using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Pinyin.NET;

public class PinyinProcessor
{
    //大写字母数组char
    readonly char[] _charArray =
    [

        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U',
        'V', 'W', 'X', 'Y', 'Z'
    ];

    readonly char[] _charArraySplit =
    [
        ' ', '_', '-', '.', ',', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '+', '=', '[', ']', '{', '}', 
        '\\', '|', ';', ':', '"', '\'', '<', '>', '?', '/', '~'
    ];

    //中文匹配正则
    readonly Regex regex = new("[\u4e00-\u9fa5]");
    private Dictionary<string, string[]> _pinyinDict=new();
    private Dictionary<string, string[]> _pinyinFullDict=new();

    public PinyinProcessor(PinyinFormat format = PinyinFormat.WithoutTone)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var char_base_aS = assembly.GetManifestResourceStream("Pinyin.NET.char_base_a.json")!;
        var char_common_aS = assembly.GetManifestResourceStream("Pinyin.NET.char_common_base_a.json")!;
        
        var char_base_a = JsonSerializer.Deserialize<CharModel[]>(new StreamReader(char_base_aS).ReadToEnd())!;
        for (var i = 0; i < char_base_a.Length; i++)
        {
            var strings = char_base_a[i].pinyin;
            if (format==PinyinFormat.WithoutTone)
            {
                var newStrings = new List<string>();
                for (var j = strings.Length - 1; j >= 0; j--)
                {
                    var removeDiacritics = RemoveDiacritics(strings[j]);
                    if (!newStrings.Contains(removeDiacritics))
                    {
                        newStrings.Add(removeDiacritics);
                    }
                }
                strings = newStrings.ToArray();
            }
            _pinyinDict.Add( char_base_a[i].Char,strings);
        }
        var char_common_a = JsonSerializer.Deserialize<CharModel[]>(new StreamReader(char_common_aS).ReadToEnd())!;
        for (var i = 0; i < char_common_a.Length; i++)
        {
            var strings = char_common_a[i].pinyin;
            if (format==PinyinFormat.WithoutTone)
            {
                var newStrings = new List<string>();
                for (var j = strings.Length - 1; j >= 0; j--)
                {
                    var removeDiacritics = RemoveDiacritics(strings[j]);
                    if (!newStrings.Contains(removeDiacritics))
                    {
                        newStrings.Add(removeDiacritics);
                    }
                }
                strings = newStrings.ToArray();
            }
            _pinyinFullDict.Add( char_common_a[i].Char,strings);
            
        }
    }

    public PinyinItem GetPinyin(string text,bool withZhongWen=false)
    {
        var result = new List<List<string>>();
        var split = new List<string>();
        StringBuilder sb = new();
        int zhongWenCount = 0;
        if (withZhongWen)
        {
            foreach (var c in text)
            {
                if (regex.IsMatch(c.ToString()))
                {
                    result.Add([c.ToString()]);
                }
            }
        }
        
        for (var i = 0; i < text.Length; i++)
        {
            var input = text[i].ToString();
            if (regex.IsMatch(input))
            {
                zhongWenCount++;
                if (sb.Length > 0)
                {
                    
                    result.Add([sb.ToString().ToLower()]);
                    split.Add(sb.ToString());
                    sb.Clear();
                }
                var c = input;
                if (_pinyinDict.ContainsKey(c))
                {
                    result.Add([.._pinyinDict[c]]);
                    split.Add(c);
                }
                else if (_pinyinFullDict.ContainsKey(c))
                {
                    result.Add([.._pinyinFullDict[c]]);
                    split.Add(c);
                }
                else
                {
                    result.Add([c]);
                    split.Add(c);
                }
            }
            else
            {
                if (_charArray.Contains(text[i]))
                {
                    if (sb.Length > 0)
                    {
                        result.Add([sb.ToString().ToLower()]);
                        split.Add(sb.ToString());
                        sb.Clear();
                    }
                }else if (_charArraySplit.Contains(text[i]))
                {
                    if (sb.Length > 0)
                    {
                        result.Add([sb.ToString().ToLower()]);
                        split.Add(sb.ToString());
                        sb.Clear();
                    }
                    continue;
                }
                sb.Append(input);
            }
        }
        if (sb.Length > 0)
        {
            result.Add([sb.ToString().ToLower()]);
            split.Add(sb.ToString());
        }

        return new PinyinItem()
        {
            SplitWords = split.ToArray(),
            Keys = result,
            ZhongWenCount = zhongWenCount
        };
    }

    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();
        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }
        return stringBuilder.ToString();
    }
}