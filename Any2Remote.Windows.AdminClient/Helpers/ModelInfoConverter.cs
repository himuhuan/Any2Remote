using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.Storage;
using System.Drawing;
using Any2Remote.Windows.Shared.Helpers;

namespace Any2Remote.Windows.AdminClient.Helpers
{
    /// <summary>
    /// 模型转化实用工具类：用于将模型信息转化为其它格式以便显示/存储
    /// </summary>
    public static class ModelInfoConverter
    {
        public static Bitmap ConvertIconToBitmap(System.Drawing.Icon icon)
        {
            Bitmap bitmap = new(icon.Width, icon.Height);
            using Graphics g = Graphics.FromImage(bitmap);
            g.Clear(Color.Transparent);
            g.DrawIcon(icon, new Rectangle(0, 0, icon.Width, icon.Height));
            return bitmap;
        }

        public static BitmapImage GetAppIconBitmapImage(string iconUrl)
        {
            var parseResult = WindowsCommon.ParseIconUrl(iconUrl);
            var icon = parseResult.IconFilePath == null
                ? SystemIcons.Application
                : WindowsCommon.ExtractIcon(parseResult.IconFilePath, parseResult.IconIndex);
            icon ??= SystemIcons.Application;
            try
            {
                return ConvertIconToBitmapImage(icon);
            }
            finally
            {
                icon.Dispose();
            }
        }

        public static async Task SaveAppIcon(string path, StorageFile file)
        {
            var programFile = StorageFile.GetFileFromPathAsync(path).GetAwaiter().GetResult();
            var icon = await programFile.GetThumbnailAsync(ThumbnailMode.SingleItem, 48);
            if (icon == null) return;
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            await RandomAccessStream.CopyAndCloseAsync(icon.GetInputStreamAt(0), stream.GetOutputStreamAt(0));
        }

        /// <summary>
        /// Convert a System.Drawing.Icon to a BitmapImage.
        /// </summary>
        public static BitmapImage ConvertIconToBitmapImage(System.Drawing.Icon icon)
        {
            using var bitmap = ConvertIconToBitmap(icon);
            using MemoryStream iconStream = new();
            BitmapImage bitmapImage = new();
            bitmap.Save(iconStream, System.Drawing.Imaging.ImageFormat.Png);
            iconStream.Position = 0;
            bitmapImage.SetSource(iconStream.AsRandomAccessStream());
            return bitmapImage;
        }
    }
}