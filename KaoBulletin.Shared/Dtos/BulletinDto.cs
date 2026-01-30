using System;

namespace KaoBulletin.Shared.Dtos
{
    /// <summary>
    /// 顯示公告用的資料傳輸物件 (包含內容)
    /// </summary>
    public class BulletinDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }

        /// <summary>
        /// 完整的 HTML 內容 (從檔案讀取後填入)
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 實體檔案路徑 (用於 Service 讀取檔案，不一定要顯示在前端)
        /// </summary>
        public string ContentFilePath { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}