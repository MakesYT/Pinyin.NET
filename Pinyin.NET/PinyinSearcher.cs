using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace Pinyin.NET;

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
            bool notFirstChar = false, int queryStartIndex = 0, int matchedPinyinStartIndex = 0, int matchedQueryStartIndex=0,int queryStartPinyinIndex=0)
        {
            if (nowQueryIndex==query.Length)
            {
                yield return new SearchPathItem()
                {
                    MatchedPinyinStartIndex = queryStartPinyinIndex,
                    MatchedPinyinEndIndex = nowMatchedPinyinIndex,
                    QueryStartIndex = queryStartIndex,
                    QueryEndIndex = nowQueryIndex-1
                    
                };
                   
                yield break;
            }
            var c = query[nowQueryIndex];
            if (!notFirstChar)
            {
                for (var i = nowMatchedPinyinIndex; i < pinyinItem.Keys.Count; i++)
                {
                    var pinyinItemKey = pinyinItem.Keys[i];
                    if (pinyinItemKey.Any(e => e.StartsWith(c)))
                    {
                        if (nowQueryIndex==0)
                        {
                            queryStartPinyinIndex = i;
                        }
                        foreach (var searchPathItem in NextQueryCharMatch(nowQueryIndex + 1, i, true,queryStartIndex,i,nowQueryIndex,queryStartPinyinIndex))
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
                var substring = query.Substring(matchedQueryStartIndex,nowQueryIndex-matchedQueryStartIndex+1);
                if (pinyinItemKey.Any(e=>e.StartsWith(substring)))
                {
                    foreach (var searchPathItem in NextQueryCharMatch(nowQueryIndex + 1, nowMatchedPinyinIndex, true,queryStartIndex,matchedPinyinStartIndex,matchedQueryStartIndex,queryStartPinyinIndex))
                    {
                        yield return  searchPathItem;
                    }
                   
                }else
                {
                    foreach (var searchPathItem in NextQueryCharMatch(nowQueryIndex, nowMatchedPinyinIndex+1, false,queryStartIndex,matchedPinyinStartIndex,matchedQueryStartIndex,queryStartPinyinIndex))
                    {
                        yield  return searchPathItem;
                    }
                   
                }
            }
        }
        var nextQueryCharMatch = NextQueryCharMatch(0,0);
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
        
        if (overSearchPaths.Any())
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