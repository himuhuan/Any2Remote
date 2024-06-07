namespace Any2Remote.Windows.Server.Helpers
{
    public static class FileControllerHelpers
    {
        // path to store uploaded files
        public const string PublicUploadPath = "UploadFiles";

        // Limit small file size to 100MB
        public const long MaxFileSize = 100 * 1024 * 1024;
    }
}