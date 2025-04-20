using Microsoft.EntityFrameworkCore;
using SJOB_EXE201.Models;
using SJOB_EXE201.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;


var builder = WebApplication.CreateBuilder(args);

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
})
.AddCookie(options =>
{
    options.LoginPath = "/Login/Index";   // Trang đăng nhập nếu chưa xác thực
    options.LogoutPath = "/Login/Logout"; // Trang đăng xuất
    options.AccessDeniedPath = "/Login/AccessDenied"; // Trang từ chối truy cập
                                                      //   options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Hết hạn sau 15 phút
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
//
app.UseMiddleware<CheckUserStatusMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
