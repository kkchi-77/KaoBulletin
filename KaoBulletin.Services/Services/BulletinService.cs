using KaoBulletin.Data.Context;
using KaoBulletin.Data.Entities;
using KaoBulletin.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KaoBulletin.Services.Services
{
    // --- 1. 定義介面 (Interface) ---
    public interface IBulletinService
    {
        /// <summary>
        /// 建立新公告 (包含寫入資料庫與儲存檔案)
        /// </summary>
        /// <param name="dto">建立資料</param>
        /// <param name="storageRootPath">實體檔案儲存根目錄</param>
        /// <returns>建立後的公告物件</returns>
        Task<BulletinDto> CreateAsync(CreateBulletinDto dto, string storageRootPath);

        /// <summary>
        /// 取得公告列表 (不含詳細內容)
        /// </summary>
        Task<List<BulletinDto>> GetListAsync();

        /// <summary>
        /// 取得公告詳情 (需傳入儲存根目錄以拼接實體路徑)
        /// </summary>
        /// <param name="id">公告 ID</param>
        /// <param name="storageRootPath">實體檔案儲存根目錄 (來自設定檔)</param>
        Task<BulletinDto> GetByIdAsync(Guid id, string storageRootPath);
    }

    // --- 2. 實作邏輯 (Implementation) ---
    public class BulletinService : IBulletinService
    {
        private readonly KaoBulletinDbContext _context;

        // 建構子注入 DbContext
        public BulletinService(KaoBulletinDbContext context)
        {
            _context = context;
        }

        public async Task<BulletinDto> CreateAsync(CreateBulletinDto dto, string storageRootPath)
        {
            // 步驟 1: 資料驗證與準備
            if (string.IsNullOrWhiteSpace(dto.Title)) throw new ArgumentException("Title cannot be empty");

            // 準備 Entity (此時尚未寫入 DB)
            Bulletin entity = new Bulletin
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Summary = dto.Summary ?? (dto.Content.Length > 100 ? dto.Content.Substring(0, 100) + "..." : dto.Content),
                CreatedAt = DateTime.Now
            };

            // 規劃檔案路徑: Year/Month/Guid.html (分散目錄以避免單一資料夾檔案過多)
            string relativeFolder = Path.Combine(entity.CreatedAt.Year.ToString(), entity.CreatedAt.Month.ToString("00"));
            string fileName = $"{entity.Id}.html";
            string relativePath = Path.Combine(relativeFolder, fileName);

            // 組合絕對路徑
            string fullFolderPath = Path.Combine(storageRootPath, relativeFolder);
            string fullFilePath = Path.Combine(fullFolderPath, fileName);

            // 步驟 2: 開啟交易 (Transaction) - 這是作品集亮點！
            // 確保「DB寫入」與「檔案建立」同生共死
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // A. 嘗試寫入檔案
                    if (!Directory.Exists(fullFolderPath))
                    {
                        Directory.CreateDirectory(fullFolderPath);
                    }

                    // 非同步寫入檔案
                    await File.WriteAllTextAsync(fullFilePath, dto.Content);

                    // B. 更新 Entity 的路徑資訊並寫入資料庫
                    entity.ContentFilePath = relativePath;
                    _context.Bulletins.Add(entity);
                    await _context.SaveChangesAsync();

                    // C. 提交交易
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    // D. 發生錯誤：回滾 DB
                    await transaction.RollbackAsync();

                    // E. 補償機制：如果檔案已經產生了，要刪掉它 (清理垃圾)
                    if (File.Exists(fullFilePath))
                    {
                        File.Delete(fullFilePath);
                    }
                    throw; // 繼續拋出錯誤給 Controller 處理
                }
            }

            // 步驟 3: 回傳 DTO
            return new BulletinDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Summary = entity.Summary,
                Content = dto.Content, // 回傳當下內容
                CreatedAt = entity.CreatedAt
            };
        }

        public async Task<List<BulletinDto>> GetListAsync()
        {
            // 使用 NoTracking 提升查詢效能 (因為只是讀取列表)
            List<Bulletin> list = await _context.Bulletins
                .AsNoTracking()
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            // 轉換為 DTO (列表不需要讀取檔案內容，省 I/O)
            List<BulletinDto> result = list.Select(b => new BulletinDto
            {
                Id = b.Id,
                Title = b.Title,
                Summary = b.Summary,
                Content = string.Empty, // 列表頁不讀檔
                CreatedAt = b.CreatedAt
            }).ToList();

            return result;
        }

        /// <summary>
        /// 根據 ID 取得公告詳細資料，並從實體路徑讀取 HTML 內容
        /// </summary>
        /// <param name="id">公告主鍵 ID</param>
        public async Task<BulletinDto> GetByIdAsync(Guid id, string storageRootPath)
        {
            // 1. 從資料庫取得 Entity
            Bulletin entity = await _context.Bulletins
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);

            if (entity == null) return null;

            // 2. 轉換為 DTO
            BulletinDto dto = new BulletinDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Summary = entity.Summary,
                CreatedAt = entity.CreatedAt,
                ContentFilePath = entity.ContentFilePath // 這裡仍保留相對路徑供參考
            };

            // 3. 關鍵修正：組合絕對路徑
            // storageRootPath = "D:\Projects\KaoBulletin_Uploads"
            // entity.ContentFilePath = "2026\01\xxx.html"
            string fullPath = Path.Combine(storageRootPath, entity.ContentFilePath);

            // 4. 讀取實體檔案內容
            if (File.Exists(fullPath))
            {
                dto.Content = await File.ReadAllTextAsync(fullPath);
            }
            else
            {
                // 如果失敗，可以在這裡記錄 Log 追蹤 fullPath 到底指向哪裡
                dto.Content = $"<p class='text-danger'>系統提示：找不到內容檔案。 (搜尋路徑: {fullPath})</p>";
            }

            return dto;
        }
    }
}