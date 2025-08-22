using BloggingCorner.Data;
using BloggingCorner.Models;
using BloggingCorner.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BloggingCorner.Controllers
{
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string[] allowedExtension = { ".jpg", ".jpeg", ".png", ".gif" };

        public PostController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
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
            var query = _db.Posts
                           .Include(s => s.Category)
                           .Include(s => s.Likes) // <-- Include Likes here
                           .AsQueryable();

            if (categoryid.HasValue && categoryid.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryid);
            }

            var posts = await query.ToListAsync();
            ViewBag.Categories = await _db.Categories.ToListAsync();

            // ✅ If logged in, fetch full name
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    ViewBag.FullName = $"{user.FullName}";
                }
            }

            return View(posts);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]

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
        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "User , Admin")]
        public async Task<ActionResult> Details(int id)
        {
            if (id <= 0)
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

        [HttpGet]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Edit(int id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var post = await _db.Posts.FirstOrDefaultAsync(p=>p.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            EditPostViewModel edit = new EditPostViewModel
            {
                post = post,
                categories = _db.Categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList()
            };
            return View(edit);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(EditPostViewModel editPostViewModel)
        {
            if (!ModelState.IsValid)
            {
                editPostViewModel.categories = _db.Categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();
                return View(editPostViewModel);
            }

            var postfromDb = await _db.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == editPostViewModel.post.Id);
            if (postfromDb == null) {
                return NotFound();
            }

            if (editPostViewModel.publishImage != null)
            {
                var imageExtensionpath = Path.GetExtension(editPostViewModel.publishImage.FileName).ToLower();
                bool isallowed = allowedExtension.Contains(imageExtensionpath);
                if (!isallowed)
                {
                    ModelState.AddModelError("publishImage", "Please upload a valid image file (jpg, jpeg, png, gif).");
                    editPostViewModel.categories = _db.Categories.Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    }).ToList();
                    return View(editPostViewModel);
                }
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath,"Images", postfromDb.Imagepath);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
                editPostViewModel.post.Imagepath = await UploadFilet(editPostViewModel.publishImage);
            }
            else
            {
                 editPostViewModel.post.Imagepath = postfromDb.Imagepath; // Keep the old image path if no new image is uploaded
            }
            _db.Posts.Update(editPostViewModel.post);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(post.Imagepath))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", post.Imagepath);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _db.Posts.Remove(post);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "User , Admin")]

        public JsonResult AddComment([FromBody] Comment comment)
        {

            if (ModelState.IsValid)
            {
                comment.CommentAt = DateTime.UtcNow;
                _db.Comments.Add(comment);
                _db.SaveChanges();

                return Json(new
                {
                    username = comment.username,
                    commentAt = comment.CommentAt.ToString("MMM dd, yyyy"),
                    commentContent = comment.Commentcontent
                });
            }

            // Return validation errors for debugging
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = "Failed to add comment.", errors });
        }

        // Anyone can get the current like count
        [HttpGet]
        public async Task<IActionResult> GetLikeCount(int postId)
        {
            var post = await _db.Posts.Include(p => p.Likes)
                       .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
                return NotFound();

            return Json(new
            {
                likeCount = post.Likes.Count
            });
        }

        
        [HttpPost]
        [Authorize(Roles = "User , Admin")]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var userId = _userManager.GetUserId(User);
            var username = User.Identity.Name ?? "Unknown";

            var post = await _db.Posts.Include(p => p.Likes)
                       .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
                return NotFound(new { success = false });

            var existingLike = post.Likes.FirstOrDefault(l => l.UserId == userId);
            bool isNowLiked;

            if (existingLike != null)
            {
                _db.Likes.Remove(existingLike);
                isNowLiked = false;
            }
            else
            {
                post.Likes.Add(new Like
                {
                    UserId = userId,
                    Username = username,
                    PostId = post.Id
                });
                isNowLiked = true;
            }

            await _db.SaveChangesAsync();

            return Json(new
            {
                success = true,
                liked = isNowLiked,
                likeCount = post.Likes.Count
            });
        }

    }
}
