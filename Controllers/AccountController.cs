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

            // Start with all users
            var query = _context.Users
                .Include(u => u.Role)
                .Include(u => u.UserDetails)
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
                SortOrder = sortOrder
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
        public async Task<IActionResult> CreateAccount()
        {
            var roleList = await _context.Roles
             .Select(r => new SelectListItem
             {
                 Value = r.Id.ToString(),
                 Text = r.Name
             })
             .ToListAsync();

            ViewBag.RoleList = roleList;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(string username, string password,
            string confirmPassword, string email, int roleId, string firstName, string lastName)
        {
            var roleList = await _context.Roles
             .Select(r => new SelectListItem
             {
                 Value = r.Id.ToString(),
                 Text = r.Name
             })
             .ToListAsync();

            ViewBag.RoleList = roleList;

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                TempData["Fail"] = "Exsited Email!";
                return View();
            }
            // Kiểm tra xác nhận mật khẩu
            if (password != confirmPassword)
            {
                TempData["Fail"] = "Passwords do not match";
                return View();
            }
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);

            // Hash mật khẩu trước khi lưu vào DB
            var newUser = new User
            {
                Username = username,
                Password = HashPassword(password),
                Email = email,
                RoleId = roleId,
                Status = true
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            var newUserDetail = new UserDetail
            {
                UserId = newUser.Id,
                LastName = lastName,
                FirstName = firstName,
                CreatedAt = DateTime.Now
            };

            _context.UserDetails.Add(newUserDetail);

            // Create user credit record
            var userCredit = new UserCredit
            {
                UserId = newUser.Id,
                Balance = 0,
                LastUpdated = DateTime.Now
            };

            _context.UserCredits.Add(userCredit);

            await _context.SaveChangesAsync();
            TempData["Message"] = "Account created successfully!";
            return RedirectToAction(nameof(Index));
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
            }
        }

        // Add this method to your AccountController class

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
                    u.Username.ToLower().Contains(term) ||
                    u.Email.ToLower().Contains(term) ||
                    u.UserDetails.Any(ud =>
                        ud.FirstName.ToLower().Contains(term) ||
                        ud.LastName.ToLower().Contains(term)))
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
