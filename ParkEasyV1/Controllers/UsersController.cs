using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using ParkEasyV1.Models;
using ParkEasyV1.Models.ViewModels;

namespace ParkEasyV1.Controllers
{
    /// <summary>
    /// Controller for handling all User events and actions (such as User Dashboard, Departures, Returns, Invoices, Bookings)
    /// </summary>
    [Authorize]
    public class UsersController : AccountController
    {
        /// <summary>
        /// Global instance of ApplicationDbContext
        /// </summary>
        private ApplicationDbContext db = new ApplicationDbContext();        

        /// <summary>
        /// default constructor
        /// </summary>
        public UsersController() : base()
        {

        }

        /// <summary>
        /// overloaded constructor
        /// </summary>
        /// <param name="userManager">UserManager</param>
        /// <param name="signInManager">SignInManager</param>
        public UsersController(ApplicationUserManager userManager, ApplicationSignInManager signInManager) : base(userManager, signInManager)
        {

        }

        /// <summary>
        /// HttpGet ActionResult to return the Users Index View
        /// </summary>
        /// <returns>Index view</returns>
        // GET: Users
        public ActionResult Index()
        {
            //get all bookings from database and store in TempData to be used on the front-end
            var bookings = db.Bookings.ToList();
            TempData["Bookings"] = bookings;

            //Store the count of total bookings and bookings today in ViewBag to be used on front-end
            ViewBag.BookingCount = bookings.Count;
            ViewBag.BookingsToday = bookings.Where(b => b.DateBooked.Day.Equals(DateTime.Today.Day)).Count();

            //get all flights from database
            var flights = db.Flights.ToList();
            //store count of flights that depart today and return today in ViewBag to be used on the front-end
            ViewBag.DepartingToday = flights.Where(f=>f.DepartureDate.Day.Equals(DateTime.Today.Day)).Count();
            ViewBag.ReturningToday = flights.Where(f => f.ReturnDate.Day.Equals(DateTime.Today.Day)).Count();

            //declare instance of usermanager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            //get the current logged in user
            User user = userManager.FindByEmail(User.Identity.GetUserName());

            //get the current user's ID and store in viewbag for front-end display
            ViewBag.UserID = user.Id;

            //check if User is Customer
            if (User.IsInRole("Customer"))
            {
                //parse User to Customer and store Customer Corportate attribute in ViewBag
                Customer customer = user as Customer;
                ViewBag.Corporate = customer.Corporate;

                //initialize invoice counter to 0
                int invoiceCounter = 0;

                //loop through all bookings for the current customer
                foreach (var booking in customer.Bookings.ToList())
                {
                    //if the booking invoice is NOT null
                    if (booking.Invoice!=null)
                    {
                        //check if the invoice status is SENT
                        if (booking.Invoice.Status==InvoiceStatus.Sent)
                        {
                            //if invoice status is sent
                            //update the invoice counter
                            invoiceCounter++;
                        }
                    }
                }

                //if the invoice counter does not equal 0
                if (invoiceCounter!=0)
                {
                    //store display message informing Customer the number of unpaid invoices they have in ViewBag to be used on front-end
                    ViewBag.Notification = "You have " + invoiceCounter + " unpaid invoices.";
                }
            }
            //return View
            return View(db.Users.ToList());
        }

        /// <summary>
        /// HttpGet ActionResult to return the Manage Users view
        /// </summary>
        /// <returns>Manage Users view</returns>
        // GET: Users/Manage
        [Authorize(Roles ="Admin, Manager, Invoice Clerk, Booking Clerk")]
        public ActionResult Manage()
        {
            //declare instance of usermanager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            //get the current user's ID and store in viewbag for front-end display
            ViewBag.UserID = userManager.FindByEmail(User.Identity.GetUserName()).Id;

            //initialize a list of customers
            List<Customer> customers = new List<Customer>();

            //loop through all users
            foreach (var user in db.Users.ToList())
            {
                //check if the user is an instance of a customer
                if (user is Customer)
                {
                    //add the customer to the list
                    customers.Add(user as Customer);
                }
            }

            //return the view with customers
            return View(customers);
        }

