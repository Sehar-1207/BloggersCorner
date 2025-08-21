using BloggingCorner.Models;
using BloggingCorner.Models.Dto;
using BloggingCorner.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BloggingCorner.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;

        // ✅ Single constructor
        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _config = config;
        }

        // GET: Auth Index (Login/Register Panel)
        [HttpGet]
        public IActionResult Index(string panel = "login")
        {
            var viewModel = new AuthViewModel
            {
                Login = new LoginDto(),
                Register = new RegisterDto(),
                IsRegisterActive = panel.ToLower() == "register"
            };
            return View(viewModel);
        }

        // POST: Register
        [HttpPost]
        [AllowAnonymous]

        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Determine role first
            var roleToAssign = (!string.IsNullOrEmpty(model.AdminCode) &&
                                model.AdminCode == _config["AdminSettings:AdminSecret"])
                               ? "Admin"
                               : "User";

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Role = roleToAssign // <-- set Role column
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            // Ensure roles exist
            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new IdentityRole("User"));
            if (!await _roleManager.RoleExistsAsync("Admin"))
                await _roleManager.CreateAsync(new IdentityRole("Admin"));

            // Add user to Identity role
            await _userManager.AddToRoleAsync(user, roleToAssign);

            // Sign in
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Post");
        }



        // POST: Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    user, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                    return RedirectToAction("Index", "Post");
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        // POST: Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
