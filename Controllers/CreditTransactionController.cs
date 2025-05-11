using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SJOB_EXE201.Data;
using SJOB_EXE201.Models;
using SJOB_EXE201.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SJOB_EXE201.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CreditTransactionController : Controller
    {
        private readonly SjobContext _context;
        private readonly int _pageSize = 10;

        public CreditTransactionController(SjobContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string searchEmail = "",
            DateTime? fromDate = null,
            DateTime? toDate = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            string transactionType = "",
            int page = 1,
            string sortBy = "Date",
            string sortOrder = "desc")
        {
            ViewData["CurrentSearchEmail"] = searchEmail;
            ViewData["CurrentFromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentToDate"] = toDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentMinAmount"] = minAmount;
            ViewData["CurrentMaxAmount"] = maxAmount;
            ViewData["CurrentTransactionType"] = transactionType;
            ViewData["CurrentPage"] = page;
            ViewData["CurrentSortBy"] = sortBy;
            ViewData["CurrentSortOrder"] = sortOrder;

            // Start with all credit transactions
            var query = _context.CreditTransactions
                .Include(t => t.User)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchEmail))
            {
                query = query.Where(t => t.User.Email.Contains(searchEmail));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(t => t.CreatedAt.HasValue && t.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                // Add one day to include the end date fully
                var endDate = toDate.Value.AddDays(1);
                query = query.Where(t => t.CreatedAt.HasValue && t.CreatedAt < endDate);
            }

            if (minAmount.HasValue)
            {
                query = query.Where(t => t.Amount >= minAmount.Value);
            }

            if (maxAmount.HasValue)
            {
                query = query.Where(t => t.Amount <= maxAmount.Value);
            }

            if (!string.IsNullOrEmpty(transactionType))
            {
                query = query.Where(t => t.TransactionType == transactionType);
            }

            // Apply sorting
            query = ApplySorting(query, sortBy, sortOrder);

            // Get total count for pagination
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)_pageSize);

            // Apply pagination
            var transactions = await query
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .ToListAsync();

            // Get transaction types for filter dropdown
            var transactionTypes = await _context.CreditTransactions
                .Select(t => t.TransactionType)
                .Distinct()
                .ToListAsync();

            // Create view model
            var viewModel = new CreditTransactionListViewModel
            {
                Transactions = transactions,
                TotalPages = totalPages,
                CurrentPage = page,
                SearchEmail = searchEmail,
                FromDate = fromDate,
                ToDate = toDate,
                MinAmount = minAmount,
                MaxAmount = maxAmount,
                TransactionType = transactionType,
                SortBy = sortBy,
                SortOrder = sortOrder,
                TransactionTypes = transactionTypes,
                TotalTransactions = totalItems,
                TotalAmount = await query.SumAsync(t => t.Amount)
            };

            return View(viewModel);
        }

        private IQueryable<CreditTransaction> ApplySorting(IQueryable<CreditTransaction> query, string sortBy, string sortOrder)
        {
            switch (sortBy)
            {
                case "Amount":
                    return sortOrder == "asc" ? query.OrderBy(t => t.Amount) : query.OrderByDescending(t => t.Amount);
                case "User":
                    return sortOrder == "asc" ? query.OrderBy(t => t.User.Username) : query.OrderByDescending(t => t.User.Username);
                case "Type":
                    return sortOrder == "asc" ? query.OrderBy(t => t.TransactionType) : query.OrderByDescending(t => t.TransactionType);
                case "Balance":
                    return sortOrder == "asc" ? query.OrderBy(t => t.BalanceAfter) : query.OrderByDescending(t => t.BalanceAfter);
                case "Date":
                default:
                    return sortOrder == "asc" ? query.OrderBy(t => t.CreatedAt) : query.OrderByDescending(t => t.CreatedAt);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var transaction = await _context.CreditTransactions
                .Include(t => t.User)
                .ThenInclude(u => u.UserDetails)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                return NotFound();
            }

            // Get related service order if applicable
            ServiceOrder serviceOrder = null;
            if (transaction.ReferenceId.HasValue && transaction.ReferenceType == "service")
            {
                serviceOrder = await _context.ServiceOrders
                    .Include(s => s.Service)
                    .FirstOrDefaultAsync(s => s.Id == transaction.ReferenceId.Value);
            }

            // Get related subscription if applicable
            Subscription subscription = null;
            if (transaction.ReferenceId.HasValue && transaction.ReferenceType == "subscription")
            {
                subscription = await _context.Subscriptions
                    .Include(s => s.Plan)
                    .FirstOrDefaultAsync(s => s.Id == transaction.ReferenceId.Value);
            }

            // Get user activity data
            var userId = transaction.UserId;
            var totalTransactions = await _context.CreditTransactions.CountAsync(t => t.UserId == userId);
            var totalTopup = await _context.CreditTransactions
                .Where(t => t.UserId == userId && t.TransactionType == "Topup")
                .SumAsync(t => t.Amount);
            var totalSpent = await _context.CreditTransactions
                .Where(t => t.UserId == userId && t.TransactionType != "Topup")
                .SumAsync(t => t.Amount);

            var userCredit = await _context.UserCredits.FirstOrDefaultAsync(c => c.UserId == userId);
            var creditBalance = userCredit?.Balance ?? 0;

            var activeServices = await _context.ServiceOrders.CountAsync(s => s.UserId == userId && s.Status == "active");
            var activeSubscriptions = await _context.Subscriptions.CountAsync(s => s.UserId == userId && s.Status == "active");

            // Get recent transactions
            var recentTransactions = await _context.CreditTransactions
                .Where(t => t.UserId == userId && t.Id != id)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Create view model
            var viewModel = new CreditTransactionDetailsViewModel
            {
                Transaction = transaction,
                ServiceOrder = serviceOrder,
                Subscription = subscription,
                TotalTransactions = totalTransactions,
                TotalTopup = totalTopup,
                TotalSpent = totalSpent,
                CreditBalance = creditBalance,
                ActiveServices = activeServices,
                ActiveSubscriptions = activeSubscriptions,
                RecentTransactions = recentTransactions
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // Get current date and calculate date ranges
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            var startOfYear = new DateTime(today.Year, 1, 1);
            var last12Months = today.AddMonths(-11);

            // Monthly statistics
            var monthlyStats = await _context.CreditTransactions
                .Where(t => t.CreatedAt.HasValue && t.CreatedAt >= startOfMonth && t.CreatedAt <= endOfMonth && t.TransactionType == "Topup")
                .GroupBy(t => 1)
                .Select(g => new
                {
                    Count = g.Count(),
                    TotalAmount = g.Sum(t => t.Amount)
                })
                .FirstOrDefaultAsync() ?? new { Count = 0, TotalAmount = 0m };

            // Yearly statistics
            var yearlyStats = await _context.CreditTransactions
                .Where(t => t.CreatedAt.HasValue && t.CreatedAt >= startOfYear && t.TransactionType == "Topup")
                .GroupBy(t => 1)
                .Select(g => new
                {
                    Count = g.Count(),
                    TotalAmount = g.Sum(t => t.Amount)
                })
                .FirstOrDefaultAsync() ?? new { Count = 0, TotalAmount = 0m };

            // Monthly transaction data for chart (last 12 months)
            var monthlyData = await _context.CreditTransactions
                .Where(t => t.CreatedAt.HasValue && t.CreatedAt >= last12Months && t.TransactionType == "Topup")
                .GroupBy(t => new {
                    Month = t.CreatedAt.Value.Month,
                    Year = t.CreatedAt.Value.Year
                })
                .Select(g => new MonthlyTransactionData
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    TransactionCount = g.Count(),
                    TotalAmount = g.Sum(t => t.Amount)
                })
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Month)
                .ToListAsync();

            // Format month names for chart
            foreach (var data in monthlyData)
            {
                data.MonthName = new DateTime(data.Year, data.Month, 1).ToString("MMM yyyy");
            }

            // Transaction type distribution
            var transactionTypeData = await _context.CreditTransactions
                .GroupBy(t => t.TransactionType)
                .Select(g => new TransactionTypeData
                {
                    Type = g.Key,
                    Count = g.Count(),
                    Amount = g.Sum(t => t.Amount)
                })
                .ToListAsync();

            // Top users by deposit amount
            var topUsers = await _context.CreditTransactions
                .Where(t => t.TransactionType == "Topup")
                .GroupBy(t => t.UserId)
                .Select(g => new TopUserData
                {
                    UserId = g.Key,
                    TransactionCount = g.Count(),
                    TotalAmount = g.Sum(t => t.Amount)
                })
                .OrderByDescending(u => u.TotalAmount)
                .Take(10)
                .ToListAsync();

            // Get user details for top users
            foreach (var user in topUsers)
            {
                var userData = await _context.Users
                    .Include(u => u.UserDetails)
                    .FirstOrDefaultAsync(u => u.Id == user.UserId);

                if (userData != null)
                {
                    user.Username = userData.Username;
                    user.Email = userData.Email;
                    var userDetail = userData.UserDetails.FirstOrDefault();
                    if (userDetail != null)
                    {
                        user.FullName = $"{userDetail.FirstName} {userDetail.LastName}";
                    }
                }
            }

            // Create view model
            var viewModel = new CreditTransactionDashboardViewModel
            {
                MonthlyTransactionCount = monthlyStats.Count,
                MonthlyTransactionAmount = monthlyStats.TotalAmount,
                YearlyTransactionCount = yearlyStats.Count,
                YearlyTransactionAmount = yearlyStats.TotalAmount,
                MonthlyData = monthlyData,
                TransactionTypeData = transactionTypeData,
                TopUsers = topUsers,
                TotalUsers = await _context.UserCredits.CountAsync(),
                TotalCredits = await _context.UserCredits.SumAsync(c => c.Balance ?? 0)
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ExportTransactions(
            string searchEmail = "",
            DateTime? fromDate = null,
            DateTime? toDate = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            string transactionType = "")
        {
            // Start with all credit transactions
            var query = _context.CreditTransactions
                .Include(t => t.User)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchEmail))
            {
                query = query.Where(t => t.User.Email.Contains(searchEmail));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(t => t.CreatedAt.HasValue && t.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                // Add one day to include the end date fully
                var endDate = toDate.Value.AddDays(1);
                query = query.Where(t => t.CreatedAt.HasValue && t.CreatedAt < endDate);
            }

            if (minAmount.HasValue)
            {
                query = query.Where(t => t.Amount >= minAmount.Value);
            }

            if (maxAmount.HasValue)
            {
                query = query.Where(t => t.Amount <= maxAmount.Value);
            }

            if (!string.IsNullOrEmpty(transactionType))
            {
                query = query.Where(t => t.TransactionType == transactionType);
            }

            // Order by date
            query = query.OrderByDescending(t => t.CreatedAt);

            // Get all transactions for export
            var transactions = await query.ToListAsync();

            // Create CSV content
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("ID,User,Email,Amount,Transaction Type,Balance After,Description,Date");

            foreach (var transaction in transactions)
            {
                csv.AppendLine($"{transaction.Id},{transaction.User.Username},{transaction.User.Email},{transaction.Amount},{transaction.TransactionType},{transaction.BalanceAfter},{transaction.Description},{(transaction.CreatedAt.HasValue ? transaction.CreatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "N/A")}");
            }

            // Return CSV file
            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"credit_transactions_{DateTime.Now:yyyyMMdd}.csv");
        }
    }
}
