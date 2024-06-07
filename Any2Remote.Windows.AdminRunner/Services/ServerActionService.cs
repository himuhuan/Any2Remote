using Any2Remote.Windows.AdminRunner.Services.Contracts;

namespace Any2Remote.Windows.AdminRunner.Services
{
    public class ServerActionService : IRunnerActionService
    {
        public void Run(string[] args)
        {
            ServerActionDialog dialog = new(args);
            Application.Run(dialog);
        }
    }
}