        /// <summary>
        /// HttpGet ActionResult to return the Departures view (all customers with flights departing today)
        /// </summary>
        /// <returns>Departures view</returns>
        // GET: Users/Departures
        [Authorize(Roles = "Admin, Manager, Invoice Clerk, Booking Clerk")]
        public ActionResult Departures()
        {
            try
            {
                //declare instance of usermanager
                UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

                //get the current user's ID and store in viewbag for front-end display
                ViewBag.UserID = userManager.FindByEmail(User.Identity.GetUserName()).Id;

                //return Departures view with bookings where departure date is today and have not checked in yet
                return View(db.Bookings.Where(b => 
                    b.Flight.DepartureDate.Day.Equals(DateTime.Today.Day)
                    &&b.CheckedIn==false)
                    .ToList());
            }
            catch (Exception ex)
            {
                //if exception occurs, throw httpnotfound error
                return HttpNotFound();
            }
            
        }

        /// <summary>
        /// HttpGet ActionResult to return the Returns view (all customers with flights returning today)
        /// </summary>
        /// <returns>Returns view</returns>
        //  GET: Users/Returns
        [Authorize(Roles = "Admin, Manager, Invoice Clerk, Booking Clerk")]
        public ActionResult Returns()
        {
            try
            {
                //declare instance of usermanager
                UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

                //get the current user's ID and store in viewbag for front-end display
                ViewBag.UserID = userManager.FindByEmail(User.Identity.GetUserName()).Id;

                //return Returns view with bookings where return date is today and have not been checked out yet
                return View(db.Bookings.Where(b => 
                    b.Flight.ReturnDate.Day.Equals(DateTime.Today.Day) 
                    && b.CheckedOut == false)
                    .ToList());
            }
            catch (Exception ex)
            {
                //if exception occurs, throw httpnotfound error
                return HttpNotFound();
            }
            
        }

        /// <summary>
        /// HttpGet ActionResult to return MyBookings view
        /// </summary>
        /// <returns>MyBookingsView</returns>
        // GET: Users/MyBookings
        public ActionResult MyBookings()
        {
            try
            {
                 //declare instance of usermanager
                UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

                //get the current logged in user
                User user = userManager.FindByEmail(User.Identity.GetUserName());

                //store current logged in user id in viewbag for front-end display
                ViewBag.UserID = user.Id;

                //parse User to Customer and store Corporate attribute in ViewBag for front-end use
                Customer customer = user as Customer;
                ViewBag.Corporate = customer.Corporate;

                //return view with bookings of the current user's
                return View(db.Bookings.Where(b=>b.UserID.Equals(user.Id)).ToList());
            }
            catch (Exception ex)
            {
                //if exception occurs, throw httpnotfound error
                return HttpNotFound();
            }
            
        }

        /// <summary>
        /// HttpGet ActionResult to return all invoices for the current User
        /// </summary>
        /// <returns></returns>
        public ActionResult MyInvoices()
        {
            try
            {
                //declare instance of usermanager
                UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

                //get the current logged in user and store id in viewbag for front-end use
                User user = userManager.FindByEmail(User.Identity.GetUserName());
                ViewBag.UserID = user.Id;

                //convert User to Customer and store corporate attribute in ViewBag for front-end use
                Customer customer = user as Customer;
                ViewBag.Corporate = customer.Corporate;
            
                //initialize a list of Bookings 
                List<Booking> invoiceBookings = new List<Booking>();

                //loop through all user bookings
                foreach (var booking in db.Bookings.Where(b=>b.UserID.Equals(user.Id)).ToList())
                {
                    //if the booking invoice is NOT null
                    if (booking.Invoice!=null)
                    {
                        //add the booking with invoice to the list
                        invoiceBookings.Add(booking);
                    }
                }

                //return myinvoices view with invoice bookings
                return View(invoiceBookings);
            }
            catch (Exception ex)
            {
                //if exception occurs, throw httpnotfound error
                return HttpNotFound();
            }
            
        }

