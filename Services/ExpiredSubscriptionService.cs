using Microsoft.EntityFrameworkCore;
using SJOB_EXE201.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SJOB_EXE201.Services
{
    public class ExpiredSubscriptionService
    {
        private readonly SjobContext _context;
        private readonly ILogger<ExpiredSubscriptionService> _logger;

        public ExpiredSubscriptionService(SjobContext context, ILogger<ExpiredSubscriptionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ProcessExpiredSubscriptions()
        {
            var today = DateTime.Now.Date;

            // Lấy tất cả các gói đăng ký đã hết hạn nhưng chưa được xử lý
            var expiredSubscriptions = await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.EndDate.Date < today && s.Status == "active")
                .ToListAsync();

            _logger.LogInformation($"Found {expiredSubscriptions.Count} expired subscriptions to process");

            foreach (var subscription in expiredSubscriptions)
            {
                try
                {
                    // Lấy UserPostCredit của người dùng
                    var userPostCredit = await _context.UserPostCredits
                        .FirstOrDefaultAsync(upc => upc.UserId == subscription.UserId);

                    if (userPostCredit == null)
                    {
                        _logger.LogWarning($"UserPostCredit not found for user {subscription.UserId}");
                        continue;
                    }

                    // Trừ các lượt đăng tương ứng
                    userPostCredit.SilverPostsAvailable -= subscription.SilverPostsRemaining;
                    userPostCredit.GoldPostsAvailable -= subscription.GoldPostsRemaining;
                    userPostCredit.DiamondPostsAvailable -= subscription.DiamondPostsRemaining;
                    userPostCredit.PushToTopAvailable -= subscription.PushTopRemaining;

                    // Đảm bảo không có giá trị âm
                    userPostCredit.SilverPostsAvailable = Math.Max(0, userPostCredit.SilverPostsAvailable);
                    userPostCredit.GoldPostsAvailable = Math.Max(0, userPostCredit.GoldPostsAvailable);
                    userPostCredit.DiamondPostsAvailable = Math.Max(0, userPostCredit.DiamondPostsAvailable);
                    userPostCredit.PushToTopAvailable = Math.Max(0, userPostCredit.PushToTopAvailable);

                    userPostCredit.LastUpdated = DateTime.Now;

                    // Cập nhật trạng thái gói đăng ký
                    subscription.Status = "expired";

                    // Thêm ghi chú về việc trừ lượt
                    await AddCreditTransaction(
                        subscription.UserId,
                        0, // Không trừ tiền
                        "subscription_expired",
                        subscription.Id,
                        "subscription",
                        0, // Balance không thay đổi
                        $"Gói đăng ký {subscription.Plan?.Name} hết hạn: -({subscription.SilverPostsRemaining} Silver, {subscription.GoldPostsRemaining} Gold, {subscription.DiamondPostsRemaining} Diamond, {subscription.PushTopRemaining} Push)"
                    );

                    _logger.LogInformation($"Processed expired subscription {subscription.Id} for user {subscription.UserId}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing expired subscription {subscription.Id}");
                }
            }

            // Xử lý các dịch vụ bổ sung đã hết hạn
            var expiredServices = await _context.ServiceOrders
                .Include(so => so.Service)
                .Where(so => so.EndDate.HasValue && so.EndDate.Value.Date < today && so.Status == "active")
                .ToListAsync();

            _logger.LogInformation($"Found {expiredServices.Count} expired services to process");

            foreach (var serviceOrder in expiredServices)
            {
                try
                {
                    // Lấy UserPostCredit của người dùng
                    var userPostCredit = await _context.UserPostCredits
                        .FirstOrDefaultAsync(upc => upc.UserId == serviceOrder.UserId);

                    if (userPostCredit == null)
                    {
                        _logger.LogWarning($"UserPostCredit not found for user {serviceOrder.UserId}");
                        continue;
                    }

                    // Trừ các lượt đăng tương ứng dựa vào loại dịch vụ
                    if (serviceOrder.Service?.ServiceType == "silver_post")
                    {
                        userPostCredit.SilverPostsAvailable -= serviceOrder.SilverPostsApplied ?? 0;
                        userPostCredit.SilverPostsAvailable = Math.Max(0, userPostCredit.SilverPostsAvailable);
                    }
                    else if (serviceOrder.Service?.ServiceType == "gold_post")
                    {
                        userPostCredit.GoldPostsAvailable -= serviceOrder.GoldPostsApplied ?? 0;
                        userPostCredit.GoldPostsAvailable = Math.Max(0, userPostCredit.GoldPostsAvailable);
                    }
                    else if (serviceOrder.Service?.ServiceType == "diamond_post")
                    {
                        userPostCredit.DiamondPostsAvailable -= serviceOrder.DiamondPostsApplied ?? 0;
                        userPostCredit.DiamondPostsAvailable = Math.Max(0, userPostCredit.DiamondPostsAvailable);
                    }
                    else if (serviceOrder.Service?.ServiceType == "push_to_top")
                    {
                        userPostCredit.PushToTopAvailable -= serviceOrder.Service.PushToTopAvailable;
                        userPostCredit.PushToTopAvailable = Math.Max(0, userPostCredit.PushToTopAvailable);
                    }
                    else if (serviceOrder.Service?.ServiceType == "verified_badge")
                    {
                        userPostCredit.AuthenLogoAvailable -= serviceOrder.Service.AuthenLogoAvailable;
                        userPostCredit.AuthenLogoAvailable = Math.Max(0, userPostCredit.AuthenLogoAvailable);

                        // Kiểm tra còn gói VerifiedBadge active nào không
                        var hasActiveVerifiedBadge = await _context.ServiceOrders
                            .AnyAsync(so => so.UserId == serviceOrder.UserId
                                            && so.Status == "active"
                                            && so.Service.ServiceType == "verified_badge");

                        if (!hasActiveVerifiedBadge)
                        {
                            var companyProfile = await _context.CompanyProfiles
                                .FirstOrDefaultAsync(cp => cp.UserId == serviceOrder.UserId);

                            if (companyProfile != null)
                            {
                                companyProfile.VerifiedBadge = false;
                            }
                        }
                    }

                    userPostCredit.LastUpdated = DateTime.Now;

                    // Cập nhật trạng thái dịch vụ
                    serviceOrder.Status = "expired";

                    // Thêm ghi chú về việc trừ lượt
                    await AddCreditTransaction(
                        serviceOrder.UserId,
                        0, // Không trừ tiền
                        "service_expired",
                        serviceOrder.Id,
                        "service",
                        0, // Balance không thay đổi
                        $"Dịch vụ {serviceOrder.Service?.Name} hết hạn"
                    );

                    _logger.LogInformation($"Processed expired service {serviceOrder.Id} for user {serviceOrder.UserId}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing expired service {serviceOrder.Id}");
                }
            }

            // Lưu tất cả thay đổi
            await _context.SaveChangesAsync();
        }

        private async Task AddCreditTransaction(int userId, decimal amount, string type, int referenceId, string referenceType, decimal balanceAfter, string description)
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
        }

        // Thêm vào ExpiredSubscriptionService
        public async Task ProcessExpiredSubscriptionsForUser(int userId)
        {
            var today = DateTime.Now.Date;

            // Lấy tất cả các gói đăng ký đã hết hạn của người dùng
            var expiredSubscriptions = await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.UserId == userId && s.EndDate.Date < today && s.Status == "active")
                .ToListAsync();

            _logger.LogInformation($"Found {expiredSubscriptions.Count} expired subscriptions for user {userId}");

            // Lấy UserPostCredit của người dùng
            var userPostCredit = await _context.UserPostCredits
                .FirstOrDefaultAsync(upc => upc.UserId == userId);

            if (userPostCredit == null)
            {
                _logger.LogWarning($"UserPostCredit not found for user {userId}");
                return;
            }

            // Xử lý từng gói đăng ký hết hạn
            foreach (var subscription in expiredSubscriptions)
            {
                try
                {
                    // Trừ các lượt đăng tương ứng
                    userPostCredit.SilverPostsAvailable -= subscription.SilverPostsRemaining;
                    userPostCredit.GoldPostsAvailable -= subscription.GoldPostsRemaining;
                    userPostCredit.DiamondPostsAvailable -= subscription.DiamondPostsRemaining;
                    userPostCredit.PushToTopAvailable -= subscription.PushTopRemaining;

                    // Đảm bảo không có giá trị âm
                    userPostCredit.SilverPostsAvailable = Math.Max(0, userPostCredit.SilverPostsAvailable);
                    userPostCredit.GoldPostsAvailable = Math.Max(0, userPostCredit.GoldPostsAvailable);
                    userPostCredit.DiamondPostsAvailable = Math.Max(0, userPostCredit.DiamondPostsAvailable);
                    userPostCredit.PushToTopAvailable = Math.Max(0, userPostCredit.PushToTopAvailable);

                    userPostCredit.LastUpdated = DateTime.Now;

                    // Cập nhật trạng thái gói đăng ký
                    subscription.Status = "expired";

                    // Thêm ghi chú về việc trừ lượt
                    await AddCreditTransaction(
                        subscription.UserId,
                        0, // Không trừ tiền
                        "subscription_expired",
                        subscription.Id,
                        "subscription",
                        0, // Balance không thay đổi
                        $"Gói đăng ký {subscription.Plan?.Name} hết hạn: -({subscription.SilverPostsRemaining} Silver, {subscription.GoldPostsRemaining} Gold, {subscription.DiamondPostsRemaining} Diamond, {subscription.PushTopRemaining} Push)"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing expired subscription {subscription.Id}");
                }
            }

            // Xử lý các dịch vụ bổ sung đã hết hạn
            var expiredServices = await _context.ServiceOrders
                .Include(so => so.Service)
                .Where(so => so.UserId == userId && so.EndDate.HasValue && so.EndDate.Value.Date < today && so.Status == "active")
                .ToListAsync();

            _logger.LogInformation($"Found {expiredServices.Count} expired services for user {userId}");

            foreach (var serviceOrder in expiredServices)
            {
                try
                {
                    // Trừ các lượt đăng tương ứng dựa vào loại dịch vụ
                    if (serviceOrder.Service?.ServiceType == "silver_post")
                    {
                        userPostCredit.SilverPostsAvailable -= serviceOrder.SilverPostsApplied ?? 0;
                        userPostCredit.SilverPostsAvailable = Math.Max(0, userPostCredit.SilverPostsAvailable);
                    }
                    else if (serviceOrder.Service?.ServiceType == "gold_post")
                    {
                        userPostCredit.GoldPostsAvailable -= serviceOrder.GoldPostsApplied ?? 0;
                        userPostCredit.GoldPostsAvailable = Math.Max(0, userPostCredit.GoldPostsAvailable);
                    }
                    else if (serviceOrder.Service?.ServiceType == "diamond_post")
                    {
                        userPostCredit.DiamondPostsAvailable -= serviceOrder.DiamondPostsApplied ?? 0;
                        userPostCredit.DiamondPostsAvailable = Math.Max(0, userPostCredit.DiamondPostsAvailable);
                    }
                    else if (serviceOrder.Service?.ServiceType == "push_to_top")
                    {
                        userPostCredit.PushToTopAvailable -= serviceOrder.Service.PushToTopAvailable;
                        userPostCredit.PushToTopAvailable = Math.Max(0, userPostCredit.PushToTopAvailable);
                    }
                    else if (serviceOrder.Service?.ServiceType == "verified_badge")
                    {
                        userPostCredit.AuthenLogoAvailable -= serviceOrder.Service.AuthenLogoAvailable;
                        userPostCredit.AuthenLogoAvailable = Math.Max(0, userPostCredit.AuthenLogoAvailable);
                    }

                    // Cập nhật trạng thái dịch vụ
                    serviceOrder.Status = "expired";

                    // Thêm ghi chú về việc trừ lượt
                    await AddCreditTransaction(
                        serviceOrder.UserId,
                        0, // Không trừ tiền
                        "service_expired",
                        serviceOrder.Id,
                        "service",
                        0, // Balance không thay đổi
                        $"Dịch vụ {serviceOrder.Service?.Name} hết hạn"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing expired service {serviceOrder.Id}");
                }
            }

            // Lưu tất cả thay đổi
            await _context.SaveChangesAsync();
        }

        /*// Thêm vào ExpiredSubscriptionService
        public async Task NotifyExpiringSubscriptions()
        {
            var today = DateTime.Now.Date;
            var threeDaysLater = today.AddDays(3);
            var sevenDaysLater = today.AddDays(7);

            // Lấy các gói sắp hết hạn trong 3 ngày và 7 ngày tới
            var expiringSubscriptions = await _context.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.User)
                .Where(s => s.Status == "active" &&
                           (s.EndDate.Date == threeDaysLater || s.EndDate.Date == sevenDaysLater))
                .ToListAsync();

            foreach (var subscription in expiringSubscriptions)
            {
                var daysRemaining = (subscription.EndDate.Date - today).Days;

                // Gửi email thông báo
                await _emailService.SendExpirationNotificationAsync(
                    subscription.User.Email,
                    subscription.User.Username,
                    subscription.Plan.Name,
                    subscription.EndDate,
                    daysRemaining);

                // Hoặc thêm thông báo vào hệ thống
                await _notificationService.AddNotificationAsync(
                    subscription.UserId,
                    "subscription_expiring",
                    $"Gói đăng ký {subscription.Plan.Name} của bạn sẽ hết hạn sau {daysRemaining} ngày.");
            }

            // Tương tự cho các dịch vụ bổ sung
            var expiringServices = await _context.ServiceOrders
                .Include(so => so.Service)
                .Include(so => so.User)
                .Where(so => so.Status == "active" && so.EndDate.HasValue &&
                            (so.EndDate.Value.Date == threeDaysLater || so.EndDate.Value.Date == sevenDaysLater))
                .ToListAsync();

            foreach (var service in expiringServices)
            {
                var daysRemaining = (service.EndDate.Value.Date - today).Days;

                // Gửi email thông báo
                await _emailService.SendExpirationNotificationAsync(
                    service.User.Email,
                    service.User.Username,
                    service.Service.Name,
                    service.EndDate.Value,
                    daysRemaining);

                // Hoặc thêm thông báo vào hệ thống
                await _notificationService.AddNotificationAsync(
                    service.UserId,
                    "service_expiring",
                    $"Dịch vụ {service.Service.Name} của bạn sẽ hết hạn sau {daysRemaining} ngày.");
            }
        }*/
    }
}
