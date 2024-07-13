using Any2Remote.Windows.AdminRunner.Services.Contracts;

namespace Any2Remote.Windows.AdminRunner.Services;

public class ConfigureEnhanceModeService : IRunnerActionService
{
    public void Run(string[] args)
    {
        ConfigureEnhanceModeDialog dialog = new(args);
        Application.Run(dialog);
    }
}