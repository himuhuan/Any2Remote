using IWshRuntimeLibrary;

namespace Any2Remote.Windows.Shared.Extensions
{
    public static class HimuWshShortcutExtensions
    {
        public static string? TryGetArguments(this IWshShortcut shortcut)
        {
            try
            {
                return shortcut.Arguments;
            }
            catch
            {
                return null;
            }
        }

        public static string? TryGetWorkingDirectory(this IWshShortcut shortcut)
        {
            try
            {
                return shortcut.WorkingDirectory;
            }
            catch
            {
                return null;
            }
        }

        public static string? TryGetDescription(this IWshShortcut shortcut)
        {
            try
            {
                return shortcut.Description;
            }
            catch
            {
                return null;
            }
        }
    }
}
