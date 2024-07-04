using Any2Remote.Windows.AdminRunner.Services;
using Any2Remote.Windows.AdminRunner.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Any2Remote.Windows.AdminRunner
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();            

            try
            {
                var services = new ServiceCollection();
                ConfigureServices(services, args);
                var serviceProvider = services.BuildServiceProvider();
                var runner = serviceProvider.GetRequiredService<IRunnerActionService>();
                runner.Run(args);
            } 
            catch (ArgumentException ae)
            {
                MessageBox.Show($"无效的启动参数: {ae.Message}", "启动失败", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                Environment.Exit(1);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Runtime Error: {e.Message}", "致命错误", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                Environment.Exit(2);
            }
        }

        // Configure Dependency Injection
        private static void ConfigureServices(IServiceCollection services, string[] args)
        {
            if (args == null || args.Length < 1)
                throw new ArgumentException("Expected at least 1 argument");

            if (args[0] == "server" && args.Length > 1)
            {
                services.AddSingleton<IRunnerActionService, ServerActionService>();
            }
            else if (args[0] == "create-ca-cert")
            {
                services.AddSingleton<IRunnerActionService, CreateCertificateService>();
            }
            else
            {
                string argList = string.Join(' ', args);
                throw new ArgumentException(argList);
            }
        }
    }
}