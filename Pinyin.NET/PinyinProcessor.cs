﻿using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Pinyin.NET;

public class PinyinProcessor
{
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
    public IEnumerable<IEnumerable<string>> GetPinyin(string text)
    {
        var result = new List<List<string>>();
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i].ToString();
            if (_pinyinDict.ContainsKey(c))
            {
                result.Add([.._pinyinDict[c]]);
            }
            else if (_pinyinFullDict.ContainsKey(c))
            {
                result.Add([.._pinyinFullDict[c]]);
            }
            else
            {
                result.Add([c]);
            }
        }
        return result;
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
        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
    
}