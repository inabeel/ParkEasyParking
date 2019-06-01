using System;
using System.Collections.Generic;
using System.Configuration;
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
using PayPal.Api;
using Stripe;

namespace ParkEasyV1.Controllers
{
    /// <summary>
    /// Controller to handle all Payments events and actions
    /// </summary>
    public class PaymentsController : Controller
    {
        /// <summary>
        /// Global instance of ApplicationDbContext
        /// </summary>
        private ApplicationDbContext db = new ApplicationDbContext();        

        /// <summary>
        /// HttpGet ActionResult to return Index view
        /// </summary>
        /// <returns>Index view</returns>
        // GET: Payments
        public ActionResult Index()
        {
            var payments = db.Payments.Include(p => p.User);
            return View(payments.ToList());
        }

        /// <summary>
        /// HttpGet ActionResult to return the Payments Charge view - to allow Customers to enter their payment details and complete booking
        /// </summary>
        /// <param name="id">Nullable Booking ID - used to pay for existing bookings (via invoice)</param>
        /// <returns>Charge View</returns>
        // GET: Payments/Charge
        [Authorize]
        public ActionResult Charge(int? id)
        {
            //create instance of user manager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            //store Stripe Payment Gateway API Publishable Key in ViewBag to be used with front-end JavaScript
            ViewBag.StripePublishableKey = ConfigurationManager.AppSettings["StripePublishableKey"];

            //create instance of a booking and default it to null
            Booking booking=null;

            //check if the payment is linked to a specific booking id parameter
            //new bookings will not have an id parameter - but payments made from an invoice for an existing booking will
            if (id==null)
            {
                //find booking from tempdata
                booking = db.Bookings.Find(TempData["bookingID"]);
            }
            else
            {
                //find booking from id parameter
                booking = db.Bookings.Find(id);
            }

            //if booking has an invoice - then the payment is being paid from a previous invoice
            if (booking.Invoice!=null)
            {
                //update viewbag invoice attribute and store the invoice in tempdata
                ViewBag.Invoice = true;
                TempData["Invoice"] = booking.Invoice;
            }

            //if the user is a customer
            if (User.IsInRole("Customer"))
            {
                //get the current User and parse to Customer
                Models.Customer customer = userManager.FindByEmail(User.Identity.GetUserName()) as Models.Customer;
                //store in ViewBag if the customer is a corporate customer or not to be used on the front-end
                ViewBag.Corporate = customer.Corporate;
            }            

            //store the booking total (rounded to 2 decimal places) in ViewBag for front-end use
            ViewBag.Total = Math.Round(booking.Total,2);
            
            //Stripe Payments are in stored in cent/pence format
            //store the stripe payment total (booking total * 100) in ViewBag for front-end use
            ViewBag.StripeTotal = (int)Math.Ceiling(booking.Total*100);

            //re-store the booking ID in new TempData - this avoids it being cleared by the session during payment
            TempData["bID"] = booking.ID;
            //return view
            return View();
        }

        /// <summary>
        /// HttpPost ActionResult for processing a Stripe API card payment
        /// </summary>
        /// <param name="stripeEmail">Email associated with Stripe</param>
        /// <param name="stripeToken">Token associated with Stripe</param>
        /// <returns>Booking Confirmation View</returns>
        // POST: Payments/Charge
        [HttpPost]
        [Authorize]
        public ActionResult Charge(string stripeEmail, string stripeToken)
        {
            try
            {
                //find the booking via Booking ID stored in TempData
                Booking booking = db.Bookings.Find(TempData["bID"]);
            
                //create instances of Stripe customers and charges objects
                var customers = new CustomerService();
                var charges = new ChargeService();

                //create stripe customer
                var customer = customers.Create(new CustomerCreateOptions
                {
                    Email = stripeEmail,
                    SourceToken = stripeToken,
                });

                //create stripe charge
                var charge = charges.Create(new ChargeCreateOptions
                {
                    Amount = (int)Math.Ceiling(booking.Total * 100),
                    Description = "ParkEasy Airport Parking Charge",
                    Currency = "gbp",
                    CustomerId = customer.Id,
                    ReceiptEmail = customer.Email,
                });

                //update booking status to confirmed
                booking.BookingStatus = BookingStatus.Confirmed;            

                //store the payment in the databse
                db.Payments.Add(new ExternalPayment()
                {
                    PaymentDate = DateTime.Now,
                    Amount = charge.Amount,
                    User = booking.User,
                    TransactionID = charge.ReceiptNumber
                });
                db.SaveChanges();

                //check if Invoice TempData is NOT null
                //if this is NOT null then the payment is being made for an existing booking from an Invoice
                if (TempData["Invoice"]!=null)
                {
                    //update the invoice status on the booking to paid
                    booking.Invoice.Status = InvoiceStatus.Paid;
                    db.SaveChanges();   //save database changes
                    //return to Invoice confirmation
                    return RedirectToAction("Confirmation", "Invoice", new { id=booking.ID});
                }
            
                //email booking confirmation
                booking.EmailConfirmation();

                //send sms confirmation
                //SMS CONFIRMATION CURRENTLY DISABLED - SEE ERROR LOG DOCUMENTATION FOR MORE DETAILS
                //booking.SMSConfirmation();

                //redirect to booking confirmation
                return RedirectToAction("Confirmation", "Bookings", new { id=booking.ID});
            }
            catch (Exception ex)
            {
                //if exception occurs, redisplay form with error message
                TempData["Error"] = "Error: Unable to process payment. Please contact us.";
                return View();
            }            
        }

