using Any2Remote.Windows.AdminClient.Contracts.Services;
using Any2Remote.Windows.AdminClient.Core.Exceptions;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;
using Any2Remote.Windows.Shared.Models;

namespace Any2Remote.Windows.AdminClient.Helpers
{
    public class CoreServerClient : ICoreServerClient
    {
        private readonly HttpClient _httpClient = new()
        {
            // TODO: https cretificate required
            BaseAddress = new Uri("https://localhost:7132/")
        };

        public async Task<List<RemoteApplication>> GetApplicationsAsync()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("api/remoteapps");
                if (response.IsSuccessStatusCode)
                {
                    var str = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<IList<RemoteApplication>>(str);
                    return result != null ? result.ToList() : new List<RemoteApplication>();
                }

                throw new ServerRequestException(response.StatusCode);
            }
            catch (Exception ex)
            {
                throw new ServerRequestException(HttpStatusCode.InternalServerError, ex);
            }
        }

        public async Task PublishRemoteApplicationAsync(RemoteApplication application)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/remoteapps", application);
                if (!response.IsSuccessStatusCode)
                    throw new ServerRequestException(response.StatusCode);
            }
            catch (Exception ex)
            {
                throw new ServerRequestException(HttpStatusCode.InternalServerError, ex);
            }
        }

        public async Task RemoveRemoteApplicationAsync(RemoteApplication application)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/remoteapps/{application.AppId}");
                if (!response.IsSuccessStatusCode)
                    throw new ServerStatusException(ServerStatus.InternalError);
            }
            catch (Exception e)
            {
                throw new ServerRequestException(HttpStatusCode.InternalServerError, e);
            }
        }
    }
}