namespace FluentGit.Services;

public static partial class GitService
{
    // ========== 状态条目 ==========
    public class GitStatusEntry
    {
        public string? Path { get; set; }
        public char X { get; set; }   // 暂存区状态
        public char Y { get; set; }   // 工作区状态
    }
}