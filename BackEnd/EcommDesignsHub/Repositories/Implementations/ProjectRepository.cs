using EcommDesignsHub.Data;
using EcommDesignsHub.Models;
using EcommDesignsHub.Models.Dtos;
using EcommDesignsHub.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommDesignsHub.Repositories.Implementations
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProjectModel>> GetAll()
        {
            return await _context.Projects.ToListAsync();
        }
        public async Task<List<ProjectSiteDto>> GetAllForSite()
        {
            return await _context.Projects
                .Include(p => p.Category)
                .Where(p => p.IsActive == true)
                .Select(p => new ProjectSiteDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    Description = p.Description,
                    ImgUrl = p.ImgUrl,
                    ProjectUrl = p.ProjectUrl
                })
                .ToListAsync();
        }
        public async Task<ProjectModel> GetById(int id)
        {
            return await _context.Projects.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task Add(ProjectModel model)
        {
            await _context.Projects.AddAsync(model);
            await _context.SaveChangesAsync();
        }
        
        public async Task Update(ProjectModel model)
        {
            var existing = await _context.Projects
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (existing != null)
            {
                existing.Title = model.Title;
                existing.Description = model.Description;
                existing.CategoryId = model.CategoryId;
                existing.ProjectUrl = model.ProjectUrl;
                existing.ImgUrl = model.ImgUrl;
                existing.IsActive = model.IsActive;

                await _context.SaveChangesAsync();
            }
        }
        public async Task Delete(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<ProjectCategory>> GetAllCategory()
        {
            return await _context.Categories.ToListAsync();

        }

        public async Task<List<ProjectCategory>> GetAllCategoryForSite()
        {
            return await _context.Categories.Where(x=>x.IsActive != false).ToListAsync();
        }
        public async Task AddCategory(ProjectCategory model)
        {
            await _context.Categories.AddAsync(model);
            await _context.SaveChangesAsync();
        }

    }
}
