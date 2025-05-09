using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SJOB_EXE201.Filters;
using SJOB_EXE201.Models;
using SJOB_EXE201.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SJOB_EXE201.Controllers
{
    [AuthorizationRequired(Roles = "Employer")]
    public class EmployerController : Controller
    {
        private readonly SjobContext _context;

        public EmployerController(SjobContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("UserId claim is missing or user not authenticated.");

            return int.Parse(userIdClaim);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = GetCurrentUserId();

            var user = await _context.Users
                .Include(u => u.UserDetails)
                .Include(u => u.CompanyProfiles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            var companyProfile = user.CompanyProfiles.FirstOrDefault();

            var viewModel = new EmployerProfileViewModel
            {
                Employer = user,
                UserDetail = user.UserDetails.FirstOrDefault(),
                CompanyProfile = companyProfile
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = GetCurrentUserId();

            var user = await _context.Users
                .Include(u => u.UserDetails)
                .Include(u => u.CompanyProfiles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            var userDetail = user.UserDetails.FirstOrDefault();
            var companyProfile = user.CompanyProfiles.FirstOrDefault();

            var viewModel = new EditEmployerProfileViewModel
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Avatar = user.Avatar,
                FirstName = userDetail?.FirstName,
                LastName = userDetail?.LastName,
                PhoneNumber = userDetail?.PhoneNumber,
                Address = userDetail?.Address,
                CompanyName = companyProfile?.CompanyName,
                CompanyDescription = companyProfile?.CompanyDescription,
                CompanyLogo = companyProfile?.CompanyLogo,
                CompanyWebsite = companyProfile?.CompanyWebsite,
                CompanySize = companyProfile?.CompanySize,
                Industry = companyProfile?.Industry
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditEmployerProfileViewModel model, IFormFile avatarFile, IFormFile logoFile)
        {

            var userId = GetCurrentUserId();

            var user = await _context.Users
                .Include(u => u.UserDetails)
                .Include(u => u.CompanyProfiles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            // Update user info
            user.Username = model.Username;
            user.Email = model.Email;

            // Handle avatar upload
            if (avatarFile != null && avatarFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(avatarFile.FileName);
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars");

                // Ensure directory exists
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                }

                user.Avatar = "/images/avatars/" + fileName;
            }

            // Update or create user details
            var userDetail = user.UserDetails.FirstOrDefault();
            if (userDetail == null)
            {
                userDetail = new UserDetail
                {
                    UserId = user.Id,
                    FirstName = model.FirstName ?? "",
                    LastName = model.LastName ?? "",
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    CreatedAt = DateTime.Now
                };
                _context.UserDetails.Add(userDetail);
            }
            else
            {
                userDetail.FirstName = model.FirstName ?? userDetail.FirstName;
                userDetail.LastName = model.LastName ?? userDetail.LastName;
                userDetail.PhoneNumber = model.PhoneNumber;
                userDetail.Address = model.Address;
            }

            // Update or create company profile
            var companyProfile = user.CompanyProfiles.FirstOrDefault();
            if (companyProfile == null)
            {
                companyProfile = new CompanyProfile
                {
                    UserId = user.Id,
                    CompanyName = model.CompanyName,
                    CompanyDescription = model.CompanyDescription,
                    CompanyWebsite = model.CompanyWebsite,
                    CompanySize = model.CompanySize,
                    Industry = model.Industry,
                    VerifiedBadge = false,
                    FreePostsRemaining = 5,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.CompanyProfiles.Add(companyProfile);
            }
            else
            {
                companyProfile.CompanyName = model.CompanyName;
                companyProfile.CompanyDescription = model.CompanyDescription;
                companyProfile.CompanyWebsite = model.CompanyWebsite;
                companyProfile.CompanySize = model.CompanySize;
                companyProfile.Industry = model.Industry;
                companyProfile.UpdatedAt = DateTime.Now;
            }

            // Handle company logo upload
            if (logoFile != null && logoFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(logoFile.FileName);
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logos");

                // Ensure directory exists
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await logoFile.CopyToAsync(stream);
                }

                companyProfile.CompanyLogo = "/images/logos/" + fileName;
            }

            // Save changes to database
            await _context.SaveChangesAsync();

            // Add success message
            TempData["Success"] = "Thông tin của bạn đã được cập nhật thành công!";

            // Redirect to profile page
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = GetCurrentUserId();

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Verify current password
            if (user.Password != HashPassword(model.CurrentPassword))
            {
                ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không chính xác");
                return View(model);
            }

            // Update password
            user.Password = HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Mật khẩu của bạn đã được thay đổi thành công!";
            return RedirectToAction("Profile");
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
            }
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var userId = GetCurrentUserId();

            // Get job post history
            var jobPosts = await _context.JobPosts
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .ToListAsync();

            // Get service order history
            var serviceOrders = await _context.ServiceOrders
                .Include(s => s.Service)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Take(10)
                .ToListAsync();

            // Get payment history
            var payments = await _context.Payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .ToListAsync();

            var viewModel = new EmployerHistoryViewModel
            {
                JobPosts = jobPosts,
                ServiceOrders = serviceOrders,
                Payments = payments
            };

            return View(viewModel);
        }
    }
}
