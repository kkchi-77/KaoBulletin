using Microsoft.AspNetCore.Mvc;

namespace KaoBulletin.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiUploadImageController : ControllerBase
    {
        private readonly IConfiguration _config;
        public ApiUploadImageController(IConfiguration config) => _config = config;

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile upload)
        {
            if (upload == null || upload.Length == 0)
                return BadRequest(new { error = new { message = "沒有選擇檔案" } });

            // 1. 取得路徑
            string baseDir = _config["FileStorage:UploadPath"] ?? "D:\\Projects\\KaoBulletin_Uploads";
            string saveDir = Path.Combine(baseDir, "images");

            if (!Directory.Exists(saveDir)) Directory.CreateDirectory(saveDir);

            // 2. 存檔
            string fileName = Guid.NewGuid() + Path.GetExtension(upload.FileName);
            string filePath = Path.Combine(saveDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await upload.CopyToAsync(stream);
            }

            return Ok(new { uploaded = true, url = $"/uploads/images/{fileName}" });
        }
    }
}