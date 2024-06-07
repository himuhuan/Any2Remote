using Any2Remote.Windows.Server.Hubs;
using Any2Remote.Windows.Server.Services.Contracts;
using Any2Remote.Windows.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Any2Remote.Windows.Server.Controllers
{
    [Route("api/remoteapps")]
    [ApiController]
    public class RemoteAppController : ControllerBase
    {
        private readonly IRemoteAppService _remoteService;
        private readonly IHubContext<RemoteAppHub> _remoteHubContext;

        public RemoteAppController(IRemoteAppService remoteService, IHubContext<RemoteAppHub> remoteHubContext)
        {
            _remoteService = remoteService;
            _remoteHubContext = remoteHubContext;
        }

        [HttpGet]
        public ActionResult<IList<RemoteApplication>> GetRemoteApps()
        {
            try
            {
                IList<RemoteApplication> remoteApps = _remoteService.GetRemoteApplications();
                if (remoteApps.Count == 0) return NoContent();
                return Ok(remoteApps);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{appId}/icon")]
        public ActionResult GetRemoteAppIcon(string appId)
        {
            var dict = _remoteService.GetRemoteAppMap(false);
            if (dict.TryGetValue(appId, out RemoteApplication? value))
            {
                var app = value;
                return PhysicalFile(app.AppIconUrl, "image/png");
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<ActionResult> AddRemoteApp(RemoteApplication application)
        {
            try
            {
                _remoteService.PublishRemoteApp(application);
                await _remoteHubContext.Clients.All.SendAsync("RefreshRequired");
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{appId}")]
        public async Task<ActionResult> RemoveRemoteApp(string appId)
        {
            try
            {
                _remoteService.RemoveRemoteApp(appId);
                await _remoteHubContext.Clients.All.SendAsync("RefreshRequired");
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
