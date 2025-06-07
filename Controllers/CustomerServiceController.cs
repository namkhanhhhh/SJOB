using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShortJob.Models.ViewModels;
using SJOB_EXE201.Models;
using SJOB_EXE201.ViewModels;
using System.Text;


namespace SJOB_EXE201.Controllers;
[Authorize(Roles = "Employer")]

public class CustomerServiceController : Controller
{
    private readonly SjobContext _context;

    public CustomerServiceController(SjobContext context)
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

    public IActionResult Index()
    {
        var userID = GetCurrentUserId();
        var additionalServices = _context.AdditionalServices
            .Where(x => x.IsActive == true)
            .ToList();

        var subscriptionPlans = _context.SubscriptionPlans
            .Where(x => x.IsActive == true)
            .ToList();

        //lấy số tiền của người dùng
        var userCredit = _context.UserCredits.FirstOrDefault(x => x.UserId == userID);
        if (userCredit != null)
        {
            ViewData["Balance"] = userCredit.Balance / 1000;
        }
        else
        {
            ViewData["Balance"] = 0;
        }

        var model = new CustomerServiceViewModel
        {
            AdditionalServices = additionalServices,
            SubscriptionPlans = subscriptionPlans,
        };

        return View(model);
    }

    //hien thi trang buy
    public async Task<IActionResult> Buy(int id, string type)
    {
        var userId = GetCurrentUserId();
        ViewData["Type"] = type;
        ViewData["Id"] = id;

        // Thêm code để lấy số dư hiện tại
        var userCredit = await _context.UserCredits.FirstOrDefaultAsync(x => x.UserId == userId);
        if (userCredit != null)
        {
            ViewData["Balance"] = userCredit.Balance / 1000;
        }
        else
        {
            ViewData["Balance"] = 0;
        }

        Console.WriteLine($"[GET] Buy - type: {type}, id: {id}, Balance: {ViewData["Balance"]}");

        if (type.ToLower() == "service")
        {
            var service = await _context.AdditionalServices.FirstOrDefaultAsync(x => x.Id == id);
            if (service == null) return NotFound();

            var vm = new BuyViewModel
            {
                Type = "service",
                Id = id,
                Service = service
            };
            return View("Buy", vm);
        }
        else if (type.ToLower() == "combo")
        {
            var combo = await _context.SubscriptionPlans.FirstOrDefaultAsync(x => x.Id == id);
            if (combo == null) return NotFound();

            var vm = new BuyViewModel
            {
                Type = "combo",
                Id = id,
                Combo = combo
            };
            return View("Buy", vm);
        }

        return NotFound();
    }

