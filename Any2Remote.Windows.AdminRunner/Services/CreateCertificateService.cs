using Any2Remote.Windows.AdminRunner.Services.Contracts;

namespace Any2Remote.Windows.AdminRunner.Services
{
    public class CreateCertificateService : IRunnerActionService
    {
        public void Run(string[] args)
        {
            CreateNewCertificateDialog dialog = new();
            Application.Run(dialog);
        }
    }
}