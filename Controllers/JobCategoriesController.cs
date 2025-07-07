using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SJOB_EXE201.Models;

namespace SJOB_EXE201.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobCategoriesController : ControllerBase
    {
        private readonly SjobContext _context;

        public JobCategoriesController(SjobContext context)
        {
            _context = context;
        }

        [HttpGet("GetCategories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.JobCategories
                    .Select(c => new
                    {
                        id = c.Id,
                        name = c.Name,
                        parentId = c.ParentId
                    })
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error loading categories", error = ex.Message });
            }
        }
    }
}