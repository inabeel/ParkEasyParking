using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ParkEasyV1.Models;

namespace ParkEasyV1.Controllers
{
    /// <summary>
    /// Controller for handling all User Account Manage actions
    /// </summary>
    [Authorize]
    public class ManageController : Controller
    {
        /// <summary>
        /// Global variable for ApplicationSignInManager
        /// </summary>
        private ApplicationSignInManager _signInManager;
        /// <summary>
        /// Global variable for ApplicationUserManager
        /// </summary>
        private ApplicationUserManager _userManager;
        /// <summary>
        /// Global variable for ApplicationDbContext
        /// </summary>
        private ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// Default constructor
        /// </summary>
        public ManageController()
        {
        }

        /// <summary>
        /// Overloaded constructor
        /// </summary>
        /// <param name="userManager">user manager object</param>
        /// <param name="signInManager">sign in manager object</param>
        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

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

        //
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

        //
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
        /// HttpGet ActionResult for returning the add phone number view
        /// </summary>
        /// <returns></returns>
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        /// <summary>
        /// HttpPost ActionResult for adding the user phone number to account
        /// </summary>
        /// <param name="model">AddPhoneNumberViewModel with inputted data</param>
        /// <returns>Verify Phone Number View</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            //check if the viewmodel state is valid
            if (!ModelState.IsValid)
            {
                //if state is not valid return the add phone number view with the model
                return View(model);
            }

            //create a variable to hold the user id and initialize is as null
            string userId = null;

            //loop through all users in the database
            foreach (var user in db.Users.ToList())
            {
                //if the user email matches the username of the current logged in user
                if (user.Email.Equals(User.Identity.GetUserName()))
                {
                    //store the user id of the current user
                    userId = user.Id;
                }
            }

            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(userId, model.Number);
            //check if the sms service is not null
            if (UserManager.SmsService != null)
            {
                //create and send new sms message with security code
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            //return verify phone number action with the user phone number
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        /// <summary>
        /// HttpPost ActionResult for enabling two factor authentication on the user's account
        /// </summary>
        /// <returns></returns>
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            string userId = null;

            foreach (var u in db.Users.ToList())
            {
                if (u.Email.Equals(User.Identity.GetUserName()))
                {
                    userId = u.Id;
                }
            }

            await UserManager.SetTwoFactorEnabledAsync(userId, true);
            var user = await UserManager.FindByIdAsync(userId);
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Manage", "Account");
        }

        /// <summary>
        /// HttpPost ActionResult for disabling two factor authentication on the user's account
        /// </summary>
        /// <returns></returns>
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            string userId = null;

            foreach (var u in db.Users.ToList())
            {
                if (u.Email.Equals(User.Identity.GetUserName()))
                {
                    userId = u.Id;
                }
            }

            await UserManager.SetTwoFactorEnabledAsync(userId, false);
            var user = await UserManager.FindByIdAsync(userId);
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Manage", "Account");
        }

        /// <summary>
        /// HttpGet ActionResult for returning the verify phone number view
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            string userId = null;

            foreach (var user in db.Users.ToList())
            {
                if (user.Email.Equals(User.Identity.GetUserName()))
                {
                    userId = user.Id;
                }
            }

            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(userId, phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        /// <summary>
        /// HttpPost ActionResult for validating the sms security code and verifying the user's phone number
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            string userId = null;

            foreach (var user in db.Users.ToList())
            {
                if (user.Email.Equals(User.Identity.GetUserName()))
                {
                    userId = user.Id;
                }
            }

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
                return RedirectToAction("MyDetails", "Users", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        /// <summary>
        /// HttpPost ActionResult for removing the phone number from the user's account
        /// </summary>
        /// <returns></returns>
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            string userId = null;

            foreach (var u in db.Users.ToList())
            {
                if (u.Email.Equals(User.Identity.GetUserName()))
                {
                    userId = u.Id;
                }
            }

            var result = await UserManager.SetPhoneNumberAsync(userId, null);
            if (!result.Succeeded)
            {
                return RedirectToAction("MyDetails", "Users", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(userId);
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("MyDetails", "Users", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        /// <summary>
        /// HttpGet ActionResult for returning the view to change the user's password
        /// </summary>
        /// <returns></returns>
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        /// HttpPost ActionResult for changing the user's password
        /// </summary>
        /// <param name="model">ChangePasswordViewModel with current password and new password data</param>
        /// <returns>Users home page</returns>
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            //check model state is valid
            if (!ModelState.IsValid)
            {
                //if model state is not valid return the view with the model
                return View(model);
            }

            //create a variable to hold the user's id and initialize as null
            string userId = null;

            //loop through all users in the database
            foreach (var user in db.Users.ToList())
            {
                //if the user's email matches the username of the current logged in user
                if (user.Email.Equals(User.Identity.GetUserName()))
                {
                    //store the user's id
                    userId = user.Id;
                }
            }
            //change password and store success result
            var result = await UserManager.ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);
            //if password is successfully changed
            if (result.Succeeded)
            {
                //get the current user
                var user = await UserManager.FindByIdAsync(userId);
                if (user != null)
                {
                    //if the user is not null sign the user in
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                //update success message and return users home
                TempData["Success"] = "Password Successfully Changed";
                return RedirectToAction("Index", "Users", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            //if error occurs add errors and return view with model
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
        /// HttpPost ActionResult for setting the users password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                string userId = null;

                foreach (var user in db.Users.ToList())
                {
                    if (user.Email.Equals(User.Identity.GetUserName()))
                    {
                        userId = user.Id;
                    }
                }

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

        //
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

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
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