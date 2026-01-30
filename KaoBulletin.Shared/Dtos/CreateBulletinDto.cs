using System.ComponentModel.DataAnnotations;

namespace KaoBulletin.Shared.Dtos
{
    /// <summary>
    /// 建立公告用的資料傳輸物件
    /// </summary>
    public class CreateBulletinDto
    {
        /// <summary>
        /// 公告標題
        /// </summary>
        [Required(ErrorMessage = "標題為必填")]
        [MaxLength(100, ErrorMessage = "標題不能超過 100 字")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 公告內容 (HTML 格式)
        /// </summary>
        [Required(ErrorMessage = "內容為必填")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 摘要 (若未填寫，後端自動截取內容前段)
        /// </summary>
        [MaxLength(500)]
        public string? Summary { get; set; }
    }
}