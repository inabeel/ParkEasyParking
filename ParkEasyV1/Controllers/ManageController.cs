using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ParkEasyV1.Models;

namespace ParkEasyV1.Controllers
{
    /// <summary>
    /// Controller to handle all Manage actions or events
    /// </summary>
    [Authorize]
    public class ManageController : Controller
    {
        /// <summary>
        /// instance of sign in manager
        /// </summary>
        private ApplicationSignInManager _signInManager;

        /// <summary>
        /// instance of user manager
        /// </summary>
        private ApplicationUserManager _userManager;

        /// <summary>
        /// instance of applicationdbcontext
        /// </summary>
        private ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// default constructor
        /// </summary>
        public ManageController()
        {
        }

        /// <summary>
        /// overloaded constructor
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="signInManager"></param>
        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        /// <summary>
        /// sign in manager class
        /// </summary>
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        /// <summary>
        /// user manager class
        /// </summary>
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        /// <summary>
        /// HttpGet Async ActionResult to return the Manage Index
        /// </summary>
        /// <param name="message">Nullable Manage Message</param>
        /// <returns>Manage Index view</returns>
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            return View(model);
        }

        /// <summary>
        /// HttpPost Async ActionResult to remove a login from account
        /// </summary>
        /// <param name="loginProvider">Login provider</param>
        /// <param name="providerKey">Login provider key</param>
        /// <returns>ManageLogins view</returns>
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        /// <summary>
        /// HttpGet ActionResult to return add phone number view
        /// </summary>
        /// <returns>AddPhoneNumber view</returns>
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        /// <summary>
        /// HttpPost Async ActionResult to add a phone number to user account and send verification code
        /// </summary>
        /// <param name="model">AddPhoneNumberViewModel with phone number data</param>
        /// <returns>VerifyPhoneNumber view</returns>
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //declare instance of usermanager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            //get the user id of the current logged in user
            string userId = UserManager.FindByEmail(User.Identity.GetUserName()).Id;

            // Generate the token and send it using Twilio API SMS Service
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(userId, model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        /// <summary>
        /// HttpPost ActionResult to enable Two Factor Authentication on user account
        /// </summary>
        /// <returns>Account ManageDetails View</returns>
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            //declare instance of usermanager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            //get user id of current logged in user
            string userId = UserManager.FindByEmail(User.Identity.GetUserName()).Id;

            await UserManager.SetTwoFactorEnabledAsync(userId, true);
            var user = await UserManager.FindByIdAsync(userId);
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("ManageDetails", "Account", new { Id=userId});
        }

        /// <summary>
        /// HttpPost ActionResult to disable Two Factor Authentication from user account
        /// </summary>
        /// <returns>ManageDetails View</returns>
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            //declare instance of usermanager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            //get user id of current logged in user
            string userId = UserManager.FindByEmail(User.Identity.GetUserName()).Id;

            await UserManager.SetTwoFactorEnabledAsync(userId, false);
            var user = await UserManager.FindByIdAsync(userId);
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("ManageDetails", "Account", new { Id = userId });
        }

        /// <summary>
        /// HttpGet ActionResult to return the verify phone number view
        /// </summary>
        /// <param name="phoneNumber">phone number to be verified</param>
        /// <returns>VerifyPhoneNumber View</returns>
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            //declare instance of usermanager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            //get the user id of the current logged in user
            string userId = UserManager.FindByEmail(User.Identity.GetUserName()).Id;

            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(userId, phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        /// <summary>
        /// HttpPost Async ActionResult to verify a phone number for a user account
        /// </summary>
        /// <param name="model">VerifyPhoneNumberViewModel with SMS code data</param>
        /// <returns>ManageDetails view</returns>
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            //declare instance of usermanager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            //get the user id of the current logged in user
            string userId = UserManager.FindByEmail(User.Identity.GetUserName()).Id;

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(userId, model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(userId);
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("ManageDetails", "Account", new { Id = user.Id});
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        /// <summary>
        /// HttpPost Async ActionResult to remove a phone number from user account
        /// </summary>
        /// <returns>ManageDetails view</returns>
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            //declare instance of usermanager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            //get the user id of the current logged in user
            string userId = UserManager.FindByEmail(User.Identity.GetUserName()).Id;

            var result = await UserManager.SetPhoneNumberAsync(userId, null);
            if (!result.Succeeded)
            {
                return RedirectToAction("ManageDetails", "Account", new { Id=userId });
            }
            var user = await UserManager.FindByIdAsync(userId);
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("ManageDetails", "Account", new { Id=userId });
        }

        /// <summary>
        /// HttpGet ActionResult to return the ChangePassword view
        /// </summary>
        /// <returns>ChangePassword View</returns>
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        /// HttpPost Async ActionResult to change the password of a user account 
        /// </summary>
        /// <param name="model">ChangePasswordViewModel with new password information</param>
        /// <returns>User Index</returns>
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //declare instance of usermanager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            //get the user id of the current logged in user
            string userId = UserManager.FindByEmail(User.Identity.GetUserName()).Id;

            var result = await UserManager.ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(userId);
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                TempData["Success"] = "Password Successfully Changed";
                return RedirectToAction("Index", "Users", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        /// <summary>
        /// HttpGet ActionResult to return the set password view
        /// </summary>
        /// <returns></returns>
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        /// <summary>
        /// HttpPost Async ActionResult to set the password on a user account
        /// </summary>
        /// <param name="model">SetPasswordViewModel with password information</param>
        /// <returns>Manage Index</returns>
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                //declare instance of usermanager
                UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

                //get the user id of the current logged in user
                string userId = UserManager.FindByEmail(User.Identity.GetUserName()).Id;

                var result = await UserManager.AddPasswordAsync(userId, model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /// <summary>
        /// HttpGet Async ActionResult to return the ManageLogins view
        /// </summary>
        /// <param name="message">Nullable Manage Message</param>
        /// <returns>ManageLogins view</returns>
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        /// <summary>
        /// HttpPost ActionResult to link a login to user account
        /// </summary>
        /// <param name="provider">login provider</param>
        /// <returns>External login provider link login</returns>
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        /// <summary>
        /// HttpGet Async ActionResult for link login callback
        /// </summary>
        /// <returns>ManageLogins view</returns>
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        /// <summary>
        /// Method to release unused resources
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

#region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

#endregion
    }
}