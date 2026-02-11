//using BloggingCorner.Models;
//using BloggingCorner.Models.Dto;
//using BloggingCorner.Models.ViewModels;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;

//namespace BloggingCorner.Controllers
//{
//    public class AuthController : Controller
//    {
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly SignInManager<ApplicationUser> _signInManager;
//        private readonly RoleManager<IdentityRole> _roleManager;
//        private readonly IConfiguration _config;

//        // ✅ Single constructor
//        public AuthController(
//            UserManager<ApplicationUser> userManager,
//            SignInManager<ApplicationUser> signInManager,
//            RoleManager<IdentityRole> roleManager,
//            IConfiguration config)
//        {
//            _userManager = userManager;
//            _signInManager = signInManager;
//            _roleManager = roleManager;
//            _config = config;
//        }

//        // GET: Auth Index (Login/Register Panel)
//        [HttpGet]
//        public IActionResult Index(string panel = "login")
//        {
//            var viewModel = new AuthViewModel
//            {
//                Login = new LoginDto(),
//                Register = new RegisterDto(),
//                IsRegisterActive = panel.ToLower() == "register"
//            };
//            return View(viewModel);
//        }

//        // POST: Register
//        [HttpPost]
//        [AllowAnonymous]

//        public async Task<IActionResult> Register(RegisterDto model)
//        {
//            if (!ModelState.IsValid)
//                return View(model);

//            // Determine role first
//            var roleToAssign = (!string.IsNullOrEmpty(model.AdminCode) &&
//                                model.AdminCode == _config["AdminSettings:AdminSecret"])
//                               ? "Admin"
//                               : "User";

//            var user = new ApplicationUser
//            {
//                UserName = model.Email,
//                Email = model.Email,
//                FullName = model.FullName,
//                Role = roleToAssign // <-- set Role column
//            };

//            var result = await _userManager.CreateAsync(user, model.Password);
//            if (!result.Succeeded)
//            {
//                foreach (var error in result.Errors)
//                    ModelState.AddModelError("", error.Description);

//                return View(model);
//            }

//            // Ensure roles exist
//            if (!await _roleManager.RoleExistsAsync("User"))
//                await _roleManager.CreateAsync(new IdentityRole("User"));
//            if (!await _roleManager.RoleExistsAsync("Admin"))
//                await _roleManager.CreateAsync(new IdentityRole("Admin"));

//            // Add user to Identity role
//            await _userManager.AddToRoleAsync(user, roleToAssign);

//            // Sign in
//            await _signInManager.SignInAsync(user, isPersistent: false);
//            return RedirectToAction("Index", "Post");
//        }



//        // POST: Login
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Login(LoginDto model)
//        {
//            if (!ModelState.IsValid)
//                return View(model);

//            var user = await _userManager.FindByEmailAsync(model.Email);
//            if (user != null)
//            {
//                var result = await _signInManager.PasswordSignInAsync(
//                    user, model.Password, model.RememberMe, lockoutOnFailure: false);

//                if (result.Succeeded)
//                    return RedirectToAction("Index", "Post");
//            }

//            ModelState.AddModelError("", "Invalid login attempt.");
//            return View(model);
//        }

//        // POST: Logout
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Logout()
//        {
//            await _signInManager.SignOutAsync();
//            return RedirectToAction("Index", "Home");
//        }

//        // GET: AccessDenied
//        [HttpGet]
//        public IActionResult AccessDenied()
//        {
//            return View();
//        }
//    }
//}
//using BloggingCorner.Models;
//using BloggingCorner.Models.Dto;
//using BloggingCorner.Models.ViewModels;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;

//namespace BloggingCorner.Controllers
//{
//    public class AuthController : Controller
//    {
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly SignInManager<ApplicationUser> _signInManager;
//        private readonly RoleManager<IdentityRole> _roleManager;
//        private readonly IConfiguration _config;

