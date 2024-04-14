using System.Text.Json.Serialization;

namespace Pinyin.NET;

public struct CharModel
{
    [JsonPropertyName("char")]
    public string Char { get; set; }
    [JsonPropertyName("pinyin")]
    public string[] pinyin { get; set; }
}