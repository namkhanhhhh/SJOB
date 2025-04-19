using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Org.BouncyCastle.Crypto.Generators;
using SJOB_EXE201.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class ForgotPasswordController : Controller
{
    private readonly EmailService _emailService;
    private static readonly List<PasswordResetModel> _resetRequests = new();
    private readonly SjobContext _context;

    public ForgotPasswordController(SjobContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }


    [HttpGet]
    public IActionResult ViewForgotPassword()
    {
        return View("~/Views/Account/ForgotPassword.cshtml");
    }

    [HttpPost]
    public async Task<IActionResult> GoForgotPassword(string email)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user == null)
        {
            ModelState.AddModelError("", "Email không tồn tại.");
            return View();
        }

        // Tạo mã OTP ngẫu nhiên
        var otp = new Random().Next(100000, 999999).ToString();
        _resetRequests.Add(new PasswordResetModel
        {
            Email = email,
            OTP = otp,
            Expiration = DateTime.Now.AddMinutes(10)
        });
        var request = _resetRequests.FirstOrDefault(r => r.Email == email);
        Console.WriteLine($"Expiration: {request.Expiration} ,DateNow: {DateTime.Now} ");
        // Gửi email chứa mã OTP
        await _emailService.SendEmailAsync(email, "Reset Password",
            $"Mã xác nhận của bạn là: <b>{otp}</b> (Hết hạn sau 10 phút)");

        TempData["Message"] = "Mã xác nhận đã được gửi đến email của bạn.";
        ViewBag.Email = email;
        return View("~/Views/Account/VerifyOtp.cshtml");
    }

    [HttpPost]
    public IActionResult VerifyOtp(string email, string otp)
    {
        var request = _resetRequests.FirstOrDefault(r => r.Email == email && r.OTP == otp && r.Expiration > DateTime.Now);
       
        if (request == null)
        {
            ModelState.AddModelError("", "OTP không hợp lệ hoặc đã hết hạn.");
            return RedirectToAction("VerifyOtp");
        }

        return RedirectToAction("ResetPassword", new { email });
    }

    [HttpGet]
    public IActionResult ResetPassword(string email)
    {
        ViewBag.Email = email;
        return View("~/Views/Account/ResetPassword.cshtml");
    }

    [HttpPost]
    public IActionResult ResetPassword(string email, string newPassword)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user == null)
        {
            return NotFound();
        }
        using (SHA256 sha256 = SHA256.Create())
        {
            user.Password = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(newPassword)));
        }
     
        _context.SaveChanges();

        TempData["Message"] = "Mật khẩu đã được đặt lại thành công!";
        return View("~/Views/Account/Login.cshtml");
    }
}
