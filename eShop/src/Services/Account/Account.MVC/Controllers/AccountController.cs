using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace eShop.Services.Account.MVC.Controllers {

    public class AccountController : Controller {
        private UserManager<IdentityUser> userManager;
        private SignInManager<IdentityUser> signInManager;
        public AccountController(UserManager<IdentityUser> userMgr,
                               SignInManager<IdentityUser> signInMgr) {
            userManager = userMgr;
            signInManager = signInMgr;
        }
        [HttpGet]
        public IActionResult Login(string returnUrl) {
            ViewBag.returnUrl = returnUrl;
            return View();
        }
        [HttpPost]
        [Route("api/account/login")]
        public async Task<IActionResult> Login(LoginViewModel creds,
                string returnUrl = "") {
            if (ModelState.IsValid) {
                if (await DoLogin(creds)) {
                    return Redirect(returnUrl ?? "/");
                } else {
                    ModelState.AddModelError("", "Invalid username or password");
                }
            }
            return View(creds);
        }
        [HttpPost]
        [Route("api/account/logout")]
        public async Task<IActionResult> Logout(string redirectUrl) {
            await signInManager.SignOutAsync();
            return Redirect(redirectUrl ?? "/");
        }
        private async Task<bool> DoLogin(LoginViewModel creds) {
            IdentityUser user = await userManager.FindByNameAsync(creds.Name);
            if (user != null) {
                await signInManager.SignOutAsync();
                Microsoft.AspNetCore.Identity.SignInResult result =
                    await signInManager.PasswordSignInAsync(user, creds.Password,
                        false, false);
                return result.Succeeded;
            }
            return false;
        }
    }
    public class LoginViewModel {
        [Required]
        public string Name {get; set;}
        [Required]
        public string Password { get; set;}
    }
}