        /// <summary>
        /// ActionResult to process the event a Staff member is creating a walk-in booking and has recieved cash from the Customer
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin, Manager, Invoice Clerk, Booking Clerk")]
        public ActionResult CashPayment()
        {
            //find the booking via booking id stored in temp data
            Booking booking = db.Bookings.Find(TempData["bID"]);

            //set the booking status to confirmed
            booking.BookingStatus = BookingStatus.Confirmed;

            //as the payment is for a walk-in booking
            //set the parking slot status to occupied as the booking will commence immediately
            booking.ParkingSlot.Status = Status.Occupied;

            //store record of the payment in database
            db.Payments.Add(new Cash()
            {
                PaymentDate = DateTime.Now,
                Amount = booking.Total,
                User = booking.User
            });
            //save database changes
            db.SaveChanges();

            //return booking confirmation
            return RedirectToAction("Confirmation", "Bookings", new { id = booking.ID });
        }

        /// <summary>
        /// HttpGet ActionResult to return the payment details view
        /// </summary>
        /// <param name="id">Payment id</param>
        /// <returns>Details view</returns>
        // GET: Payments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Models.Payment payment = db.Payments.Find(id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            return View(payment);
        }
        
        /// <summary>
        /// HttpPost ActionResult to create a new payment
        /// </summary>
        /// <param name="payment">Created payment</param>
        /// <returns>Payments index</returns>
        // POST: Payments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID, PaymentDate, Amount")] Models.Payment payment)  //remember date, amount, user
        {
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            if (ModelState.IsValid)
            {
                db.Payments.Add(payment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserID = new SelectList(db.Users, "Id", "FirstName", payment.UserID);
            return View(payment);
        }                     

        /// <summary>
        /// HttpGet ActionResult to return the Edit payment view
        /// </summary>
        /// <param name="id">Payment id</param>
        /// <returns>Edit view</returns>
        // GET: Payments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Models.Payment payment = db.Payments.Find(id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserID = new SelectList(db.Users, "Id", "FirstName", payment.UserID);
            return View(payment);
        }

        /// <summary>
        /// HttpPost ActionResult to update a payment
        /// </summary>
        /// <param name="payment">Updated payment</param>
        /// <returns>Payments index</returns>
        // POST: Payments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,PaymentDate,Amount,UserID")] Models.Payment payment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(payment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserID = new SelectList(db.Users, "Id", "FirstName", payment.UserID);
            return View(payment);
        }

