using HimuRdp.Core.Resources;
using System.Text;

namespace HimuRdp.Core;

public static class ResourceHelper
{
    public static async Task ExtractResourceAsync(HimuRdpResourceKey resourceKey, string path, bool overwrite = true)
    {
        await using var stream = File.OpenWrite(path);
        var resource = GetResource(resourceKey);
        await stream.WriteAsync(resource);
    }

    public static void ExtractResource(HimuRdpResourceKey resourceKey, string path, bool overwrite = true)
    {
        using var stream = File.OpenWrite(path);
        var resource = GetResource(resourceKey);
        stream.Write(resource);
    }

    private static byte[] GetResource(HimuRdpResourceKey resourceKey)
    {
        return resourceKey switch
        {
            HimuRdpResourceKey.RdpClip6164Exe
                => (byte[]) HimuRdpResource.ResourceManager.GetObject("rdpclip6164")!,
            HimuRdpResourceKey.RdpClip6032Exe 
                => (byte[]) HimuRdpResource.ResourceManager.GetObject("rdpclip6032")!,
            HimuRdpResourceKey.RdpClip6064Exe 
                => (byte[]) HimuRdpResource.ResourceManager.GetObject("rdpclip6034")!,
            HimuRdpResourceKey.RdpClip6132Exe 
                => (byte[]) HimuRdpResource.ResourceManager.GetObject("rdpclip6132")!,
            HimuRdpResourceKey.RdpWrapperConfigIni
                => Encoding.UTF8.GetBytes(HimuRdpResource.ResourceManager.GetString("rdpwrap")!),
            HimuRdpResourceKey.RdpWrap32Dll
                => (byte[]) HimuRdpResource.ResourceManager.GetObject("rdpwrap32")!,
            HimuRdpResourceKey.RdpWrap64Dll
                => (byte[]) HimuRdpResource.ResourceManager.GetObject("rdpwrap64")!,
            HimuRdpResourceKey.RfxVmt32Dll
                => (byte[]) HimuRdpResource.ResourceManager.GetObject("rfxvmt32")!,
            HimuRdpResourceKey.RfxVmt64Dll
                => (byte[]) HimuRdpResource.ResourceManager.GetObject("rfxvmt64")!,
            _ => throw new ArgumentOutOfRangeException(nameof(resourceKey), resourceKey, null)
        };
    }
}