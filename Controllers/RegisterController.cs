using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SJOB_EXE201.Models;
using System;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace SJOB_EXE201.Controllers
{
    public class RegisterController : Controller
    {
        private readonly SjobContext _context;

        public RegisterController(SjobContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("~/Views/Account/Register.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Index(string username, string password,
            string confirmPassword, string email, string firstName, string lastName)
        {
            // Kiểm tra email đã tồn tại chưa
            var userWithEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (userWithEmail != null)
            {
                TempData["Fail"] = "Email này đã được sử dụng!";
                return RedirectToAction("Index");
            }

            // Kiểm tra xác nhận mật khẩu
            if (password != confirmPassword)
            {
                TempData["Fail"] = "Mật khẩu xác nhận không khớp!";
                return RedirectToAction("Index");
            }


            // Lấy RoleId cho người dùng thông thường (Ví dụ: WORKER hoặc USER)
            // Cần thay đổi "WORKER" thành role mặc định phù hợp với hệ thống của bạn
            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer");
            if (defaultRole == null)
            {
                TempData["Fail"] = "Lỗi hệ thống: không thể xác định vai trò người dùng!";
                return RedirectToAction("Index");
            }

            // Hash mật khẩu trước khi lưu vào DB
            var newUser = new User
            {
                Username = username,
                Password = HashPassword(password),
                Email = email,
                RoleId = defaultRole.Id,
                Status = true,
                AuthProvider = "local"
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            var newUserDetail = new UserDetail
            {
                UserId = newUser.Id,
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTime.Now
            };

            _context.UserDetails.Add(newUserDetail);
            await _context.SaveChangesAsync();


            //thêm 5 lượt đăng vào đây 


            // Chuyển hướng về trang Login với thông báo thành công
            TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
            return Redirect("/Login");
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
            }
        }
    }
}