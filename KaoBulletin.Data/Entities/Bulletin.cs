using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KaoBulletin.Data.Entities
{
    /// <summary>
    /// 公告 Metadata 實體
    /// (注意：公告的 HTML 內容儲存在檔案系統，不在此資料表中)
    /// </summary>
    [Table("Bulletins")]
    public class Bulletin
    {
        /// <summary>
        /// 公告唯一識別碼 (Primary Key)
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// 公告標題
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 簡短摘要 (用於列表顯示，不含 HTML 標籤)
        /// </summary>
        [MaxLength(500)]
        public string? Summary { get; set; }

        /// <summary>
        /// 內容檔案的相對路徑
        /// 範例: "2024/01/guid-content.html"
        /// </summary>
        [MaxLength(255)]
        public string? ContentFilePath { get; set; }

        /// <summary>
        /// 資料建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 最後修改時間
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 發布者 ID (預留擴充欄位)
        /// </summary>
        public Guid? AuthorId { get; set; }
    }
}