using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SJOB_EXE201.Models;
using SJOB_EXE201.Utils;
using System.Data;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SJOB_EXE201.Controllers
{
    public class LoginController : Controller
    {
        private readonly SjobContext _context;

        public LoginController(SjobContext context)
        {
            _context = context;
        }

        #region Login
        public IActionResult Index()
        {
            // Kiểm tra thông báo thành công từ quá trình đăng ký
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"].ToString();
            }
            ViewBag.BanMessage = HttpContext.Session.GetString("BanMessage");
            HttpContext.Session.Remove("BanMessage"); // Xóa sau khi lấy ra
            return View("~/Views/Account/Login.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Find user by email instead of username
            var user = await _context.Users
                            .Include(u => u.Role) // Load Role liên quan
                            .FirstOrDefaultAsync(u => u.Email.Equals(email));

            if (user == null)
            {
                HttpContext.Session.SetString("BanMessage", "Wrong email or password!");
                return RedirectToAction("Index", "Login");
            }

            // Verify password
            if (!VerifyPassword(password, user.Password))
            {
                HttpContext.Session.SetString("BanMessage", "Wrong email or password!");
                return RedirectToAction("Index", "Login");
            }

            // Check if user is active
            if (!user.Status)
            {
                HttpContext.Session.SetString("BanMessage", "Your account has been banned. Please contact Admin");
                return RedirectToAction("Index", "Login");
            }

            // Lấy thông tin role từ người dùng (quan hệ 1-1)
            var roleName = user.Role.Name;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.Role, roleName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties();

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToAction("NavigateHomePageForRole", "Login", new { role = roleName });
        }

        private bool VerifyPassword(string password, string hashPassword)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                var computedHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
                return computedHash == hashPassword;
            }
        }
        #endregion

        #region Logout
        public async Task<IActionResult> Logout()
        {
            // Đăng xuất khỏi hệ thống
            await HttpContext.SignOutAsync();

            // Xóa tất cả cookies
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            // Chuyển hướng về trang đăng nhập hoặc trang chủ
            return RedirectToAction("Index", "Login");
        }
        #endregion

        public IActionResult AccessDenied()
        {
            return View();
        }

        // Đăng nhập 
        public IActionResult ExternalLogin(string provider)
        {
            //sau khi login xog Oauth qlai
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Login");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            // redirect trang login OAuth của provider
            return Challenge(properties, provider);
        }

        // Xử lý phản hồi 
        public async Task<IActionResult> ExternalLoginCallback()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            // Lấy thông tin từ OAuth
            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var providerIdClaim = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            var provider = providerIdClaim?.Issuer;
            var providerId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (email == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra xem user đã tồn tại trong database chưa
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                // Lấy role Customer từ DB
                var customerRole = _context.Roles.FirstOrDefault(r => r.Name.Equals(DbConstants.User_Role_CUSTOMER));

                // Nếu không tìm thấy, trả về trang đăng nhập
                if (customerRole == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                user = new User
                {
                    Username = email.Split('@')[0] ?? "Unknown",
                    Email = email,
                    AuthProvider = provider,
                    AuthProviderId = providerId,
                    RoleId = customerRole.Id, // Gán RoleId trực tiếp
                    Status = true,
                    Password = "a"
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                var userDetail = new UserDetail();
                if ("Google".Equals((provider)))
                {
                    userDetail = new UserDetail
                    {
                        UserId = user.Id,
                        FirstName = name.Split(' ')[^1],
                        LastName = string.Join(" ", name.Split(' ')[..^1])

                    };
                }
                else
                {
                    userDetail = new UserDetail
                    {
                        UserId = user.Id,
                        FirstName = name.Split('@')[0],
                        LastName = name.Split('@')[0]

                    };
                }
                _context.UserDetails.Add(userDetail);
                await _context.SaveChangesAsync();
            }

            //// Tạo claims để đăng nhập
            var claimsIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, user.Username));
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
            claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claimsIdentity.AddClaim(new Claim("UserId", user.Id.ToString()));
            // Lấy role name của user
            var userWithRole = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            if (userWithRole?.Role != null)
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, userWithRole.Role.Name));
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            if (string.IsNullOrEmpty(userWithRole.Role.Name))
                return RedirectToAction("Index", "Login");

            else if (DbConstants.User_Role_Admin.Equals(userWithRole.Role.Name))
                return RedirectToAction("Index", "Account");
            else if (DbConstants.User_Role_CUSTOMER.Equals(userWithRole.Role.Name))
                return RedirectToAction("Index", "Customer"); // Redirect to Customer controller
            else if (DbConstants.User_Role_WORKER.Equals(userWithRole.Role.Name))
                return RedirectToAction("Index", "Worker");
            else if (DbConstants.User_Role_EMPLOYER.Equals(userWithRole.Role.Name))
                return RedirectToAction("Index", "Employer");

            return RedirectToAction("Index", "Customer"); // Default to Customer
        }

        [HttpGet]
        public IActionResult NavigateHomePageForRole(string role)
        {
            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Index", "Login");

            else if (DbConstants.User_Role_Admin.Equals(role))
                return RedirectToAction("Index", "Account");
            else if (DbConstants.User_Role_CUSTOMER.Equals(role))
                return RedirectToAction("Index", "Customer"); // Redirect to Customer controller
            else if (DbConstants.User_Role_WORKER.Equals(role))
                return RedirectToAction("Index", "Worker");
            else if (DbConstants.User_Role_EMPLOYER.Equals(role))
                return RedirectToAction("Index", "Employer");

            return RedirectToAction("Index", "Customer"); // Default to Customer
        }
    }
}
