﻿using System.Collections;
using System.Text;

namespace Pinyin.NET;

public class PinyinSearcher
{
    public IEnumerable Source { get; set; }
    public string PropertyName { get; set; }
    public PinyinSearcher(IEnumerable source, string propertyName)
    {
        Source = source;
        PropertyName = propertyName;
        
    }
    public IEnumerable<SearchResults> Search(string query)
    {
        var results = new List<SearchResults>();
        foreach (var x in Source)
        {
            var value = x.GetType()
                         .GetProperty(PropertyName)
                         .GetValue(x);
            if (value
                is IEnumerable<IEnumerable<string>> pinyins )
            {
                
                var matches =new bool[pinyins.Count()];
                query = query.ToLower();
                var weight = 0;
                var matchIndexList = new List<int>();
                var matchT = true;
                //检测搜索词的每一个字符
                for (var i = 0; i < query.Length; i++)
                {
                    //Console.WriteLine("搜索字符"+query[i]);
                    var array = matchIndexList.ToArray();
                    if (array.Length == 0)
                    {
                        array = new int[]{0};
                    }
                    var allMatchNum =array.Length;
                    matchIndexList.Clear();
                    var match = false;
                    foreach (var i1 in array)
                    {
                        for (int j = i1; j < pinyins.Count(); j++)
                        {
                            //Console.WriteLine(" 搜索拼音"+string.Join(",",pinyins.ElementAt(j)));
                            var elementAt = pinyins.ElementAt(j);
                            foreach (var se in elementAt)
                            {
                                //Console.WriteLine("     拼音字符"+se);
                                if (se.StartsWith(query[i]))
                                {
                                    //Console.WriteLine("     匹配成功");
                                    StringBuilder sb = new();
                                    sb.Append(query[i]);
                                    while (i < query.Length - 1)
                                    {
                                        sb.Append(query[i+1]);
                                        if (!se.StartsWith(sb.ToString()))
                                        {
                                            //Console.WriteLine("     "+sb+" "+se+"匹配失败");
                                            break;
                                        }
                                        else
                                        {
                                            i++;
                                        }
                                    }
                                    match = true;
                                    matches[j] = true;
                                    weight++;
                                    matchIndexList.Add(j);
                                
                                }
                            }
                        }
                        if (!match)
                        {
                            --allMatchNum;
                            break;
                        }
                    }
                    if (allMatchNum == 0)
                    {
                        matchT = false;
                        break;
                    }
                    
                   
                }
                if (matchT)
                {
                    results.Add(new SearchResults
                    {
                        Source = x,
                        Weight = weight,
                        CharMatchResults = matches
                    });
                }

                
            }
        }
       
        return results;
    }
   
  
}