        /// <summary>
        /// HttpGet ActionResult to return the details for a user
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>Details view</returns>
        // GET: Users/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        /// <summary>
        /// HttpGet ActionResult to return the create view
        /// </summary>
        /// <returns>Create view</returns>
        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// HttpPost ActionResult to create a new user
        /// </summary>
        /// <param name="user">created user</param>
        /// <returns>Users index</returns>
        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,FirstName,LastName,AddressLine1,AddressLine2,City,Postcode,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }

        /// <summary>
        /// HttpGet ActionResult to return the edit user view
        /// </summary>
        /// <param name="id">User id</param>
        /// <returns>Edit view</returns>
        // GET: Users/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        /// <summary>
        /// HttpPost ActionResult to update a user
        /// </summary>
        /// <param name="user">Edited user</param>
        /// <returns>User index</returns>
        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,FirstName,LastName,AddressLine1,AddressLine2,City,Postcode,Email,PhoneNumber")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        /// <summary>
        /// ActionResult to return create staff view
        /// </summary>
        /// <returns>Create staff view</returns>
        // GET: Users/Create
        [Authorize(Roles = "Admin")]
        public ActionResult CreateStaff()
        {
            //return create staff view
            return View();
        }

        /// <summary>
        /// POST ActionResult to create a staff member
        /// </summary>
        /// <param name="model">CreateStaff view model with staff details</param>
        /// <returns>Redirect to users index</returns>
        // POST: Users/Create  
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateStaff([Bind(Include = "Email, FirstName, LastName, AddressLine1, AddressLine2, City, Postcode, JobTitle, CurrentQualification, EmergencyContactName, EmergencyContactPhoneNo, Password, PasswordConfirm")] CreateStaffViewModel model)
        {
            try
            {
                //add new staff member
                if (ModelState.IsValid)
                {
                    //initialize new staff
                    Staff staff = new Staff();
                    UpdateModel(staff); //update model
                    staff.UserName = model.Email;   //set username to email

                    //create user result
                    IdentityResult result = await UserManager.CreateAsync(staff, model.Password);

                    //if succesfully created
                    if (result.Succeeded)
                    {
                        //Assign role to staff member
                        await UserManager.AddToRoleAsync(staff.Id, "Booking Clerk");

                        //success message and redirect
                        //TempData["Success"] = "Staff member successfully created";
                        return RedirectToAction("Index", "Users");
                    }
                    else
                    {
                        AddErrors(result);
                    }

                }

                return View(model);
            }
            catch (Exception ex)
            {
                //if exception occurs, return view
                return View(model);
            }
            
        }

