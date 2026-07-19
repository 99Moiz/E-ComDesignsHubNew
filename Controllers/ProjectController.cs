using EcommDesignsHub.Data;
using EcommDesignsHub.Models;
using EcommDesignsHub.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EcommDesignsHub.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly IProjectRepository _repo;
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;

        public ProjectController(IProjectRepository repo, IWebHostEnvironment env,ApplicationDbContext context)
        {
            _repo = repo;
            _env = env;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _repo.GetAll();
            return View(data);
        }
        private async Task LoadCategories()
        {
            ViewBag.Categories = new SelectList(
                await _context.Categories.ToListAsync(),
                "Id",
                "Name"
            );
        }


        public async Task<IActionResult> Category()
        {
            var data = await _repo.GetAllCategory();
            return View(data);
        }
        public IActionResult Createcategory()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Createcategory(ProjectCategory model)
        {

            await _repo.AddCategory(model);

            TempData["success"] = "Category Created Successfully!";
            return RedirectToAction("Category");
            

        }
        

        public IActionResult DeleteCategory(int id)
        {
          var data =   _context.Categories.FirstOrDefault(x=>x.Id == id);
            _context.Remove(data!);
            _context.SaveChanges();

            TempData["success"] = "Category Delete Successfully!";
            return RedirectToAction("Category");
        }

        public async Task<IActionResult> Create()
        {
            await LoadCategories();
            return View();
        }
       

        [HttpPost]
        public async Task<IActionResult> Create(ProjectModel model, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategories();
                TempData["error"] = "Somnthing Went Wrong !";
                return View(model);
            }

            if (imageFile != null)
            {
                string folder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                string path = Path.Combine(folder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                model.ImgUrl = "/uploads/" + fileName;
            }

            await _repo.Add(model);

            TempData["success"] = "Project Created Successfully!";
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Edit(int id)
        {
            var project = await _repo.GetById(id);
            if (project == null) return NotFound();

            await LoadCategories();
            return View(project);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProjectModel model, IFormFile imageFile)
        {

            if (!ModelState.IsValid)
            {
                await LoadCategories();
                return View(model);
            }

            var existing = await _repo.GetById(model.Id);

            if (imageFile != null)
            {
                // Delete old image
                if (!string.IsNullOrEmpty(existing.ImgUrl))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, existing.ImgUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                string folder = Path.Combine(_env.WebRootPath, "uploads");
                string fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                string path = Path.Combine(folder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                model.ImgUrl = "/uploads/" + fileName;
            }
            else
            {
                model.ImgUrl = existing.ImgUrl;
            }

            await _repo.Update(model);

            TempData["success"] = "Project Updated Successfully!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var project = await _repo.GetById(id);

            if (project.ImgUrl != null)
            {
                var oldPath = Path.Combine(_env.WebRootPath, project.ImgUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            await _repo.Delete(id);

            TempData["success"] = "Project Deleted Successfully!";
            return RedirectToAction("Index");
        }
    }
}
