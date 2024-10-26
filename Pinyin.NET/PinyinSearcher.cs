using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
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
        var overSearchPaths = new List<SearchPathItem>();
        if (value is not PinyinItem pinyinItem)
        {
            return;
        }

        IEnumerable<SearchPathItem> NextQueryCharMatch(int nowQueryIndex, int nowMatchedPinyinIndex,
            bool notFirstChar = false, int queryStartIndex = 0, int matchedPinyinStartIndex = 0,
            int matchedPinyinNowStartIndex = 0)
        {
            if (nowQueryIndex==query.Length)
            {
                yield return new SearchPathItem()
                {
                    MatchedPinyinStartIndex = matchedPinyinStartIndex,
                    MatchedPinyinEndIndex = nowMatchedPinyinIndex-1,
                    QueryStartIndex = queryStartIndex,
                    QueryEndIndex = nowQueryIndex-1
                    
                };
                   
                yield break;
            }
            var c = query[nowQueryIndex];
            if (!notFirstChar)
            {
                for (var i = nowMatchedPinyinIndex+1; i < pinyinItem.Keys.Count; i++)
                {
                    var pinyinItemKey = pinyinItem.Keys[i];
                    if (pinyinItemKey.Any(e => e.StartsWith(c)))
                    {
                        if (nowQueryIndex==0)
                        {
                            var a = i;
                            foreach (var searchPathItem in NextQueryCharMatch(nowQueryIndex + 1, i, true,queryStartIndex,a,matchedPinyinNowStartIndex))
                            {
                                yield return searchPathItem;
                            }
                        }else 
                            foreach (var searchPathItem in NextQueryCharMatch(nowQueryIndex + 1, i, true,queryStartIndex,matchedPinyinStartIndex,matchedPinyinNowStartIndex))
                            {
                                yield return searchPathItem;
                            }
                      
                    }
                }
            }
            else
            {
                //全拼
                var pinyinItemKey = pinyinItem.Keys[nowMatchedPinyinIndex];
                var substring = query.Substring(matchedPinyinNowStartIndex,nowQueryIndex-matchedPinyinNowStartIndex+1);
                if (pinyinItemKey.Any(e=>e.StartsWith(substring)))
                {
                    foreach (var searchPathItem in NextQueryCharMatch(nowQueryIndex + 1, nowMatchedPinyinIndex, true,queryStartIndex,matchedPinyinStartIndex))
                    {
                        yield return  searchPathItem;
                    }
                   
                }else
                {
                    foreach (var searchPathItem in NextQueryCharMatch(nowQueryIndex, nowMatchedPinyinIndex, false,queryStartIndex,matchedPinyinStartIndex,nowQueryIndex))
                    {
                        yield  return searchPathItem;
                    }
                   
                }
            }
        }
        var nextQueryCharMatch = NextQueryCharMatch(0,-1);
        foreach (var searchPathItem in nextQueryCharMatch)
        {
            if (searchPathItem.QueryEndIndex==-1)
            {
                continue;
            }
            overSearchPaths.Add(searchPathItem);
        }
        
        bool IsMatchOrNot(int queryEndIndex)
        {
            if (queryEndIndex==query.Length-1)
            {
                return true;
            }
            var searchPathItems = overSearchPaths.Where(e=>e.QueryStartIndex==queryEndIndex);
            return searchPathItems.Any(e =>
            {
                if (e.QueryStartIndex == queryEndIndex)
                {
                    return IsMatchOrNot(e.QueryEndIndex);
                }
                else return false;
            });
        }
        var pinyinMatched = new bool[pinyinItem.Keys.Count()];
        
        if (IsMatchOrNot(0))
        {
            foreach (var overSearchPath in overSearchPaths)
            {
                for (int i = overSearchPath.MatchedPinyinStartIndex; i <= overSearchPath.MatchedPinyinEndIndex; i++)
                {
                    pinyinMatched[i] = true;
                }
            }
            results.Add(new SearchResults<T>
            {
                Source = source,
                Weight = overSearchPaths.Count,
                CharMatchResults = pinyinMatched
            });
            
            var propertyInfo = source.GetType().GetProperty(PropertyName);
            if ( propertyInfo!= null)
            {
                propertyInfo.GetValue(source)?.GetType()?.GetProperty("CharMatchResults")?.SetValue(propertyInfo.GetValue(source),pinyinMatched);
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
        public SearchPathItem()
        {
            
        }
        public int QueryStartIndex=-1;
        public int QueryEndIndex=-1;
        public int MatchedPinyinStartIndex=-1;
        public int MatchedPinyinEndIndex=-1;
      
    }
}