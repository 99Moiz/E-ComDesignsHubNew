using EcommDesignsHub.Models;
using EcommDesignsHub.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EcommDesignsHub.Controllers.Api
{
    [ApiController]
    [Route("api/jobs")]
    public class JobController : ControllerBase
    {
        private readonly IJobRepository _repo;

        public JobController(IJobRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetJobs()
        {
            var jobs = await _repo.GetAllAsync();
            return Ok(jobs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetJob(int id)
        {
            var job = await _repo.GetByIdAsync(id);
            if (job == null) return NotFound();
            return Ok(job);
        }

        [HttpPost]
        public async Task<IActionResult> CreateJob(Job job)
        {
            await _repo.AddAsync(job);
            return Ok(job);
        }
    }
}
