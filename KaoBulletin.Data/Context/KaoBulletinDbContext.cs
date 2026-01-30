using KaoBulletin.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KaoBulletin.Data.Context
{
    public class KaoBulletinDbContext : DbContext
    {
        // 建構子：接收外部注入的設定 (如連線字串)
        public KaoBulletinDbContext(DbContextOptions<KaoBulletinDbContext> options)
            : base(options)
        {
        }

        // 定義資料表
        public DbSet<Bulletin> Bulletins { get; set; }

        // 進階設定 (Optional)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 設定預設值: 建立時間預設為當下
            modelBuilder.Entity<Bulletin>()
                .Property(b => b.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        }
    }
}