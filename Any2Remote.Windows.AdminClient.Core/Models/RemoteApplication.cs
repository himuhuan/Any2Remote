using Any2Remote.Windows.Grpc.Services;
using Any2Remote.Windows.Shared.Models;

namespace Any2Remote.Windows.AdminClient.Core.Models
{
    public class RemoteApplication : ExecutableApplication
    {
        public string AppId { get; set; } = null!;

        public string AppIconUrl { get; set; } = null!;

        /// <summary>
        /// 如果该程序是没有注册表信息的本地程序，该项留空
        /// </summary>
        public LocalApp? LocalInfo { get; set; } = null;

        public RemoteApplication(string id, ExecutableApplication app) : base(app)
        {
            AppId = id;
        }

        public RemoteApplication()
        {
        }

        public RemoteApplication(ExecutableApplication app) : base(app)
        {
            AppId = Guid.NewGuid().ToString();
        }

        public RemoteApplication(RemoteApplication application)
            : this(application.AppId, application)
        {

        }
    }
}
