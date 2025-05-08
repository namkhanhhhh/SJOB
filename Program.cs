using Microsoft.EntityFrameworkCore;
using SJOB_EXE201.Models;
using SJOB_EXE201.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;
using Net.payOS.Types;
using Net.payOS;


var builder = WebApplication.CreateBuilder(args);

//payment
IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

builder.Services.AddSingleton<PayOSService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var payos = new PayOS(
        configuration["Environment:PAYOS_CLIENT_ID"] ?? throw new Exception("Missing PAYOS_CLIENT_ID"),
        configuration["Environment:PAYOS_API_KEY"] ?? throw new Exception("Missing PAYOS_API_KEY"),
        configuration["Environment:PAYOS_CHECKSUM_KEY"] ?? throw new Exception("Missing PAYOS_CHECKSUM_KEY")
    );

    return new PayOSService(payos, configuration);
});


// Add services to the container.
builder.Services.AddSingleton<EmailService>();
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<SjobContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DB"));
});

// Thêm Authentication với Cookie
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Login/Index";   // Trang đăng nhập nếu chưa xác thực
    options.LogoutPath = "/Login/Logout"; // Trang đăng xuất
    options.AccessDeniedPath = "/Login/AccessDenied"; // Trang từ chối truy cập
    options.SlidingExpiration = false; // Không gia hạn lại cookie khi có tương tác
})
.AddGoogle(options =>
{
    options.ClientId = "1002813393737-uto7g3e7mq9cnqiskk0o10eef2c8usqo.apps.googleusercontent.com";
    options.ClientSecret = "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";
    options.CallbackPath = "/signin-google";
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session hết hạn sau 30 phút
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDistributedMemoryCache(); // Cần thiết để sử dụng Session
var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<CheckUserStatusMiddleware>();
app.UseCustomExceptionMiddleware();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Worker}/{action=Index}/{id?}");

// Add a specific route for the root URL
app.MapControllerRoute(
    name: "home",
    pattern: "",
    defaults: new { controller = "Worker", action = "Index" });

// Map the HomePage route to ensure it's accessible
app.MapControllerRoute(
    name: "homePage",
    pattern: "HomePage",
    defaults: new { controller = "Worker", action = "Index" });

app.Run();