        /// <summary>
        /// ActionResult to return edit staff view
        /// </summary>
        /// <param name="id">ID of staff being edited</param>
        /// <returns>EditStaff view</returns>
        // GET: Users/EditStaff/5
        [Authorize(Roles = "Admin")]
        public ActionResult EditStaff(string id)
        {
            //check if id is null
            if (id == null)
            {
                //return error
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //find staff by id
            Staff staff = db.Users.Find(id) as Staff;

            //if staff is null
            if (staff == null)
            {
                //return error
                return HttpNotFound();
            }

            //return view with new view model and staff details
            return View(new EditStaffViewModel
            {
                FirstName = staff.FirstName,
                LastName = staff.LastName,
                AddressLine1 = staff.AddressLine1,
                AddressLine2 = staff.AddressLine2,
                City = staff.City,
                Postcode = staff.Postcode,
                UserName = staff.UserName,
                Email = staff.Email,
                EmailConfirmed = staff.EmailConfirmed,
                JobTitle = staff.JobTitle,
                CurrentQualification = staff.CurrentQualification,
                EmergencyContactName = staff.EmergencyContactName,
                EmergencyContactPhoneNo = staff.EmergencyContactPhoneNo
            });
        }

        /// <summary>
        /// POST ActionResult to edit a staff member
        /// </summary>
        /// <param name="id">ID of staff to be edited</param>
        /// <param name="model">EditStaffViewModel with modified staff details</param>
        /// <returns>Redirect to User index</returns>
        // POST: Users/EditStaff/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditStaff(string id, [Bind(Include = "UserName, Email, FirstName, LastName, AddressLine1, AddressLine2, City, Postcode, EmailConfirmed, JobTitle, CurrentQualification, EmergencyContactName, EmergencyContactPhoneNo")] EditStaffViewModel model)
        {
            try
            {
                //if model is valid
                if (ModelState.IsValid)
                {
                    //find staff by id
                    Staff staff = (Staff)await UserManager.FindByIdAsync(id);
                    UpdateModel(staff); //update staff model

                    //get result for update staff
                    IdentityResult result = await UserManager.UpdateAsync(staff);

                    //if update is successful
                    if (result.Succeeded)
                    {
                        //success message and redirect
                        //TempData["Success"] = "Staff member successfully edited";
                        return RedirectToAction("Index", "Users");
                    }
                    AddErrors(result);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                //if exception occurs, return view
                return View(model);
            }
            
        }

        /// <summary>
        /// ActionResult for returing edit customer view
        /// </summary>
        /// <param name="id">ID of customer being edited</param>
        /// <returns>EditMember view with EditCustomerViewModel</returns>
        // GET: Users/EditMember/5
        [Authorize(Roles = "Admin")]
        public ActionResult EditCustomer(string id)
        {
            //if id is null
            if (id == null)
            {
                //error
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //find customer by id
            Customer customer = db.Users.Find(id) as Customer;

            //if customer is null
            if (customer == null)
            {
                //error
                return HttpNotFound();
            }

            //return view with new edit customer view model
            return View(new EditCustomerViewModel
            {
                Email = customer.Email,
                UserName = customer.Email,
                EmailConfirmed = customer.EmailConfirmed,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                AddressLine1 = customer.AddressLine1,
                AddressLine2 = customer.AddressLine2,
                City = customer.City,
                Postcode = customer.Postcode,
                Corporate = customer.Corporate
            });
        }

        /// <summary>
        /// POST ActionResult to edit customer details
        /// </summary>
        /// <param name="id">ID of customer being edited</param>
        /// <param name="model">EditCustomerViewModel with edited member details</param>
        /// <returns>Redirect to index</returns>
        // POST: Users/EditCustomer/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditCustomer(string id, [Bind(Include = "UserName,Email,EmailConfirmed, FirstName, LastName, AddressLine1, AddressLine2, City, Postcode, Corporate")] EditCustomerViewModel model)
        {
            try
            {
                //if model is valid
                if (ModelState.IsValid)
                {
                    //find customer by id and update model
                    Customer customer = (Customer)await UserManager.FindByIdAsync(id);
                    UpdateModel(customer);

                    //get update result
                    IdentityResult result = await UserManager.UpdateAsync(customer);

                    //if update result is successful
                    if (result.Succeeded)
                    {
                        //success message and redirect
                        //TempData["Success"] = "Member successfully edited";
                        return RedirectToAction("Index", "Users");
                    }
                    AddErrors(result);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                //if exception occurs, throw view
                return View(model);
            }
            
        }

        // GET: Users/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// ActionResult to return the create role view
        /// </summary>
        /// <returns>CreateRole view</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult CreateRole()
        {
            //return view
            return View();
        }


        /// <summary>
        /// POST ActionResult to create a new user role
        /// </summary>
        /// <param name="model">IdentityRole model with new role</param>
        /// <returns>Redirect to index</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult CreateRole([Bind(Include = "Name")] IdentityRole model)
        {
            try
            {
                //new instance of role manager
                RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));

                //if role doesnt exist
                if (!roleManager.RoleExists(model.Name))
                {
                    //create role
                    roleManager.Create(model);
                    return RedirectToAction("DisplayRoles", "Users");
                }
                //success message
                //TempData["Success"] = "Role successfully created";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                //error message
                //TempData["Error"] = "Role could not be created";
                return RedirectToAction("Index");
            }

        }


        /// <summary>
        /// ActionResult to return display all roles view
        /// </summary>
        /// <returns>DisplayRoles view</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult DisplayRoles()
        {
            //get roles and return view
            var roles = db.Roles.ToList();
            return View(roles);
        }

        /// <summary>
        /// ActionResult to delete a user role
        /// </summary>
        /// <param name="id">ID of role being deleted</param>
        /// <returns>DisplayRoles view with a list of roles</returns>
        // GET: Users/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteRole(string id)
        {
            try
            {
                //if id is null
                if (id == null)
                {
                    //error
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                //new instance of role manager
                RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
                //lookup role by id
                var role = roleManager.FindById(id);
                //if role is admin role
                if (role.Name.Equals("Admin"))
                {
                    //error
                    throw new Exception(String.Format("Cannot delete {0} Role.", role.Name));
                }

                //get amount of users in role
                var UsersInRole = roleManager.FindById(id).Users.Count();
                //if more than 0 users in role
                if (UsersInRole > 0)
                {
                    //error
                    throw new Exception(String.Format("Cannot delete {0} Role because it still has users.", role.Name));
                }

                //if role isnt null
                if (role != null)
                {
                    //delete role
                    roleManager.Delete(role);
                }
                else
                {
                    //error
                    throw new Exception(String.Format("Cannot delete {0} Role does not exist.", role.Name));
                }

                var roles = db.Roles.ToList();
                return View("DisplayRoles", roles);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error: " + ex);
                var roles = db.Roles.ToList();

                return View("DisplayRolesRoles", roles);
            }


        }


        /// <summary>
        /// ActionResult to return the change role view
        /// </summary>
        /// <param name="id">ID of user changing role</param>
        /// <returns>ChangeRole view</returns>
        // GET: Users/ChangeRole/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ChangeRole(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //declare instance of usermanager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            // Can't change your own role.
            if (id == userManager.FindByEmail(User.Identity.GetUserName()).Id)
            {
                //TempData["Error"] = "You cannot change your own role";
                return RedirectToAction("index", "users");
            }

            //find user by id
            User user = await UserManager.FindByIdAsync(id);
            //store old role
            string oldRole = (await UserManager.GetRolesAsync(id)).Single(); // Only ever a single role.

            var items = db.Roles.Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Name,
                Selected = r.Name == oldRole
            }).ToList();

            return View(new ChangeRoleViewModel
            {
                UserName = user.UserName,
                Roles = items,
                OldRole = oldRole,
            });
        }

