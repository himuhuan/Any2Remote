namespace Any2Remote.Windows.AdminRunner.Services.Contracts
{
    /// <summary>
    /// Marker interface for AdminRunner actions
    /// </summary>
    public interface IRunnerActionService
    {
        public void Run(string[] args);
    }
}
