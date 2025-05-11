using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SJOB_EXE201.Models;


namespace SJOB_EXE201.Controllers;
[Authorize(Roles = "Employer")]

public class CustomerServiceController : Controller
    {
        private readonly SjobContext _context;

        public CustomerServiceController(SjobContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var additionalServices = _context.AdditionalServices
                .Where(x => x.IsActive == true)
                .ToList();

            var subscriptionPlans = _context.SubscriptionPlans
                .Where(x => x.IsActive == true)
                .ToList();

            return View((additionalServices, subscriptionPlans));
        }
    }

