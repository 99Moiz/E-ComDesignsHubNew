using EcommDesignsHub.Data;
using EcommDesignsHub.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommDesignsHub.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectApiController : ControllerBase
    {
        private readonly IProjectRepository _repo;
        private readonly IWebHostEnvironment _env;

        public ProjectApiController(IProjectRepository repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env = env;
        }
        [HttpGet("Get")]
        public async Task<IActionResult> Get()
        {
            var projects = await _repo.GetAllForSite();
            return Ok(projects);
        }
        [HttpGet("GetCategory")]
        public async Task<IActionResult> GetCategory()
        {
            var categories = await _repo.GetAllCategoryForSite();

            
            return Ok(categories);
        }
    }
}
