using KaoBulletin.Services.Services;
using KaoBulletin.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// 公告管理控制器 (Page Controller)
/// </summary>
public class BulletinController : Controller
{
    private readonly IBulletinService _bulletinService;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// 建構子注入依賴
    /// </summary>
    public BulletinController(IBulletinService bulletinService, IConfiguration configuration)
    {
        _bulletinService = bulletinService;
        _configuration = configuration;
    }

    /// <summary>
    /// 顯示公告列表頁面
    /// </summary>
    /// <returns>回傳 BulletinList View</returns>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        List<BulletinDto> list = await _bulletinService.GetListAsync();
        // 規範：明確指定 View 名稱，不依賴預設的 Index
        return View("BulletinList", list);
    }

    /// <summary>
    /// 顯示新增公告頁面
    /// </summary>
    /// <returns>回傳 BulletinCreate View</returns>
    [HttpGet]
    public IActionResult Create()
    {
        return View("BulletinCreate");
    }

    /// <summary>
    /// 處理新增公告請求
    /// </summary>
    /// <param name="dto">建立公告資料傳輸物件</param>
    /// <returns>成功導回列表，失敗回傳原頁面</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBulletinDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View("BulletinCreate", dto);
        }

        try
        {
            string uploadPath = _configuration["FileStorage:UploadPath"];
            if (string.IsNullOrEmpty(uploadPath))
            {
                throw new Exception("FileStorage:UploadPath is not configured.");
            }

            await _bulletinService.CreateAsync(dto, uploadPath);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"建立失敗: {ex.Message}");
            return View("BulletinCreate", dto);
        }
    }

    /// <summary>
    /// 顯示公告詳細內容頁面
    /// </summary>
    /// <param name="id">公告 ID</param>
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        // 從設定檔讀取 D 槽路徑
        string storagePath = _configuration["FileStorage:UploadPath"] ?? "D:\\Projects\\KaoBulletin_Uploads";

        // 呼叫更新後的 Service
        BulletinDto bulletin = await _bulletinService.GetByIdAsync(id, storagePath);

        if (bulletin == null) return NotFound();

        return View("BulletinDetails", bulletin);
    }
}