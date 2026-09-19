using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;

namespace FluentGit.Services.Search;

/// <summary>
/// 通用搜索服务：匹配、过滤、更新可见性
/// </summary>
public static class SearchService
{
    private const string TAG = "SearchService";

    /// <summary>
    /// 判断单个搜索项是否匹配 query
    /// </summary>
    public static bool Matches(SearchEntry entry, string query)
    {
        if (entry == null) return false;
        if (string.IsNullOrWhiteSpace(query)) return true;

        // 1. 标题匹配
        if (entry.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
            return true;

        // 2. 关键字匹配
        foreach (var keyword in entry.Keywords)
        {
            if (keyword.Contains(query, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 过滤搜索项集合，返回匹配的项
    /// </summary>
    public static List<SearchEntry> Filter(IEnumerable<SearchEntry> entries, string query)
    {
        if (entries == null) return new List<SearchEntry>();
        if (string.IsNullOrWhiteSpace(query)) return entries.ToList();

        return entries.Where(e => Matches(e, query)).ToList();
    }

    /// <summary>
    /// 应用过滤结果到 UI：
    /// - 匹配项可见、不匹配项隐藏
    /// - 分组内所有项都隐藏时，分组本身也隐藏
    /// - 返回匹配项的标题列表（给 AutoSuggestBox 用）
    /// </summary>
    public static List<string> ApplyFilter(
        IEnumerable<SearchEntry> allEntries,
        string query,
        out bool hasAnyMatch)
    {
        var entries = allEntries?.ToList() ?? new List<SearchEntry>();
        hasAnyMatch = false;

        if (entries.Count == 0) return new List<string>();

        // 1. 空搜索 → 全部显示
        if (string.IsNullOrWhiteSpace(query))
        {
            foreach (var e in entries)
            {
                if (e.Item != null) e.Item.Visibility = Visibility.Visible;
                if (e.Group != null) e.Group.Visibility = Visibility.Visible;
            }
            hasAnyMatch = true;
            return new List<string>();
        }

        // 2. 逐项匹配
        var matchedTitles = new List<string>();
        var visibleGroups = new HashSet<FrameworkElement>();

        foreach (var e in entries)
        {
            bool match = Matches(e, query);

            if (e.Item != null)
                e.Item.Visibility = match ? Visibility.Visible : Visibility.Collapsed;

            if (match)
            {
                hasAnyMatch = true;
                matchedTitles.Add(e.Title);
                if (e.Group != null) visibleGroups.Add(e.Group);
            }
        }

        // 3. 更新分组可见性
        var allGroups = entries
            .Where(e => e.Group != null)
            .Select(e => e.Group!)
            .Distinct();

        foreach (var group in allGroups)
        {
            group.Visibility = visibleGroups.Contains(group)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        AppLogger.Info(TAG, $"搜索 \"{query}\" → {matchedTitles.Count}/{entries.Count} 匹配");

        return matchedTitles;
    }
}