//        public AuthController(
//            UserManager<ApplicationUser> userManager,
//            SignInManager<ApplicationUser> signInManager,
//            RoleManager<IdentityRole> roleManager,
//            IConfiguration config)
//        {
//            _userManager = userManager;
//            _signInManager = signInManager;
//            _roleManager = roleManager;
//            _config = config;
//        }

//        [HttpGet]
//        public IActionResult Index(string panel = "login")
//        {
//            var viewModel = new AuthViewModel
//            {
//                Login = new LoginDto(),
//                Register = new RegisterDto(),
//                IsRegisterActive = panel.ToLower() == "register"
//            };
//            // Check if there are temporary login errors from a redirect
//            if (TempData.ContainsKey("LoginError"))
//            {
//                ModelState.AddModelError(string.Empty, TempData["LoginError"].ToString());
//            }
//            // Populate the Login DTO if it was passed via TempData
//            if (TempData.ContainsKey("LoginDtoEmail"))
//            {
//                viewModel.Login.Email = TempData["LoginDtoEmail"].ToString();
//            }
//            if (TempData.ContainsKey("LoginDtoRememberMe"))
//            {
//                viewModel.Login.RememberMe = (bool)TempData["LoginDtoRememberMe"];
//            }

//            return View(viewModel);
//        }

//        [HttpPost]
//        [AllowAnonymous]
//        public async Task<IActionResult> Register(RegisterDto model)
//        {
//            if (!ModelState.IsValid)
//            {
//                // If validation fails, return to the Index action with the register panel active
//                var viewModel = new AuthViewModel
//                {
//                    Login = new LoginDto(), // Keep login DTO empty or populate if needed
//                    Register = model, // Pass the invalid register model back
//                    IsRegisterActive = true
//                };
//                return View("Index", viewModel);
//            }

//            var roleToAssign = (!string.IsNullOrEmpty(model.AdminCode) &&
//                                model.AdminCode == _config["AdminSettings:AdminSecret"])
//                               ? "Admin"
//                               : "User";

//            var user = new ApplicationUser
//            {
//                UserName = model.Email,
//                Email = model.Email,
//                FullName = model.FullName,
//                Role = roleToAssign
//            };

//            var result = await _userManager.CreateAsync(user, model.Password);
//            if (!result.Succeeded)
//            {
//                foreach (var error in result.Errors)
//                    ModelState.AddModelError("", error.Description);

//                // If Identity errors, return to the Index action with the register panel active
//                var viewModel = new AuthViewModel
//                {
//                    Login = new LoginDto(),
//                    Register = model, // Pass the invalid register model back
//                    IsRegisterActive = true
//                };
//                return View("Index", viewModel);
//            }

//            if (!await _roleManager.RoleExistsAsync("User"))
//                await _roleManager.CreateAsync(new IdentityRole("User"));
//            if (!await _roleManager.RoleExistsAsync("Admin"))
//                await _roleManager.CreateAsync(new IdentityRole("Admin"));

//            await _userManager.AddToRoleAsync(user, roleToAssign);

//            await _signInManager.SignInAsync(user, isPersistent: false);
//            return RedirectToAction("Index", "Post");
//        }

//        // POST: Login
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Login(LoginDto model)
//        {
//            if (!ModelState.IsValid)
//            {
//                // If client-side validation fails (e.g., empty email/password),
//                // redirect to Index and pass the model state errors.
//                // We need to pass the model data so the form can be repopulated.
//                TempData["LoginDtoEmail"] = model.Email;
//                TempData["LoginDtoRememberMe"] = model.RememberMe;
//                // Add model errors to TempData to be retrieved by the GET Index action
//                TempData["LoginError"] = "Please fill in all required fields.";
//                return RedirectToAction("Index", new { panel = "login" });
//            }

//            var user = await _userManager.FindByEmailAsync(model.Email);
//            if (user != null)
//            {
//                var result = await _signInManager.PasswordSignInAsync(
//                    user, model.Password, model.RememberMe, lockoutOnFailure: false);

