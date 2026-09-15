cd D:\Code\C#_NET\FluentGit
Get-Process FluentGit -ErrorAction SilentlyContinue | Stop-Process -Force
Remove-Item -Recurse -Force obj,bin -ErrorAction SilentlyContinue
dotnet build FluentGit.csproj -c Debug -p:Platform=x64
Pause