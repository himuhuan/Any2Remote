
namespace Any2Remote.Windows.Shared.Models;

/// <summary>
/// 表示一个可执行程序，包含程序的显示名称、路径、命令行参数、工作目录和描述信息。
/// </summary>
public class ExecutableApplication
{
    public string DisplayName { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public string CommandLine { get; set; } = string.Empty;

    public string WorkingDirectory { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{DisplayName} - {Path} {CommandLine}";
    }

    public ExecutableApplication()
    {
    }

    public ExecutableApplication(ExecutableApplication app)
    {
        DisplayName = app.DisplayName;
        Path = app.Path;
        CommandLine = app.CommandLine;
        WorkingDirectory = app.WorkingDirectory;
        Description = app.Description;
    }
}
