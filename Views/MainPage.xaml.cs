using Microsoft.UI.Xaml.Controls;

namespace FluentGit.Views;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();

        // 默认选中第一项（仓库）
        if (MainNavView.MenuItems.Count > 0)
        {
            var firstItem = MainNavView.MenuItems[0] as NavigationViewItem;
            if (firstItem != null)
            {
                MainNavView.SelectedItem = firstItem;
                NavigateToPage(firstItem.Tag?.ToString());
            }
        }
    }

    private void OnNavigationViewSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item)
        {
            NavigateToPage(item.Tag?.ToString());
        }
    }

    private void NavigateToPage(string? tag)
{
    if (string.IsNullOrEmpty(tag)) return;

    switch (tag)
    {
        case "Repo":
            ContentFrame.Navigate(typeof(RepoPage));
            break;
        case "Admin":
            ContentFrame.Navigate(typeof(AdminPage));
            break;
        case "Settings":
            ContentFrame.Navigate(typeof(SettingsPage));  // 改为 SettingsPage
            break;
        default:
            ContentFrame.Navigate(typeof(PlaceholderPage), "未知");
            break;
    }
}
}