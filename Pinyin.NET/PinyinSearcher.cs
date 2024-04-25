using System.Collections;
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
    public Dictionary<object,T> SourceDictionary { get; set; }
    public IEnumerable Source { get; set; }
    private bool isKeyValuePair;
    public string PropertyName { get; set; }
    private string? CharMatchResultsPropertyName { get; set; }
    public PinyinSearcher(IEnumerable source, string propertyName,string? charMatchResultsPropertyName=null,bool isKeyValuePair=false)
    {
        Source = source;
        PropertyName = propertyName;
        this.isKeyValuePair = isKeyValuePair;
        CharMatchResultsPropertyName = charMatchResultsPropertyName;

    }
   
    public PinyinSearcher(Dictionary<object,T> source, string propertyName,string? charMatchResultsPropertyName=null)
    {
        SourceDictionary = source;
        PropertyName = propertyName;
        CharMatchResultsPropertyName = charMatchResultsPropertyName;
    }


    private void PreSearch()
    {
        
    }
    private void Search(string query,List<SearchResults<T>> results,T source)
    {
        var value = source.GetType()
                          .GetProperty(PropertyName)
                          .GetValue(source);
        if (value
            is IEnumerable<IEnumerable<string>> pinyins )
        {
            var enumerable = pinyins as IEnumerable<string>[] ?? pinyins.ToArray();
            var matches =new bool[enumerable.Count()];
            query = query.ToLower();
            var weight = 0;
            var matchIndexList = new List<int>();
            var matchT = true;
            //检测搜索词的每一个字符
            for (var i = 0; i < query.Length; i++)
            {
                Console.WriteLine("搜索字符"+query[i]);
                var array = matchIndexList.ToArray();
                if (array.Length == 0)
                {
                    array = new int[]{0};
                }
                var allMatchNum =array.Length;
                int nowMatchIndex = 0;
                matchIndexList.Clear();
                var match = false;
                foreach (var i1 in array)
                {
                    for (int j = i1; j < enumerable.Count(); j++)
                    {
                        Console.WriteLine(" 搜索拼音"+string.Join(",",pinyins.ElementAt(j)));
                        var elementAt = enumerable.ElementAt(j);
                        foreach (var se in elementAt)
                        {
                            Console.WriteLine("     拼音字符"+se);
                            if (se.StartsWith(query[i]))
                            {
                                Console.WriteLine("     匹配成功");
                                StringBuilder sb = new();
                                sb.Append(query[i]);
                                while (i < query.Length - 1)
                                {
                                    sb.Append(query[i+1]);
                                    if (!se.StartsWith(sb.ToString()))
                                    {
                                        Console.WriteLine("     "+sb+" "+se+"匹配失败");
                                        break;
                                    }
                                    else
                                    {
                                        Console.WriteLine("     "+sb+" "+se+"匹配成功");
                                        i++;
                                    }
                                }
                                Console.WriteLine("     L "+sb+" "+se+"匹配成功");
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
                List<List<int>> matchGroups = new List<List<int>>();
                List<int> currentGroup = new List<int>();

                for (var i = 0; i < matches.Length; i++)
                {
                    if (matches[i])
                    {
                        currentGroup.Add(i);
                    }
                    else
                    {
                        if (currentGroup.Count > 0)
                        {
                            matchGroups.Add(currentGroup);
                            currentGroup = new List<int>();
                        }
                    }
                }

                if (currentGroup.Count > 0)
                {
                    matchGroups.Add(currentGroup);
                }

                foreach (var group in matchGroups)
                {
                    int firstMatchIndex = group.First();
                    int lastMatchIndex = group.Last();

                    Console.WriteLine("firstMatchIndex " + firstMatchIndex);
                    Console.WriteLine("lastMatchIndex " + lastMatchIndex);

                    for (var i = firstMatchIndex; i <= lastMatchIndex; i++)
                    {
                        if (!matches[i])
                        {
                            return;
                        }
                    }
                }
                results.Add(new SearchResults<T>
                {
                    Source = source,
                    Weight = weight/ enumerable.Count(),
                    CharMatchResults = matches
                });
                if (CharMatchResultsPropertyName!=null)
                {
                    var propertyInfo = source.GetType().GetProperty(CharMatchResultsPropertyName);
                    if ( propertyInfo!= null)
                    {
                        propertyInfo.SetValue(source,matches);
                    }
                }
            }

                
        }
        
        
    }
    public IEnumerable<SearchResults<T>> Search(string query)
    {
        var results = new List<SearchResults<T>>();
        if (Source is not null)
        {
            foreach (var o in Source)
            {
                if (isKeyValuePair)
                {
                    Search(query, results, (T)o.GetType().GetProperty("Value").GetValue(o));
                }else
                {
                    Search(query, results, (T)o);
                }
            }
        }else if (SourceDictionary is not null)
        {
            foreach (var (key, value) in SourceDictionary)
            {
                Search(query, results,value);
            }
        }
        
       
        return results;
    }
}