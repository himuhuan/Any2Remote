using Any2Remote.Windows.Server.Helpers;
using Any2Remote.Windows.Shared.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Any2Remote.Windows.Server.Controllers
{

    /// <summary>
    /// File controller: API for file operations
    /// </summary>
    [Route("api/file")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;

        public FileController(ILogger<FileController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 上传一个小型文件到 <c> %APPDATA%\Any2Remote\UploadFiles </c>
        /// </summary>
        /// <returns> 服务器为其创建的临时文件名 </returns>
        /// <seealso cref="UploadFileByStream"> 传输超大型文件 </seealso>
        [HttpPost]
        public async Task<ActionResult> UploadFile(IFormFile file)
        {
            if (file.Length > 0)
            {
                // WindowsCommon.Any2RemoteAppDataFolder 指向当前用户的应用数据文件夹\Any2Remote
                // FileControllerHelpers.PublicUploadPath 目前为 UploadFiles
                // 因此文件将被保存在 %APPDATA%\Any2Remote\UploadFiles
                string filePath = 
                    Path.Combine(WindowsCommon.Any2RemoteAppDataFolder, FileControllerHelpers.PublicUploadPath);
                if (!Directory.Exists(filePath)) 
                    Directory.CreateDirectory(filePath);
                // 文件名是不可信的，所以需要创建一个临时文件名
                string extension = Path.GetExtension(file.FileName);
                string fileName = Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + extension;
                filePath = Path.Combine(filePath, fileName);
                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                return Ok(fileName);
            }
            return NoContent();
        }

        /// <summary>
        /// 使用流上传超大型文件，支持断点续传等
        /// </summary>
        [HttpPost("stream")]
        public ActionResult UploadFileByStream()
        {
            throw new NotImplementedException();
        }
    }
}
