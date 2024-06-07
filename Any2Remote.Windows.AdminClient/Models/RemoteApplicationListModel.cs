﻿using Microsoft.UI.Xaml.Media.Imaging;
using Any2Remote.Windows.AdminClient.Helpers;
using Any2Remote.Windows.Grpc.Services;
using Any2Remote.Windows.Shared.Models;
using Newtonsoft.Json;

namespace Any2Remote.Windows.AdminClient.Models
{
    public class RemoteApplicationListModel : RemoteApplication
    {
        [JsonIgnore]
        public BitmapImage AppIconImage { get; set; }

        public string UninstallString
        {
            get  => LocalInfo?.UninstallString ?? string.Empty;
            set {
                if (LocalInfo != null) 
                    LocalInfo.UninstallString = value;
            }
        }

        public RemoteApplicationListModel(ExecutableApplication app)
            : base(app)
        {
            AppIconImage = ModelInfoConverter.GetAppIconBitmapImage(Path);
        }

        public RemoteApplicationListModel(RemoteApplication application)
            : base(application)
        {
            AppIconImage = ModelInfoConverter.GetAppIconBitmapImage(Path);
        }

        public RemoteApplicationListModel(LocalApplicationShowModel localModel, ExecutableApplication application)
            : base(application)
        {
            AppIconImage = localModel.AppIconImage;
            LocalInfo = localModel.RawInfo;
            AppId = localModel.Id;
            DisplayName = localModel.DisplayName;
        }

        public RemoteApplicationListModel(LocalApplicationShowModel localModel)
        {
            AppIconImage = localModel.AppIconImage;
            AppId = localModel.Id;
            AppIconUrl = localModel.Icon;
            DisplayName = localModel.DisplayName;
            LocalInfo = localModel.RawInfo;
        }

        public void SetExecutableInfo(ExecutableApplication app)
        {
            Path = app.Path;
            CommandLine  = app.CommandLine;
            WorkingDirectory = app.WorkingDirectory;
            Description = app.Description;
        }
    }
}