using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SJOB_EXE201.Models;
using SJOB_EXE201.Utils;
using System.Security.Claims;

namespace SJOB_EXE201.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly SjobContext _context;

        public CustomerController(SjobContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Show the role selection page for customers
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SelectRole(string role)
        {
            // Validate role
            if (role != DbConstants.User_Role_WORKER && role != DbConstants.User_Role_EMPLOYER)
            {
                return BadRequest("Invalid role selection");
            }

            // Get current user ID from claims
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null)
            {
                return RedirectToAction("Index", "Login");
            }

            int userId = int.Parse(userIdClaim.Value);

            // Get the role ID from the database
            var selectedRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == role);
            if (selectedRole == null)
            {
                return NotFound("Role not found");
            }

            // Update user's role
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            user.RoleId = selectedRole.Id;
            await _context.SaveChangesAsync();

            // Update the user's claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties();

            // Sign out the current user
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Sign in with the updated claims
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity), authProperties);

            // Redirect based on the selected role
            if (role == DbConstants.User_Role_WORKER)
            {
                return RedirectToAction("Index", "HomePage");
            }
            else
            {
                return RedirectToAction("Index", "Employer");
            }
        }
    }
}
