using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SJOB_EXE201.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SJOB_EXE201.ViewComponents
{
    public class JobCategoriesMenuViewComponent : ViewComponent
    {
        private readonly SjobContext _context;

        public JobCategoriesMenuViewComponent(SjobContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _context.JobCategories
                .Where(c => c.ParentId == null)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }
    }
}