        /// <summary>
        /// POST ActionResult to change the role of a user
        /// </summary>
        /// <param name="id">ID of user who is changing role</param>
        /// <param name="model">ChangeRoleViewModel with new role</param>
        /// <returns>Redirect to index</returns>
        // POST: Users/ChangeRole/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("ChangeRole")]
        public async Task<ActionResult> ChangeRoleConfirmation(string id,
            [Bind(Include = "Role")] ChangeRoleViewModel model)
        {
            //declare instance of usermanager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            // Can't change your own role.
            if (id == userManager.FindByEmail(User.Identity.GetUserName()).Id)
            {
                //TempData["Error"] = "You cannot change your own role";
                return RedirectToAction("index", "users");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    User user = await UserManager.FindByIdAsync(id);
                    string oldRole = (await UserManager.GetRolesAsync(id)).Single(); // Only ever a single role.

                    //if old role = new role
                    if (oldRole == model.Role)
                    {
                        //redirect
                        return RedirectToAction("Index", "Users");
                    }

                    // Remove old role and add new one.
                    await UserManager.RemoveFromRoleAsync(id, oldRole);
                    await UserManager.AddToRoleAsync(id, model.Role);


                    if (model.Role == "Manager")
                    {
                        db.Database.ExecuteSqlCommand(
                        "UPDATE AspNetUsers SET Discriminator={0} WHERE id={1}",
                        model.Role == "Admin" ? "Manager" : model.Role,
                        id);
                    }

                    // Update discriminator to change the type of this user. 
                    //This is a bit of a hack, but it works!
                        db.Database.ExecuteSqlCommand(
                        "UPDATE AspNetUsers SET Discriminator={0} WHERE id={1}",
                        model.Role == "Admin" ? "Staff" : model.Role,
                        id);

                    //success message
                    //TempData["Success"] = "Role successfully changed";
                    return RedirectToAction("Index", "Users");
                }
                catch (Exception ex)
                {
                    //TempData["Error"] = "Role could not be changed";
                    return RedirectToAction("Index", "Users");
                }

            }

            return View(model);
        }

        /// <summary>
        /// Method to release unused resources
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
