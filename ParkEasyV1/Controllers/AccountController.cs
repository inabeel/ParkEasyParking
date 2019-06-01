using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using ParkEasyV1.Models;

namespace ParkEasyV1.Controllers
{
    /// <summary>
    /// Controller for handling User Account actions
    /// </summary>
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// Default constructor
        /// </summary>
        public AccountController()
        {
        }

        /// <summary>
        /// Overloaded constructor
        /// </summary>
        /// <param name="userManager">instance of user manager</param>
        /// <param name="signInManager">instance of sign in manager</param>
        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        /// <summary>
        /// ApplicationSignInManager 
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
        /// ApplicationUserManager
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
        /// HttpGet ActionResult for returning the login view
        /// </summary>
        /// <param name="returnUrl">Previous URL the user came from</param>
        /// <returns>Login view</returns>
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        /// <summary>
        /// HttpPost ActionResult for validating the User login
        /// </summary>
        /// <param name="model">LoginViewModel with login details inputted</param>
        /// <param name="returnUrl">Previous URL of the view user came from</param>
        /// <returns>User Home View</returns>
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    if (returnUrl==null)
                    {
                        return RedirectToAction("Index", "Users");
                    }
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        /// <summary>
        /// HttpGet ActionResult for returning the verify SMS code view
        /// </summary>
        /// <param name="provider">SMS provider</param>
        /// <param name="returnUrl">URL of previous page user came from</param>
        /// <param name="rememberMe">boolean for if user wants browser to be remembered</param>
        /// <returns>View for verifying SMS code</returns>
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        /// <summary>
        /// HttpPost ActionResult for validating the User SMS code
        /// </summary>
        /// <param name="model">VerifyCodeViewModel with code inputted</param>
        /// <returns>Previous URL View</returns>
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        /// <summary>
        /// HttpGet ActionResult for returning the Register view
        /// </summary>
        /// <returns>Register view</returns>
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// HttpPost ActionResult for handling User registration
        /// </summary>
        /// <param name="model">RegisterViewModel with registration details inputted</param>
        /// <returns>User home</returns>
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            CaptchaResponse response = ValidateCaptcha(Request["g-recaptcha-response"]);

            if (response.Success && ModelState.IsValid)
            {
                //CREATE NEW CUSTOMER - ALL USERS REGISTERING WILL BE CUSTOMERS
                var user = new Customer { UserName = model.Email, Email = model.Email, RegistrationDate=DateTime.Now, Corporate = model.Corporate, FirstName = model.FirstName, LastName = model.Surname, AddressLine1 = model.AddressLine1, AddressLine2 = model.AddressLine2, City = model.City, Postcode = model.Postcode };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    //ADD USER TO THE CUSTOMER ROLE
                    await UserManager.AddToRoleAsync(user.Id, "Customer");
                    await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);

                    // UNCOMMENT TO ENABLE EMAIL ACCOUNT CONFIRMATION

