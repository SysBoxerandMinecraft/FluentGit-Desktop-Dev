using System;
using Microsoft.UI.Xaml;

namespace FluentGit.Services.Search;

/// <summary>
/// 一个可搜索的设置项
/// </summary>
public class SearchEntry
{
    /// <summary>显示标题（同时用于 AutoSuggestBox 建议列表）</summary>
    public string Title { get; set; } = "";

    /// <summary>搜索关键字（中英文、别名、技术名词等）</summary>
    public string[] Keywords { get; set; } = Array.Empty<string>();

    /// <summary>关联的 UI 元素（匹配时显示，否则隐藏）</summary>
    public FrameworkElement? Item { get; set; }

    /// <summary>所属分组（分组内所有 Item 都隐藏时，分组也隐藏）</summary>
    public FrameworkElement? Group { get; set; }
}