    // xử lý thanh toán 
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Buy(int id, string type, int quantity)
    {
        try
        {
            var userId = GetCurrentUserId();
            Console.WriteLine($"[POST] Buy action started - UserId: {userId}, Type: {type}, Id: {id}, Quantity: {quantity}");

            // Check for UserCredits and create if it doesn't exist
            var userCredit = await _context.UserCredits.FirstOrDefaultAsync(x => x.UserId == userId);
            if (userCredit == null)
            {
                Console.WriteLine($"[INFO] Creating new UserCredits record for user {userId}");
                userCredit = new UserCredit
                {
                    UserId = userId,
                    Balance = 0, // Start with zero balance
                    LastUpdated = DateTime.Now
                };
                _context.UserCredits.Add(userCredit);
                await _context.SaveChangesAsync();

                TempData["Error"] = "Bạn chưa có số dư trong tài khoản. Vui lòng nạp tiền trước khi mua dịch vụ.";
                return RedirectToAction("Index");
            }

            // Check for UserPostCredits and create if it doesn't exist
            var postCredit = await _context.UserPostCredits.FirstOrDefaultAsync(x => x.UserId == userId);
            if (postCredit == null)
            {
                Console.WriteLine($"[INFO] Creating new UserPostCredits record for user {userId}");
                postCredit = new UserPostCredit
                {
                    UserId = userId,
                    PushToTopAvailable =0,
                    AuthenLogoAvailable =0,
                    SilverPostsAvailable = 0,
                    GoldPostsAvailable = 0,
                    DiamondPostsAvailable = 0,
                    LastUpdated = DateTime.Now
                };
                _context.UserPostCredits.Add(postCredit);
                await _context.SaveChangesAsync();
            }

            if (quantity < 1) quantity = 1;

            if (type == "service")
            {
                Console.WriteLine($"[POST] Processing service purchase - type: {type}, id: {id}, quantity: {quantity}");
                var service = await _context.AdditionalServices.FirstOrDefaultAsync(x => x.Id == id);
                if (service == null)
                {
                    Console.WriteLine($"[ERROR] Service with id {id} not found");
                    TempData["Error"] = "Không tìm thấy dịch vụ này.";
                    return RedirectToAction("Index");
                }

                // Chuyển đổi giá từ credit sang VND (1 credit = 1000 VND)
                var totalPriceInVND = service.Price * quantity * 1000;

                // So sánh với số dư (đã ở dạng VND)
                if (userCredit.Balance < totalPriceInVND)
                {
                    Console.WriteLine($"[INFO] Insufficient balance - Required: {totalPriceInVND} VND, Available: {userCredit.Balance} VND");
                    var missingAmount = (totalPriceInVND - userCredit.Balance) / 1000;
                    TempData["Error"] = $"Số dư không đủ để mua gói này. Bạn cần thêm {missingAmount} CD.";
                    return RedirectToAction("Buy", new { id, type });
                }

                // Use a transaction to ensure data consistency
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        userCredit.Balance -= totalPriceInVND; // Trừ số tiền VND
                        userCredit.LastUpdated = DateTime.Now;

                        if (service.ServiceType == "silver_post")
                        {
                            postCredit.SilverPostsAvailable += service.SilverPostsIncluded * quantity;
                        }
                        else if (service.ServiceType == "gold_post")
                        {
                            postCredit.GoldPostsAvailable += service.GoldPostsIncluded * quantity;
                        }
                        else if (service.ServiceType == "diamond_post")
                        {
                            postCredit.DiamondPostsAvailable += service.DiamondPostsIncluded * quantity;
                        }
                        else if (service.ServiceType == "push_to_top")
                        {
                            postCredit.PushToTopAvailable += service.PushToTopAvailable * quantity;
                        }
                        else if (service.ServiceType == "verified_badge")
                        {
                            postCredit.AuthenLogoAvailable += service.AuthenLogoAvailable * quantity;
                        }
                        else
                        {
                            throw new InvalidOperationException($"Unknown ServiceType: {service.ServiceType}");
                        }

                        postCredit.LastUpdated = DateTime.Now;

                        // Thêm mới: Tạo bản ghi ServiceOrder để theo dõi thời hạn
                        var startDate = DateTime.Now;
                        var endDate = service.DurationDays.HasValue
                            ? startDate.AddDays(service.DurationDays.Value * quantity)
                            : (DateTime?)null;

                        var serviceOrder = new ServiceOrder
                        {
                            UserId = userId,
                            ServiceId = service.Id,
                            StartDate = startDate,
                            EndDate = endDate,
                            Status = "active",
                            CreatedAt = DateTime.Now,
                            DiamondPostsApplied = service.ServiceType == "diamond_post" ? service.DiamondPostsIncluded * quantity : 0,
                            GoldPostsApplied = service.ServiceType == "gold_post" ? service.GoldPostsIncluded * quantity : 0,
                            SilverPostsApplied = service.ServiceType == "silver_post" ? service.SilverPostsIncluded * quantity : 0
                        };

                        _context.ServiceOrders.Add(serviceOrder);

                        await _context.SaveChangesAsync();

                        await AddTransaction(userId, totalPriceInVND, "purchase", service.Id, "service"
                                , (decimal)userCredit.Balance, $"Mua dịch vụ: {service.Name} x {quantity}");

                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine($"[EXCEPTION] Transaction failed: {ex.Message}");
                        throw;
                    }
                }
            }
            else if (type == "combo")
            {
                Console.WriteLine($"[POST] Processing combo purchase - type: {type}, id: {id}, quantity: {quantity}");
                var combo = await _context.SubscriptionPlans.FirstOrDefaultAsync(x => x.Id == id);
                if (combo == null)
                {
                    Console.WriteLine($"[ERROR] Combo with id {id} not found");
                    TempData["Error"] = "Không tìm thấy gói combo này.";
                    return RedirectToAction("Index");
                }

                // Chuyển đổi giá từ credit sang VND (1 credit = 1000 VND)
                var totalPriceInVND = combo.Price * quantity * 1000;

                // So sánh với số dư (đã ở dạng VND)
                if (userCredit.Balance < totalPriceInVND)
                {
                    Console.WriteLine($"[INFO] Insufficient balance - Required: {totalPriceInVND} VND, Available: {userCredit.Balance} VND");
                    var missingAmount = (totalPriceInVND - userCredit.Balance) / 1000;
                    TempData["Error"] = $"Số dư không đủ để mua gói combo này. Bạn cần thêm {missingAmount} CD.";
                    return RedirectToAction("Buy", new { id, type });
                }

                // Use a transaction to ensure data consistency
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        userCredit.Balance -= totalPriceInVND; // Trừ số tiền VND
                        userCredit.LastUpdated = DateTime.Now;

                        postCredit.SilverPostsAvailable += combo.SilverPosts * quantity;
                        postCredit.GoldPostsAvailable += combo.GoldPosts * quantity;
                        postCredit.DiamondPostsAvailable += combo.DiamondPosts * quantity;
                        postCredit.PushToTopAvailable += combo.PushTopTimes * quantity;
                        postCredit.LastUpdated = DateTime.Now;

                        // Thêm mới: Tạo bản ghi Subscription để theo dõi thời hạn
                        var startDate = DateTime.Now;
                        var endDate = startDate.AddDays(combo.DurationDays * quantity);

                        var subscription = new Subscription
                        {
                            UserId = userId,
                            PlanId = combo.Id,
                            SilverPostsRemaining = combo.SilverPosts * quantity,
                            GoldPostsRemaining = combo.GoldPosts * quantity,
                            DiamondPostsRemaining = combo.DiamondPosts * quantity,
                            PushTopRemaining = combo.PushTopTimes * quantity,
                            StartDate = startDate,
                            EndDate = endDate,
                            Status = "active",
                            CreatedAt = DateTime.Now
                        };

                        _context.Subscriptions.Add(subscription);

                        await _context.SaveChangesAsync();
                        await AddTransaction(userId, totalPriceInVND, "purchase", combo.Id, "combo"
                                , (decimal)userCredit.Balance, $"Mua combo: {combo.Name} x {quantity}");

                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine($"[EXCEPTION] Transaction failed: {ex.Message}");
                        throw;
                    }
                }
            }
            else
            {
                Console.WriteLine($"[ERROR] Invalid type: {type}");
                TempData["Error"] = "Loại dịch vụ không hợp lệ.";
                return RedirectToAction("Index");
            }

            Console.WriteLine($"[SUCCESS] Purchase completed successfully");
            TempData["Success"] = "Mua thành công!";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EXCEPTION] {ex.Message}");
            Console.WriteLine($"[EXCEPTION] {ex.StackTrace}");
            TempData["Error"] = "Đã xảy ra lỗi trong quá trình xử lý. Vui lòng thử lại sau.";
            return RedirectToAction("Index");
        }
    }

    //thêm vào lịch sử giao dịch
    private async Task AddTransaction(int userId, decimal amount, string type, int referenceId, string referenceType, decimal balanceAfter, string description)
    {
        var transaction = new CreditTransaction
        {
            UserId = userId,
            Amount = amount,
            TransactionType = type,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            BalanceAfter = balanceAfter,
            Description = description,
            CreatedAt = DateTime.Now
        };
        _context.CreditTransactions.Add(transaction);
        await _context.SaveChangesAsync();
    }

    //hiển thị các gói dịch vụ đang có 
    public async Task<IActionResult> UserPackages()
    {
        var userId = GetCurrentUserId();

        // Thêm log để kiểm tra userId
        System.Diagnostics.Debug.WriteLine($"Current User ID: {userId}");

        var userPackages = await GetUserPackagesAsync(userId);

        // Thêm log để kiểm tra userPackages
        if (userPackages != null)
        {
            System.Diagnostics.Debug.WriteLine($"Subscriptions count: {userPackages.Subscriptions?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"Additional Services count: {userPackages.AdditionalServices?.Count ?? 0}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("userPackages is null");
        }

        if (userPackages == null ||
            (!userPackages.Subscriptions.Any() && !userPackages.AdditionalServices.Any()))
        {
            // Thay vì trả về list rỗng, hãy trả về một model với thông báo
            ViewBag.Message = "Bạn chưa đăng ký gói dịch vụ nào.";
            return View(new List<UserPackagesViewModel>());
        }

        var result = new List<UserPackagesViewModel> { userPackages };

        // Kiểm tra kết quả cuối cùng
        System.Diagnostics.Debug.WriteLine($"Result count: {result.Count}");
        //lấy số tiền của người dùng
        var userCredit = await _context.UserCredits.FirstOrDefaultAsync(x => x.UserId == userId);
        if (userCredit != null)
        {
            ViewData["Balance"] = userCredit.Balance / 1000;
        }
        else
        {
            ViewData["Balance"] = 0;
        }
        return View(result);
    }


    private async Task<UserPackagesViewModel> GetUserPackagesAsync(int userId)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                System.Diagnostics.Debug.WriteLine($"User with ID {userId} not found");
                return null;
            }

            // Lấy thông tin credits của user
            var userCredits = await _context.UserPostCredits
                .FirstOrDefaultAsync(c => c.UserId == userId);

            // Nếu không tìm thấy, tạo một đối tượng mới với giá trị mặc định
            if (userCredits == null)
            {
                userCredits = new UserPostCredit
                {
                    UserId = userId,
                    SilverPostsAvailable = 0,
                    GoldPostsAvailable = 0,
                    DiamondPostsAvailable = 0,
                    PushToTopAvailable = 0,
                    AuthenLogoAvailable = 0,
                    LastUpdated = DateTime.Now
                };
            }

            var subscriptions = await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.UserId == userId)
                .ToListAsync();

            var services = await _context.ServiceOrders
                .Include(so => so.Service)
                .Where(so => so.UserId == userId)
                .ToListAsync();

            var today = DateTime.Now.Date;

            var viewModel = new UserPackagesViewModel
            {
                User = user,
                UserCredits = userCredits, // Thêm thông tin credits
                Subscriptions = subscriptions.Select(s => new UserSubscriptionViewModel
                {
                    Id = s.Id,
                    PlanName = s.Plan?.Name ?? "Unknown Plan",
                    StartDate = s.StartDate ?? s.CreatedAt ?? DateTime.Now,
                    EndDate = s.EndDate,
                    Status = s.Status ?? "Unknown",
                    DaysRemaining = (s.EndDate.Date - today).Days,
                    Description = s.Plan?.Description,
                    Price = s.Plan?.Price ?? 0
                }).ToList(),
                AdditionalServices = services.Select(so => new UserServiceViewModel
                {
                    Id = so.Id,
                    ServiceName = so.Service?.Name ?? "Unknown Service",
                    StartDate = so.StartDate ?? so.CreatedAt ?? DateTime.Now,
                    EndDate = so.EndDate,
                    Status = so.Status ?? "Unknown",
                    DaysRemaining = so.EndDate.HasValue ? (so.EndDate.Value.Date - today).Days : null,
                    Description = so.Service?.Description,
                    Price = so.Service?.Price ?? 0
                }).ToList()
            };

            return viewModel;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetUserPackagesAsync: {ex.Message}");
            // Có thể log lỗi ở đây
            return new UserPackagesViewModel
            {
                User = new User { Username = "Error", Email = "Error retrieving user data" },
                UserCredits = new UserPostCredit(), // Thêm đối tượng rỗng
                Subscriptions = new List<UserSubscriptionViewModel>(),
                AdditionalServices = new List<UserServiceViewModel>()
            };
        }
    }

    // Trong ServiceController hoặc controller hiện tại xử lý việc mua dịch vụ
    //đồng bộ giao dịch
    public async Task<IActionResult> SyncPurchaseHistory()
    {
        var result = new StringBuilder();

        try
        {
            // Lấy ID người dùng hiện tại
            int userId;
            try
            {
                userId = GetCurrentUserId();
            }
            catch (UnauthorizedAccessException ex)
            {
                // Xử lý trường hợp người dùng không đăng nhập
                TempData["Error"] = "Bạn cần đăng nhập để sử dụng chức năng này.";
                return RedirectToAction("Login", "Account"); // Điều hướng đến trang đăng nhập
            }

            // Kiểm tra xem người dùng có tồn tại trong cơ sở dữ liệu không
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Index", "Home"); // Hoặc trang khác phù hợp
            }

            result.AppendLine($"User: {user.Username} (ID: {userId})");

            // Lấy tất cả giao dịch mua dịch vụ
            var transactions = await _context.CreditTransactions
                .Where(t => t.UserId == userId && t.TransactionType == "purchase")
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();

            result.AppendLine($"Found {transactions.Count} purchase transactions");

            if (!transactions.Any())
            {
                ViewBag.Message = "Bạn chưa thực hiện giao dịch nào.";
                return RedirectToAction("UserPackages");
            }

            int subscriptionsCreated = 0;
            int serviceOrdersCreated = 0;

            foreach (var transaction in transactions)
            {
                if (!transaction.CreatedAt.HasValue)
                {
                    result.AppendLine($"Skipping transaction ID {transaction.Id} because CreatedAt is null");
                    continue; // Bỏ qua giao dịch này thay vì throw exception
                }

                if (transaction.ReferenceType == "combo")
                {
                    // Kiểm tra xem đã có subscription cho giao dịch này chưa
                    var existingSubscription = await _context.Subscriptions
                        .FirstOrDefaultAsync(s => s.UserId == userId && s.PlanId == transaction.ReferenceId &&
                                             s.CreatedAt.HasValue && s.CreatedAt.Value.Date == transaction.CreatedAt.Value.Date);

                    if (existingSubscription == null)
                    {
                        var plan = await _context.SubscriptionPlans.FindAsync(transaction.ReferenceId);
                        if (plan != null)
                        {
                            var startDate = transaction.CreatedAt ?? DateTime.Now;
                            var endDate = startDate.AddDays(plan.DurationDays);

                            var subscription = new Subscription
                            {
                                UserId = userId,
                                PlanId = plan.Id,
                                SilverPostsRemaining = plan.SilverPosts,
                                GoldPostsRemaining = plan.GoldPosts,
                                DiamondPostsRemaining = plan.DiamondPosts,
                                PushTopRemaining = plan.PushTopTimes,
                                StartDate = startDate,
                                EndDate = endDate,
                                Status = DateTime.Now > endDate ? "expired" : "active",
                                CreatedAt = transaction.CreatedAt
                            };

                            _context.Subscriptions.Add(subscription);
                            subscriptionsCreated++;
                        }
                        else
                        {
                            result.AppendLine($"Plan with ID {transaction.ReferenceId} not found for transaction {transaction.Id}");
                        }
                    }
                    else
                    {
                        result.AppendLine($"Subscription already exists for transaction {transaction.Id}");
                    }
                }
                else if (transaction.ReferenceType == "service")
                {
                    // Kiểm tra xem đã có service order cho giao dịch này chưa
                    var existingServiceOrder = await _context.ServiceOrders
                        .FirstOrDefaultAsync(s => s.UserId == userId && s.ServiceId == transaction.ReferenceId &&
                                             s.CreatedAt.HasValue && s.CreatedAt.Value.Date == transaction.CreatedAt.Value.Date);

                    if (existingServiceOrder == null)
                    {
                        var service = await _context.AdditionalServices.FindAsync(transaction.ReferenceId);
                        if (service != null)
                        {
                            var startDate = transaction.CreatedAt ?? DateTime.Now;
                            var endDate = service.DurationDays.HasValue
                                ? startDate.AddDays(service.DurationDays.Value)
                                : (DateTime?)null;

                            var serviceOrder = new ServiceOrder
                            {
                                UserId = userId,
                                ServiceId = service.Id,
                                StartDate = startDate,
                                EndDate = endDate,
                                Status = endDate.HasValue && DateTime.Now > endDate ? "expired" : "active",
                                CreatedAt = transaction.CreatedAt,
                                DiamondPostsApplied = service.ServiceType == "diamond_post" ? service.DiamondPostsIncluded : 0,
                                GoldPostsApplied = service.ServiceType == "gold_post" ? service.GoldPostsIncluded : 0,
                                SilverPostsApplied = service.ServiceType == "silver_post" ? service.SilverPostsIncluded : 0
                            };

                            _context.ServiceOrders.Add(serviceOrder);
                            serviceOrdersCreated++;
                        }
                        else
                        {
                            result.AppendLine($"Service with ID {transaction.ReferenceId} not found for transaction {transaction.Id}");
                        }
                    }
                    else
                    {
                        result.AppendLine($"Service Order already exists for transaction {transaction.Id}");
                    }
                }
                else
                {
                    result.AppendLine($"Unknown reference type: {transaction.ReferenceType} for transaction {transaction.Id}");
                }
            }

            await _context.SaveChangesAsync();

            result.AppendLine($"Created {subscriptionsCreated} subscriptions and {serviceOrdersCreated} service orders");

            // Thêm thông báo thành công
            if (subscriptionsCreated > 0 || serviceOrdersCreated > 0)
            {
                TempData["Success"] = $"Đã đồng bộ thành công {subscriptionsCreated} gói đăng ký và {serviceOrdersCreated} dịch vụ bổ sung.";
                return RedirectToAction("UserPackages");
            }
            else
            {
                TempData["Info"] = "Không có dữ liệu mới để đồng bộ.";

                // Nếu muốn hiển thị chi tiết kết quả đồng bộ (chỉ nên dùng trong môi trường phát triển)
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    return Content(result.ToString(), "text/plain");
                }

                return RedirectToAction("UserPackages");
            }
        }
        catch (Exception ex)
        {
            //code debug 

/*            result.AppendLine($"Error: {ex.Message}");
            if (ex.InnerException != null)
                result.AppendLine($"Inner Error: {ex.InnerException.Message}");

            // Thêm thông báo lỗi
            TempData["Error"] = $"Đã xảy ra lỗi khi đồng bộ dữ liệu: {ex.Message}";

            // Trong môi trường phát triển, hiển thị chi tiết lỗi
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                return Content(result.ToString(), "text/plain");
            }*/

            return RedirectToAction("UserPackages");
        }
    }

}


