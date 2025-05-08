using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SJOB_EXE201.Models;
using System;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using SJOB_EXE201.Utils;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SJOB_EXE201.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        private readonly SjobContext _context;
        private readonly int _pageSize = 10; // Default page size

        public AccountController(SjobContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm = "", int page = 1, string sortBy = "Username", string sortOrder = "asc")
        {
            ViewData["CurrentSearch"] = searchTerm;
            ViewData["CurrentPage"] = page;
            ViewData["CurrentSortBy"] = sortBy;
            ViewData["CurrentSortOrder"] = sortOrder;

            // Start with all users except admins
            var query = _context.Users
                .Include(u => u.Role)
                .Include(u => u.UserDetails)
                .Where(u => u.Role.Name != "Admin") // Exclude admin users
                .AsQueryable();

            // Apply search if provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u =>
                    u.Username.Contains(searchTerm) ||
                    u.Email.Contains(searchTerm) ||
                    u.UserDetails.Any(ud =>
                        ud.FirstName.Contains(searchTerm) ||
                        ud.LastName.Contains(searchTerm)));
            }

            // Apply sorting
            query = ApplySorting(query, sortBy, sortOrder);

            // Get total count for pagination
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)_pageSize);

            // Apply pagination
            var users = await query
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .ToListAsync();

            // Create view model
            var viewModel = new UserListViewModel
            {
                Users = users,
                TotalPages = totalPages,
                CurrentPage = page,
                SearchTerm = searchTerm,
                SortBy = sortBy,
                SortOrder = sortOrder,
                TotalUsers = await _context.Users.CountAsync(u => u.Role.Name != "Admin")
            };

            return View(viewModel);
        }

        private IQueryable<User> ApplySorting(IQueryable<User> query, string sortBy, string sortOrder)
        {
            switch (sortBy)
            {
                case "Email":
                    return sortOrder == "asc" ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email);
                case "Role":
                    return sortOrder == "asc" ? query.OrderBy(u => u.Role.Name) : query.OrderByDescending(u => u.Role.Name);
                case "Status":
                    return sortOrder == "asc" ? query.OrderBy(u => u.Status) : query.OrderByDescending(u => u.Status);
                case "Username":
                default:
                    return sortOrder == "asc" ? query.OrderBy(u => u.Username) : query.OrderByDescending(u => u.Username);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.UserDetails)
                .Include(u => u.UserCredits)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Toggle the status
            user.Status = !user.Status;

            await _context.SaveChangesAsync();

            TempData["Message"] = $"User {user.Username} has been {(user.Status ? "activated" : "deactivated")} successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [AllowAnonymous]  // Add this attribute
        public IActionResult Logout()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]  // Add this attribute
        public async Task<IActionResult> Logout(string returnUrl = null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "HomePage");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            // Get current user identity
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Home");
            }

            int userId = int.Parse(userIdClaim.Value);

            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.UserDetails)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            // Get current user identity
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Home");
            }

            int userId = int.Parse(userIdClaim.Value);

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Verify current password
            if (user.Password != HashPassword(currentPassword))
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect");
                return View();
            }

            // Confirm passwords match
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "New passwords do not match");
                return View();
            }

            // Update password
            user.Password = HashPassword(newPassword);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Password changed successfully!";
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
        public async Task<IActionResult> SearchUsers(string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return Json(new { success = false });
            }

            term = term.ToLower();

            var users = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.UserDetails)
                .Where(u =>
                    u.Role.Name != "Admin" && // Exclude admin users
                    (u.Username.ToLower().Contains(term) ||
                    u.Email.ToLower().Contains(term) ||
                    u.UserDetails.Any(ud =>
                        ud.FirstName.ToLower().Contains(term) ||
                        ud.LastName.ToLower().Contains(term))))
                .Take(10) // Limit results
                .Select(u => new {
                    u.Id,
                    u.Username,
                    u.Email,
                    FullName = u.UserDetails.FirstOrDefault() != null ?
                        u.UserDetails.FirstOrDefault().FirstName + " " + u.UserDetails.FirstOrDefault().LastName :
                        "N/A",
                    Role = u.Role.Name,
                    u.Status
                })
                .ToListAsync();

            return Json(new { success = true, users });
        }
    }
}