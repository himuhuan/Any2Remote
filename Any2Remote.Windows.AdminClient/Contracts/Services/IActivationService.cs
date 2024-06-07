namespace Any2Remote.Windows.AdminClient.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
