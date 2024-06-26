﻿using System.Collections;
using System.Collections.Concurrent;
using System.Text;

namespace Pinyin.NET;

public class PinyinSearcher
{
    public PinyinSearcher(IEnumerable source, string propertyName)
    {
        Source = source;
        PropertyName = propertyName;
        
    }

    public IEnumerable Source { get; set; }
    public string PropertyName { get; set; }

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
                                            //Console.WriteLine("     "+sb+" "+se+"匹配成功");
                                            i++;
                                        }
                                    }
                                    //Console.WriteLine("     L "+sb+" "+se+"匹配成功");
                                    match = true;
                                    matches[j] = true;
                                    weight++;
                                    if (!matchIndexList.Contains(j)&&j>i1)
                                    {
                                        matchIndexList.Add(j);
                                    }
                                    
                                
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

public class PinyinSearcher<T> 
{
    private bool isKeyValuePair;

    public PinyinSearcher(IEnumerable source, string propertyName,bool isKeyValuePair=false)
    {
        Source = source;
        PropertyName = propertyName;
        this.isKeyValuePair = isKeyValuePair;

    }

    public PinyinSearcher(Dictionary<object,T> source, string propertyName)
    {
        SourceDictionary = source;
        PropertyName = propertyName;
        
    }

    public Dictionary<object,T> SourceDictionary { get; set; }
    public IEnumerable Source { get; set; }
    public string PropertyName { get; set; }


    private void PreSearch(char query,IEnumerable<string> pinyins,List<int> searchPaths)
    {
        
    }

    private void Search(string query,ConcurrentBag<SearchResults<T>> results,T source)
    {
        var value = source.GetType()
                          .GetProperty(PropertyName)
                          .GetValue(source);
        var overSearchPaths = new List<List<SearchPathItem>>();
        if (value is not PinyinItem pinyinItem)
        {
            return;
        }
        
        for (var index = 0; index < query.ToArray().Length; index++)
        {
            var c = query.ToArray()[index];
            var searchPaths = new List<SearchPathItem>();
            {
                var enumerable = pinyinItem.Keys.ToArray();
                for (var i = 0; i < enumerable.Length; i++)
                {
                    var enumerable1 = enumerable[i];
                    for (int j = 0; j < enumerable1.Count(); j++)
                    {
                        if (enumerable1.ElementAt(j)
                                       .StartsWith(c))
                        {
                            StringBuilder sb = new();
                            
                            int i1 = index;
                            
                            while (i1 < query.Length )
                            {
                                sb.Append(query[i1]);
                                
                                if (enumerable1.ElementAt(j).StartsWith(sb.ToString()))
                                {
                                    i1++;
                                    continue;
                                }
                                else
                                {
                                    i1--;
                                    break;
                                }
                                
                            }

                            if (i1>=query.Length)
                            {
                                i1--;
                            }
                            
                            var searchPathItem = new SearchPathItem
                            {
                                QueryStartIndex = index,
                                QueryEndIndex = i1,
                                MatchedPinyinIndex = i,
                                MatchedPinyinPronounceIndex = j
                            };
                            //Console.WriteLine(searchPathItem);
                            searchPaths.Add(searchPathItem);
                        }
                    }
                }

                
            }
            overSearchPaths.Add(searchPaths);
            var ismatched= new bool[query.Length];
            for (var index1 = 0; index1 < index+1; index1++)
            {
                var overSearchPath = overSearchPaths[index1];
                foreach (var searchPathItem in overSearchPath)
                {
                    for (int i = searchPathItem.QueryStartIndex; i <= searchPathItem.QueryEndIndex; i++)
                    {
                        ismatched[i] = true;
                    }
                }
            }
            var match1 = true;
            for (var i = 0; i < index+1; i++)
            {
                if (!ismatched[i])
                {
                    match1 = false;
                    break;
                }

            }
            if (!match1)
            {
                return;
            }
        }
        var match = true;
        var matched= new bool[query.Length];
        var pinyinMatched = new bool[pinyinItem.Keys.Count()];
        var weight = 0;
        foreach (var overSearchPath in overSearchPaths)
        {
            foreach (var searchPathItem in overSearchPath)
            {
                pinyinMatched[searchPathItem.MatchedPinyinIndex] = true;
                for (int i = searchPathItem.QueryStartIndex; i <=searchPathItem.QueryEndIndex; i++)
                {
                    matched[i] = true;
                    weight++;
                }
            }
        }
        bool nowMatch = true;
        int space = 0;
        for (var i = 0; i < pinyinMatched.Length; i++)
        {
            if (nowMatch)
            {
                if (!pinyinMatched[i])
                {
                    nowMatch = false;
                    space++;
                }
            }
            else
            {
                nowMatch = pinyinMatched[i];
            }
        }

        if (space !=0)
        {
            weight/=space;
        }
        if (matched.All(e=>e))
        {
            
            results.Add(new SearchResults<T>
            {
                Source = source,
                Weight = ((double)weight)/ (pinyinItem.Keys.Count()),
                CharMatchResults = pinyinMatched
            });
            
            var propertyInfo = source.GetType().GetProperty(PropertyName);
            if ( propertyInfo!= null)
            {
                propertyInfo.GetValue(source).GetType().GetProperty("CharMatchResults").SetValue(propertyInfo.GetValue(source),pinyinMatched);
            }
            
        }
    }

    public IEnumerable<SearchResults<T>> Search(string query)
    {
        var results = new ConcurrentBag<SearchResults<T>>();
        if (Source is not null)
        {
            if (isKeyValuePair)
            {
                Parallel.ForEach((ConcurrentDictionary<string,T>)Source, o =>
                {
                    Search(query, results, (T)o.GetType().GetProperty("Value").GetValue(o));
                });
            }else
            {
                Parallel.ForEach((IEnumerable<T>)Source, o =>
                {
                    Search(query, results, (T)o);
                });
            }
            
        }else if (SourceDictionary is not null)
        {
            Parallel.ForEach(SourceDictionary, o =>
            {
                Search(query, results, o.Value);
            });
        }
        
       
        return results;
    }

    public struct SearchPathItem
    {
        public int QueryStartIndex;
        public int QueryEndIndex;
        public int MatchedPinyinIndex;
        public int MatchedPinyinPronounceIndex;
        public override string ToString()
        {
            return
                $"QueryStartIndex:{QueryStartIndex} QueryEndIndex:{QueryEndIndex} MatchedPinyinIndex:{MatchedPinyinIndex} MatchedPinyinPronounceIndex:{MatchedPinyinPronounceIndex}";
        }
    }
}