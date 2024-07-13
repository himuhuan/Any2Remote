using System.Diagnostics;
using Any2Remote.Windows.AdminClient.Core.Exceptions;

namespace Any2Remote.Windows.AdminClient.Core.Helpers;

public static class AdminRunnerHelper
{
    public static int StartRunner(params string[] args)
    {
        string runnerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            @"Assets\AdminRunner\Any2Remote.Windows.AdminRunner.exe");
        ProcessStartInfo startInfo = new()
        {
            FileName = runnerPath,
            UseShellExecute = true,
            CreateNoWindow = false,
            Arguments = string.Join(' ', args),
            Verb = "runas"
        };
        try
        {
            var process = Process.Start(startInfo);
            process?.WaitForExit();
            return process?.ExitCode ?? -1;
        }
        catch (Exception ex)
        {
            throw new Any2RemoteException("Unable to launch AdminRunner", ex);
        }
    }
}