        /// <summary>
        /// HttpGet ActionResult to return the delete payment view
        /// </summary>
        /// <param name="id">Payment id</param>
        /// <returns>Delete view</returns>
        // GET: Payments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Models.Payment payment = db.Payments.Find(id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            return View(payment);
        }

        /// <summary>
        /// HttpPost ActionResult to delete a payment
        /// </summary>
        /// <param name="id">Payment id</param>
        /// <returns>Index view</returns>
        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Models.Payment payment = db.Payments.Find(id);
            db.Payments.Remove(payment);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// ActionResult for making a payment using PayPal API
        /// </summary>
        /// <param name="Cancel">Cancel status</param>
        /// <returns>Confirmation view</returns>
        [Authorize]
        public ActionResult PaymentWithPaypal(string Cancel = null)
        {
            //get the booking using booking id stored in TempData
            Booking booking = db.Bookings.Find(TempData["bID"]);

            //getting the apiContext  
            APIContext apiContext = PayPalConfiguration.GetAPIContext();
            try
            {
                //A resource representing a Payer that funds a payment Payment Method as paypal  
                //Payer Id will be returned when payment proceeds or click to pay  
                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {
                    //this section will be executed first because PayerID doesn't exist  
                    //it is returned by the create function call of the payment class  
                    // Creating a payment  
                    // baseURL is the url on which paypal sendsback the data.  
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Payments/PaymentWithPayPal?";
                    //here we are generating guid for storing the paymentID received in session  
                    //which will be used in the payment execution  
                    var guid = Convert.ToString((new Random()).Next(100000));
                    //CreatePayment function gives us the payment approval url  
                    //on which payer is redirected for paypal account payment  
                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid);
                    //get links returned from paypal in response to Create function call  
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;
                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            //saving the payapalredirect URL to which user will be redirected for payment  
                            paypalRedirectUrl = lnk.href;
                        }
                    }
                    // saving the paymentID in the key guid  
                    Session.Add(guid, createdPayment.id);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // This function exectues after receving all parameters for the payment  
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    //If executed payment failed then we will show payment failure message to user  
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        TempData["Error"] = "We were unable to process your payment. Please try again.";
                        ViewBag.Total = booking.Total;
                        return View("Charge");
                    }
                }
            }
            //if exception occurs
            catch (Exception ex)
            {
                //return booking charge View with error message
                TempData["Error"] = "We were unable to process your payment. Please try again.";
                ViewBag.Total = booking.Total;
                return View("Charge");
            }

            //if TempData Invoice is NOT null
            //then this payment is for an existing booking (payment for invoice)
            if (TempData["Invoice"]!=null)
            {
                //update booking invoice status to paid and save changes
                booking.Invoice.Status = InvoiceStatus.Paid;
                db.SaveChanges();
                //return invoice confirmation
                return RedirectToAction("Confirmation", "Invoice", new { id = booking.ID });
            }

            //update booking status and save changes
            booking.BookingStatus = BookingStatus.Confirmed;
            db.SaveChanges();
            //email booking confirmation
            booking.EmailConfirmation();
            //on successful payment, show success page to user.  
            return RedirectToAction("Confirmation", "Bookings", new { id=booking.ID});
        }
        private PayPal.Api.Payment payment;
        private PayPal.Api.Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new PayPal.Api.Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }
        private PayPal.Api.Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {
            Booking booking = db.Bookings.Find(TempData["bID"]);

            //create itemlist and add item objects to it  
            var itemList = new ItemList()
            {
                items = new List<Item>()
            };
            //Adding Item Details like name, currency, price etc  
            itemList.items.Add(new Item()
            {
                name = "ParkEasy Airport Parking Booking",
                currency = "GBP",
                price = booking.Total.ToString(),
                quantity = "1",
                sku = booking.ParkingSlot.ID.ToString()
            });
            var payer = new Payer()
            {
                payment_method = "paypal"
            };
            // Configure Redirect Urls here with RedirectUrls object  
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };
            // Adding Tax, shipping and Subtotal details  
            var details = new Details()
            {
                tax = "0",
                shipping = "0",
                subtotal = booking.Total.ToString(),
            };
            //Final amount with details  
            var amount = new Amount()
            {
                currency = "GBP",
                total = booking.Total.ToString(), // Total must be equal to sum of tax, shipping and subtotal.  
                details = details
            };
            var transactionList = new List<Transaction>();
            // Adding description about the transaction  
            transactionList.Add(new Transaction()
            {
                description = "ParkEasy Airport Parking Booking",
                //invoice_number = "your generated invoice number", //Generate an Invoice No  
                amount = amount,
                item_list = itemList
            });
            this.payment = new PayPal.Api.Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };

            SavePayment(booking.Total, booking.User, payment.id);

            // Create a payment using a APIContext  
            return this.payment.Create(apiContext);
        }

        /// <summary>
        /// ActionResult to Confirm a booking for corporate customers who will be invoiced for payment
        /// </summary>
        /// <returns>Confirmation View</returns>
        [Authorize]
        public ActionResult InvoiceCharge()
        {
            try
            {
                //find the booking via id stored in TempData
                Booking booking = db.Bookings.Find(TempData["bID"]);

                //as this booking is for a corporate customer
                //confirm booking without payment instantly (they will be invoiced at later date)
                //update booking status and save changes
                booking.BookingStatus = BookingStatus.Confirmed;
                db.SaveChanges();

                //return booking confirmation
                return RedirectToAction("Confirmation", "Bookings", new { id = booking.ID });
            }
            catch (Exception ex)
            {
                //if exception occurs, return User Index with error message
                TempData["Error"] = "Error: Unable to confirm corporate booking. Please contact us";
                return RedirectToAction("Index", "Users");
            }
            
        }

        /// <summary>
        /// Method to save payment to database
        /// </summary>
        /// <param name="amount">Payment amount</param>
        /// <param name="user">Payment user</param>
        /// <param name="transactionId">Payment ID</param>
        private void SavePayment(double amount, User user, string transactionId)
        {
            //add the new payment to the database
           db.Payments.Add(new ExternalPayment()
            {
                PaymentDate = DateTime.Now,
                Amount = amount,
                User = user,
                TransactionID = transactionId
            });

            //save changes
            db.SaveChanges();
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
