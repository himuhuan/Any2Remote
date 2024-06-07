using Any2Remote.Windows.AdminClient.Helpers;
using Any2Remote.Windows.Grpc.Services;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Any2Remote.Windows.AdminClient.Models
{
    public class LocalApplicationShowModel
    {
        public LocalApp RawInfo { get; set; }
        public string DisplayName => RawInfo.DisplayName;
        public string Icon => RawInfo.IconUrl;
        public string Id => RawInfo.Id;
        public string UninstallString => RawInfo.UninstallString;
        public BitmapImage AppIconImage { get; private set; } 

        public LocalApplicationShowModel(LocalApp app)
        {
            RawInfo = app; 
            AppIconImage = ModelInfoConverter.GetAppIconBitmapImage(app.IconUrl);
        }
    }
}
