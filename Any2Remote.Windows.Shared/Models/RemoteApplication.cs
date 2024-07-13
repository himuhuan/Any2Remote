using Any2Remote.Windows.Grpc.Services;

namespace Any2Remote.Windows.Shared.Models
{
    public class RemoteApplication : ExecutableApplication
    {
        public string AppId { get; set; } = string.Empty;

        public string AppIconUrl { get; set; } = string.Empty;

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
            LocalInfo = application.LocalInfo;
        }
    }
}