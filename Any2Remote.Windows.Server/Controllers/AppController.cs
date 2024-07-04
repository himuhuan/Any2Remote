using Any2Remote.Windows.Shared.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Any2Remote.Windows.Server.Controllers
{
    [Route("api/app")]
    [ApiController]
    public class AppController : ControllerBase
    {
        [HttpGet("certificate")]
        public async Task<ActionResult> DownloadCertificate()
        {
            string path = Path.Combine(WindowsCommon.Any2RemoteAppDataFolder, "Certificates", "any2remote.cer");
            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }
            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(path);
            return File(fileBytes, "application/x-x509-ca-cert", "any2remote.cer");
        }
    }
}
