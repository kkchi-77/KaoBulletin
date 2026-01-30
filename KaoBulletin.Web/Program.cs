using KaoBulletin.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// --- [Gao's Architecture] Database Configuration Start ---

if (builder.Environment.EnvironmentName.StartsWith("Development"))
{
    // 強制載入，這會覆蓋掉 appsettings.json 裡的 127.0.0.1
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 檢查連線字串是否為空 (防呆)
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

// 註冊 DbContext 使用 MySQL
builder.Services.AddDbContext<KaoBulletinDbContext>(options =>
    options.UseMySql(connectionString,
new MySqlServerVersion(new Version(8, 0, 45)),
mysqlOptions =>
        {
            // 設定 Migration 所在的專案 (因為 Context 在 Data 層，但執行點在 Web 層)
            mysqlOptions.MigrationsAssembly("KaoBulletin.Data");
        }
    ));
// --- Database Configuration End ---

builder.Services.AddScoped<KaoBulletin.Services.Services.IBulletinService, KaoBulletin.Services.Services.BulletinService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

// --- [DEBUG] 檢查連線字串 ---
var debugConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//Console.WriteLine($"[DEBUG] Connection String: {debugConnectionString}");
// ---------------------------

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles(); // 預設 wwwroot

app.Use(async (context, next) =>
{
    var ip = context.Connection.RemoteIpAddress?.ToString();
    var path = context.Request.Path;
    var method = context.Request.Method;

    // 使用自定義標籤，方便搜尋
    Console.WriteLine($"[Access_Log] IP: {ip} | Method: {method} | Path: {path}");

    await next();
});

app.UseRouting(); // 1. 先啟動路由

// --- [Gao's Architecture] External Static Files Mapping ---

// 1. 取得基礎路徑 (D:\Projects\KaoBulletin_Uploads)
string uploadPath = builder.Configuration["FileStorage:UploadPath"]
                    ?? "D:\\Projects\\KaoBulletin_Uploads";

// 2. 確保「完整層級」目錄存在 (包含 images 子目錄)
string imagesPath = Path.Combine(uploadPath, "images");

// Directory.CreateDirectory 的強大之處在於：如果父目錄不存在，它會連父目錄一起建
if (!Directory.Exists(imagesPath))
{
    Directory.CreateDirectory(imagesPath);
}

// 3. 設定網址映射
app.UseStaticFiles(new StaticFileOptions
{
    // 指向父目錄，這樣網址 /uploads/images 才能對應到實體 images 資料夾
    FileProvider = new PhysicalFileProvider(uploadPath),
    RequestPath = "/uploads"
});

// ---------------------------------------------------------

app.UseAuthorization();

//app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
