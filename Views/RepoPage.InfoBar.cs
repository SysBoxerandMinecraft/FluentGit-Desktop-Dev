using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading;
using System.Threading.Tasks;

namespace FluentGit.Views;

public sealed partial class RepoPage
{
    // ========== InfoBar ==========
    private async void ShowInfoBar(string title, string message, InfoBarSeverity severity)
    {
        if (_isInfoBarAnimating)
        {
            _infoBarCts?.Cancel();
            await Task.Delay(50);
        }

        _isInfoBarAnimating = true;
        _infoBarCts = new CancellationTokenSource();
        var token = _infoBarCts.Token;

        SlideInStoryboard.Stop();
        SlideOutStoryboard.Stop();
        InfoBarTransform.Y = -80;
        InfoBarContainer.Opacity = 0;
        InfoBarContainer.Visibility = Visibility.Visible;

        StatusInfoBar.Title = title;
        StatusInfoBar.Message = message;
        StatusInfoBar.Severity = severity;

        SlideInStoryboard.Begin();

        try
        {
            await Task.Delay(3000, token);
            SlideOutStoryboard.Begin();
            await Task.Delay(200);
            InfoBarContainer.Visibility = Visibility.Collapsed;
        }
        catch (TaskCanceledException)
        {
            InfoBarContainer.Visibility = Visibility.Collapsed;
            InfoBarTransform.Y = -80;
            InfoBarContainer.Opacity = 0;
            SlideInStoryboard.Stop();
            SlideOutStoryboard.Stop();
        }
        finally
        {
            _isInfoBarAnimating = false;
        }
    }
}