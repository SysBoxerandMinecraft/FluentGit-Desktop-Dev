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
    /// <summary>
    /// 判断单个搜索项是否匹配 query
    /// </summary>
    public static bool Matches(SearchEntry entry, string query)
    {
        if (entry == null) return false;
        if (string.IsNullOrWhiteSpace(query)) return true;

        if (entry.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
            return true;

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
    /// 应用过滤结果到 UI
    /// </summary>
    public static List<string> ApplyFilter(
        IEnumerable<SearchEntry> allEntries,
        string query,
        out bool hasAnyMatch)
    {
        var entries = allEntries?.ToList() ?? new List<SearchEntry>();
        hasAnyMatch = false;

        if (entries.Count == 0) return new List<string>();

        // 空搜索 → 全部显示
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

        return matchedTitles;
    }
}