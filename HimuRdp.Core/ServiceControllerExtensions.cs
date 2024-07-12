using System.ComponentModel;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using Windows.Win32;
using Windows.Win32.System.Services;

namespace HimuRdp.Core;

public static class ServiceControllerExtensions
{
    public static void StartServiceWithDepends(this ServiceController service)
    {
        if (service.Status == ServiceControllerStatus.Running)
            return;

        foreach (var depend in service.ServicesDependedOn)
        {
            if (depend.Status != ServiceControllerStatus.Running)
            {
                StartServiceWithDepends(depend);
            }
        }

        service.Start();
        service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// Configures the start mode of a specified service.
    /// </summary>
    /// <param name="service">The service to configure.</param>
    /// <param name="startMode">The desired start mode for the service.</param>
    /// <returns>The previous start mode of the service.</returns>
    /// <exception cref="Win32Exception">
    /// Thrown when there is an error accessing the service control manager or changing the service configuration.
    /// </exception>
    /// <remarks>
    /// This method changes the start mode of the specified service to the provided start mode.
    /// If the service is already set to the desired start mode, no changes are made.
    /// </remarks>
    public static ServiceStartMode ConfigureStartMode(this ServiceController service, ServiceStartMode startMode)
    {
        const uint scManagerConnect = 0x0001, serviceChangeConfig = 0x0002;
        if (service.StartType == startMode)
            return service.StartType;
        var       oldStartMode = service.StartType;
        using var handle       = PInvoke.OpenSCManager(string.Empty, default!, scManagerConnect);
        if (handle.IsInvalid)
            throw new Win32Exception(Marshal.GetLastWin32Error());
        using var serviceHandle = PInvoke.OpenService(handle, service.ServiceName, serviceChangeConfig);
        if (serviceHandle.IsInvalid)
            throw new Win32Exception(Marshal.GetLastWin32Error());
        unsafe
        {
            var result = PInvoke.ChangeServiceConfig(
                serviceHandle,
                ENUM_SERVICE_TYPE.SERVICE_NO_CHANGE,
                (SERVICE_START_TYPE)(uint)startMode,
                SERVICE_ERROR.SERVICE_NO_CHANGE,
                default, default, null, default, default, default, default);
            if (!result)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        return oldStartMode;
    }

    public static void Restart(this ServiceController service)
    {
        if (service.Status == ServiceControllerStatus.Running)
        {
            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(5));
        }
        if (service.Status != ServiceControllerStatus.Stopped) 
            return;
        service.StartServiceWithDepends();
        service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(5));
    }
}