                    // Send an email with this link
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    //return User home
                    return RedirectToAction("Index", "Users");
                }
                AddErrors(result);
            }
            else if(response.Success==false)
            {
                return Content("Error From Google ReCaptcha : " + response.ErrorMessage[0].ToString());
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /// <summary>  
        /// CaptchaResponse Class for verifying Google reCaptcha API response 
        /// </summary>  
        /// <param name="response">Captcha Response</param>  
        /// <returns>Deserialize Captcha Response</returns>  
        public static CaptchaResponse ValidateCaptcha(string response)
        {
            string secret = System.Web.Configuration.WebConfigurationManager.AppSettings["recaptchaPrivateKey"];
            var client = new WebClient();
            var jsonResult = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secret, response));
            return JsonConvert.DeserializeObject<CaptchaResponse>(jsonResult.ToString());
        }

        /// <summary>
        /// Async ActionResult for confirming user email
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="code">Email code sent to user</param>
        /// <returns>Email confirmed view</returns>
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        /// <summary>
        /// HttpGet ActionResult for returning forgot password view
        /// </summary>
        /// <returns>ForgotPassword View</returns>
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// HttpPost Async ActionResult for handling forgot password user output
        /// </summary>
        /// <param name="model">ForgotPasswordViewModel with inputted data</param>
        /// <returns>ForgotPasswordConfirmation View</returns>
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                 string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                 var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                 await UserManager.SendEmailAsync(user.Id, "Reset Password", 
                     "Hello, " + model.Email + "<br>We recieved a request to reset your password from ParkEasy Airport Parking. " +
                     "<br>Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>" +
                     "<br>If you did not request a new password, please let us know.");
                 return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /// <summary>
        /// HttpGet ActionResult for returning forgot password confirmation
        /// </summary>
        /// <returns>ForgotPassword Confirmation View</returns>
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        /// <summary>
        /// HttpGet ActionResult for returning reset password view
        /// </summary>
        /// <param name="code">Reset password code</param>
        /// <returns>Reset password view</returns>
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        /// <summary>
        /// HttpPost ActionResult for handling user reset password output
        /// </summary>
        /// <param name="model">ResetPasswordViewModel with inputted data</param>
        /// <returns>Reset Password Confirmation</returns>
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        /// <summary>
        /// HttpGet ActionResult for returning reset password confirmation
        /// </summary>
        /// <returns>ResetPasswordConfirmation View</returns>
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        /// <summary>
        /// HttpPost ActionResult for handling external login output and redirecting to login provider
        /// </summary>
        /// <param name="provider">External login provider</param>
        /// <param name="returnUrl">User's previous URL</param>
        /// <returns></returns>
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        /// <summary>
        /// Async ActionResult for sending SMS code used for 2FA
        /// </summary>
        /// <param name="returnUrl">User URL of previous page</param>
        /// <param name="rememberMe">Boolean for if the user wishes to be remebered on this browser</param>
        /// <returns></returns>
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        /// <summary>
        /// HttpPost Async ActionResult for handling SMS verify code result
        /// </summary>
        /// <param name="model">SendCodeViewModel with inputted data</param>
        /// <returns>VerifyCode View</returns>
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        /// <summary>
        /// Async ActionResult for retrieving external login information
        /// </summary>
        /// <param name="returnUrl">URL of previous page</param>
        /// <returns>External login confirmation</returns>
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        /// <summary>
        /// HttpPost ActionResult for processing user external login registration information upon confirmation
        /// </summary>
        /// <param name="model">ExternalLoginConfirmationViewModel with inputted data</param>
        /// <param name="returnUrl">URL of previous page</param>
        /// <returns>User Home</returns>
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Users");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new Customer { UserName = model.Email, Email = model.Email, RegistrationDate = DateTime.Now, Corporate = false, FirstName = model.FirstName, LastName = model.Surname, AddressLine1 = model.AddressLine1, AddressLine2 = model.AddressLine2, City = model.City, Postcode = model.Postcode, EmailConfirmed = true };
                //CREATE NEW CUSTOMER - ALL USERS REGISTERING WILL BE CUSTOMERS
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        //ADD USER TO THE CUSTOMER ROLE
                        await UserManager.AddToRoleAsync(user.Id, "Customer");
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToAction("Index", "Users");
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        /// <summary>
        /// HttpGet ActionResult for finding user by id and returning the manage details view
        /// </summary>
        /// <param name="Id">User ID</param>
        /// <returns>Manage View</returns>
        // GET: Account/ManageDetails
        [Authorize]
        public ActionResult ManageDetails(string Id)
        {
            //check if id is null
            if (Id == null)
            {
                //return error
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //find user by id
            User user = db.Users.Find(Id);

            //if user is null
            if (user == null)
            {
                //return error
                return HttpNotFound();
            }

            //return view with new view model and user details
            return View("Manage", (new ManageDetailsViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                AddressLine1 = user.AddressLine1,
                AddressLine2 = user.AddressLine2,
                City = user.City,
                Postcode = user.Postcode,
                Email = user.Email,
                PhoneNo = user.PhoneNumber,
                TwoFactor = user.TwoFactorEnabled
            }));
        }

        /// <summary>
        /// HttpPost Async ActionResult for processing any changes made to the user details
        /// </summary>
        /// <param name="id">User id</param>
        /// <param name="model">ManageDetailsViewModel with inputted data</param>
        /// <returns>User home</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> ManageDetails(string id, [Bind(Include = "Email, FirstName, LastName, AddressLine1, AddressLine2, City, Postcode, PhoneNo")] ManageDetailsViewModel model)
        {
            //if model is valid
            if (ModelState.IsValid)
            {
                //find staff by id
                User user = await UserManager.FindByIdAsync(id);
                UpdateModel(user); //update user model

                //get result for update staff
                IdentityResult result = await UserManager.UpdateAsync(user);

                //if update is successful
                if (result.Succeeded)
                {
                    //success message and redirect
                    TempData["Success"] = "Details successfully updated";
                    return RedirectToAction("Index", "Users");
                }
                AddErrors(result);
            }
            return View(model);
        }

        /// <summary>
        /// HttpPost ActionResult for logging the user out of the application
        /// </summary>
        /// <returns>Home page</returns>
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// HttpGet ActionResult for returning external login failure view
        /// </summary>
        /// <returns>External login failed view</returns>
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        /// <summary>
        /// Method for unloading unused resources
        /// </summary>
        /// <param name="disposing">true or false for disposing</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
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

        protected void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}