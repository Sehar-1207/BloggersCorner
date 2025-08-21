using BloggingCorner.Models;
using BloggingCorner.Models.Dto;
using BloggingCorner.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace YourProject.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager, IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _config = config;
        }

        // ===================== REGISTER & LOGIN =====================

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

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = new AuthViewModel
                {
                    Login = new LoginDto(),
                    Register = model,
                    IsRegisterActive = true
                };
                return View("Index", viewModel);
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                FullName = model.FullName,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign roles
                await AssignRoles(user, model.AdminCode);

                // Sign in and set cookies
                await _signInManager.SignInAsync(user, false);
                await SetAuthCookies(user);

                return RedirectToAction("Index", "Post");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            var errorViewModel = new AuthViewModel { Login = new LoginDto(), Register = model, IsRegisterActive = true };
            return View("Index", errorViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = new AuthViewModel
                {
                    Login = model,
                    Register = new RegisterDto(),
                    IsRegisterActive = false
                };
                return View("Index", viewModel);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("Email", "Email not found.");
                var viewModel = new AuthViewModel { Login = model, Register = new RegisterDto(), IsRegisterActive = false };
                return View("Index", viewModel);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("Password", "Incorrect password.");
                var viewModel = new AuthViewModel { Login = model, Register = new RegisterDto(), IsRegisterActive = false };
                return View("Index", viewModel);
            }

            await SetAuthCookies(user);
            return RedirectToAction("Index", "Post");
        }


        // ===================== LOGOUT =====================

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiresAt = null;
                await _userManager.UpdateAsync(user);
            }

            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");

            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ===================== TOKEN HANDLING =====================

        private async Task SetAuthCookies(ApplicationUser user)
        {
            var accessToken = await GenerateJwtToken(user);

            if (string.IsNullOrEmpty(user.RefreshToken) || user.RefreshTokenExpiresAt < DateTime.UtcNow)
            {
                user.RefreshToken = Guid.NewGuid().ToString();
                user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
                await _userManager.UpdateAsync(user);
            }

            Response.Cookies.Append("access_token", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(15)
            });

            Response.Cookies.Append("refresh_token", user.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = user.RefreshTokenExpiresAt
            });
        }

        [HttpPost]
        public async Task<IActionResult> AutoRefreshToken()
        {
            var accessToken = Request.Cookies["access_token"];
            var refreshToken = Request.Cookies["refresh_token"];

            if (IsTokenValid(accessToken))
                return Ok(new { accessToken });

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized("No refresh token provided");

            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshToken == refreshToken);
            if (user == null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
                return Unauthorized("Invalid or expired refresh token");

            var newAccessToken = await GenerateJwtToken(user);
            var newRefreshToken = Guid.NewGuid().ToString();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            Response.Cookies.Append("access_token", newAccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(15)
            });

            Response.Cookies.Append("refresh_token", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new { accessToken = newAccessToken });
        }

        private bool IsTokenValid(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            var jwtSettings = _config.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["DurationInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task AssignRoles(ApplicationUser user, string adminCode)
        {
            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new IdentityRole("User"));

            if (!await _roleManager.RoleExistsAsync("Admin"))
                await _roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!string.IsNullOrEmpty(adminCode) && adminCode == _config["AdminSettings:Secret"])
                await _userManager.AddToRoleAsync(user, "Admin");
            else
                await _userManager.AddToRoleAsync(user, "User");
        }
    }
}
