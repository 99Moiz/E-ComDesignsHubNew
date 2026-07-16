using EcommDesignsHub.Data;
using EcommDesignsHub.Models;
using EcommDesignsHub.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommDesignsHub.Repositories.Implementations
{
    public class JobRepository : IJobRepository
    {
        private readonly ApplicationDbContext _context;

        public JobRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Job>> GetAllAsync()
        {
            return await _context.Jobs.Where(j => j.IsActive).ToListAsync();
        }

        public async Task<Job> GetByIdAsync(int id)
        {
            return await _context.Jobs.FindAsync(id);
        }

        public async Task AddAsync(Job job)
        {
            await _context.Jobs.AddAsync(job);
            await _context.SaveChangesAsync();
        }
    }
}