//                if (result.Succeeded)
//                {
//                    // Clear TempData for login info on successful login
//                    TempData.Remove("LoginDtoEmail");
//                    TempData.Remove("LoginDtoRememberMe");
//                    TempData.Remove("LoginError");
//                    return RedirectToAction("Index", "Post");
//                }
//            }

//            // If user is null or PasswordSignInAsync failed
//            // Set error message in TempData
//            TempData["LoginError"] = "Invalid login attempt. Please check your credentials or register.";
//            // Store the attempted login email and remember me status to repopulate the form
//            TempData["LoginDtoEmail"] = model.Email;
//            TempData["LoginDtoRememberMe"] = model.RememberMe;

//            // Redirect back to the Index action to display the login form with errors
//            return RedirectToAction("Index", new { panel = "login" });
//        }

//        // POST: Logout
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Logout()
//        {
//            await _signInManager.SignOutAsync();
//            return RedirectToAction("Index", "Home");
//        }

//        // GET: AccessDenied
//        [HttpGet]
//        public IActionResult AccessDenied()
//        {
//            return View();
//        }
//    }
//}
// before and after handling of error and data 

using BloggingCorner.Models;
using BloggingCorner.Models.Dto;
using BloggingCorner.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration; // Ensure this is included

namespace BloggingCorner.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;

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

            // TempData for specific LoginDto fields is removed to ensure fields clear on failed login.
            // TempData["LoginDtoEmail"] and TempData["LoginDtoRememberMe"] are no longer used to repopulate on error.

            return View(viewModel);
        }

        // POST: Register
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                // If validation fails, return to the Index action with the register panel active.
                // We show field-specific validation errors directly.
                var viewModel = new AuthViewModel
                {
                    Login = new LoginDto(), // Keep login DTO empty
                    Register = model, // Pass the invalid register model back to preserve input
                    IsRegisterActive = true
                };
                // We can also add a general error toast here if desired, but field errors are primary.
                // TempData["ToastMessage"] = "Please correct the registration errors.";
                // TempData["ToastType"] = "error";
                return View("Index", viewModel);
            }

            // Determine role first
            var roleToAssign = (!string.IsNullOrEmpty(model.AdminCode) &&
                                model.AdminCode == _config["AdminSettings:AdminSecret"])
                               ? "Admin"
                               : "User";

            var user = new ApplicationUser
            {
                UserName = model.Email, // Identity uses UserName for login usually, mapping email to it
                Email = model.Email,
                FullName = model.FullName,
                Role = roleToAssign
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                // If Identity errors, add them to ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description); // Empty string for general error
                }

                // Return to the Index action with the register panel active, preserving input and showing errors
                var viewModel = new AuthViewModel
                {
                    Login = new LoginDto(),
                    Register = model, // Pass the invalid register model back to preserve input
                    IsRegisterActive = true
                };
                TempData["ToastMessage"] = "Registration failed. Please correct the errors.";
                TempData["ToastType"] = "error";
                return View("Index", viewModel);
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
            TempData["ToastMessage"] = "Registration successful! Welcome.";
            TempData["ToastType"] = "success";
            return RedirectToAction("Index", "Post");
        }

        // POST: Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                // If client-side validation fails (e.g., empty email/password),
                // redirect to Index and pass a general error via Toast.
                // Fields like Email and Password will clear.
                TempData["ToastMessage"] = "Please fill in all required fields.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Index", new { panel = "login" });
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    user, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    TempData["ToastMessage"] = $"Welcome back, {user.UserName}!";
                    TempData["ToastType"] = "success";
                    return RedirectToAction("Index", "Post");
                }
            }

            // If user is null or PasswordSignInAsync failed
            // Set error message for the toast. Email and Password fields will be cleared.
            TempData["ToastMessage"] = "Invalid login attempt. Please check your credentials or register.";
            TempData["ToastType"] = "error";

            // Redirect back to the Index action to display the login form with cleared fields and toast.
            return RedirectToAction("Index", new { panel = "login" });
        }

        // POST: Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["ToastMessage"] = "You have been logged out.";
            TempData["ToastType"] = "info";
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