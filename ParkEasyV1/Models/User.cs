using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace ParkEasyV1.Models
{
    /// <summary>
    /// Abstract Class that inherits from IdentityUser and models a User
    /// </summary>
    public abstract class User : IdentityUser
    {
        /// <summary>
        /// ApplicationUserManager for managing users
        /// </summary>
        private ApplicationUserManager userManager;

        /// <summary>
        /// User first name
        /// </summary>
        [Display(Name ="First Name")]
        public string FirstName { get; set; }

        /// <summary>
        /// User last name
        /// </summary>
        [Display(Name ="Last Name")]
        public string LastName { get; set; }

        /// <summary>
        /// user address line 1
        /// </summary>
        [Display(Name ="Address Line 1")]
        public string AddressLine1 { get; set; }

        /// <summary>
        /// user address line 2
        /// </summary>
        public string AddressLine2 { get; set; }

        /// <summary>
        /// user city
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// user postcode
        /// </summary>
        public string Postcode { get; set; }

        //Navigational Properties

        /// <summary>
        /// Virtual list of Payments to model One to Many relationship with Payment
        /// </summary>
        public virtual List<Payment> Payments { get; set; }

        /// <summary>
        /// Virtual list of Bookings to model One to Many relationship with Booking
        /// </summary>
        public virtual List<Booking> Bookings { get; set; }


        /// <summary>
        /// Current role user belongs to - not mapped in DB
        /// </summary>
        [NotMapped]
        public string CurrentRole
        {
            get
            {
                if (userManager == null)
                {
                    userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();

                }

                return userManager.GetRoles(Id).Single();
            }
        }

        /// <summary>
        /// Async Function to generate user identity
        /// </summary>
        /// <param name="manager"></param>
        /// <returns></returns>
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
}