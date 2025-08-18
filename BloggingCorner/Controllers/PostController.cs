using BloggingCorner.Data;
using BloggingCorner.Models;
using BloggingCorner.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BloggingCorner.Controllers
{
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string[] allowedExtension = { ".jpg", ".jpeg", ".png", ".gif" };

        public PostController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }
        private async Task<string> UploadFilet(IFormFile file)
        {
            var uploads = Path.GetExtension(file.FileName).ToLower();
            var identifier = Guid.NewGuid().ToString();
            var filepath = identifier + uploads;
            var rootfolderpath = _webHostEnvironment.WebRootPath;
            var imageFolderPath = Path.Combine(rootfolderpath, "images");

            if (!Directory.Exists(imageFolderPath))
            {
                Directory.CreateDirectory(imageFolderPath);
            }

            var filepathname = Path.Combine(imageFolderPath, filepath);
            try
            {
                await using (var stream = new FileStream(filepathname, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while uploading the file: " + ex.Message);
                return null;
            }
            return "images/" + filepath;
        }
        public async Task<IActionResult> Index(int? categoryid)
        {
            var query = _db.Posts.Include(s => s.Category).AsQueryable();
            if (categoryid.HasValue && categoryid.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryid);

            }
            var posts = query.ToList();

            ViewBag.Categories = await _db.Categories.ToListAsync();
            return View(posts);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var postmodel = new PostViewModel();
            postmodel.categories = _db.Categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
            return View(postmodel);
        }
        [HttpPost]
        public async Task<IActionResult> Create(PostViewModel postmodel)
        {
            if (!ModelState.IsValid)
            {
                postmodel.categories = _db.Categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();
                return View(postmodel);
            }
            if (ModelState.IsValid)
            {
                var imageExtensionpath = Path.GetExtension(postmodel.publishImage.FileName).ToLower();
                bool isallowed = allowedExtension.Contains(imageExtensionpath);
                if (!isallowed)
                {
                    ModelState.AddModelError("publishImage", "Please upload a valid image file (jpg, jpeg, png, gif).");
                    postmodel.categories = _db.Categories.Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    }).ToList();
                    return View(postmodel);
                }

            }
            postmodel.post.Imagepath = await UploadFilet(postmodel.publishImage);
            postmodel.categories = _db.Categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
            await _db.Posts.AddAsync(postmodel.post);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Details(int id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var post = await _db.Posts.Include(p => p.Category)
                .Include(q => q.Comments)
                .Include(r => r.Likes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        [HttpPost]
        public IActionResult AddComment(Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.CommentAt = DateTime.UtcNow;
                _db.Comments.Add(comment);
                _db.SaveChanges();

                // Redirect back to the post details page
                return RedirectToAction("Details", "Post", new { id = comment.PostId });
            }

            // Show same page with errors
            return RedirectToAction("Details", "Post", new { id = comment.PostId });
        }

    }
}
