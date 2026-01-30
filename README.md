# KaoBulletin - .NET 9 公告管理系統

這是一個基於 **ASP.NET Core 9** 開發的公告管理系統，採用分層架構 (Layered Architecture) 並實作了安全的外部檔案儲存機制。

## 🚀 核心技術亮點

- **分層架構與依賴注入**: 嚴格劃分 Controller、Service、Data (EF Core) 與 Shared (DTO) 層級，並透過 Interface 進行 DI 註冊。
- **安全外部存儲策略**: 動態生成的 HTML 與上傳圖片不存放於 `wwwroot`，而是透過 `PhysicalFileProvider` 對應至伺服器實體路徑，確保系統重新部署時資料不遺失並提升安全性。
- **資料庫事務 (Transaction)**: 在建立公告時整合 DB 寫入與實體檔案產生的行為，確保資料的一致性與錯誤補償邏輯。
- **專業日誌過濾與監控**: 實作自定義 Log 攔截器，過濾冗餘框架資訊，精準記錄存取 IP 與業務足跡。

## 🛠 技術堆疊

- **後端**: ASP.NET Core MVC (C#), Entity Framework Core
- **資料庫**: MySQL 8.0 
- **前端**: Razor View, JavaScript, CKEditor 5, Bootstrap 5

## 📥 快速啟動

1. **複製設定檔範本**:
   將 `appsettings.json.example` 複製並重新命名為 `appsettings.json`。
   
2. **設定環境變數**:
   請於 User Secrets 或設定檔中配置資料庫連線字串 `DefaultConnection` 與檔案存儲路徑 `FileStorage:UploadPath`。

3. **資料庫遷移**:
   ```bash
   dotnet ef database update --project KaoBulletin.Data --startup-project KaoBulletin.Web
