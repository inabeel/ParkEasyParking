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
    /// Controller for handling all payment events and actions
    /// </summary>
    public class PaymentsController : Controller
    {
        /// <summary>
        /// Global variable to hold the ApplicationDbContext
        /// </summary>
        private ApplicationDbContext db = new ApplicationDbContext();        

        // GET: Payments
        public ActionResult Index()
        {
            var payments = db.Payments.Include(p => p.User);
            return View(payments.ToList());
        }

        /// <summary>
        /// HTTP GET ActionResult for processing booking payments
        /// </summary>
        /// <returns>Payment view</returns>
        // GET: Payments/Charge
        [Authorize]
        public ActionResult Charge(int? id)
        {
            //create new instance of user manager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            //store stripe publishable key in viewbag to be used on the front-end
            ViewBag.StripePublishableKey = ConfigurationManager.AppSettings["StripePublishableKey"];

            //create a variable to hold the booking and initialize as null
            Booking booking=null;

            //check if the payment is linked to a specific booking id parameter
            //new bookings will not have an id parameter - but invoice booking payments will
            if (id==null)
            {
                //find and store booking from tempdata
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

            //if the user is in the customer role
            if (User.IsInRole("Customer"))
            {
                //find the user via email using the user manager and parse User to Customer
                Models.Customer customer = userManager.FindByEmail(User.Identity.GetUserName()) as Models.Customer;
                //store the true or false boolean if customer is a corporate customer in a viewbag to be used in the front-end
                ViewBag.Corporate = customer.Corporate;
            }            

            //store the booking total and stripe payment total (total * 100 for cents) and store in viewbag to be used in front-end
            ViewBag.Total = booking.Total;
            ViewBag.StripeTotal = (int)Math.Ceiling(booking.Total*100);

            //store the booking id in a new temp data to avoid it being cleared after request
            TempData["bID"] = booking.ID;
            //return the view
            return View();
        }

        /// <summary>
        /// HttpPost ActionResult for processing a Stripe API card payment
        /// </summary>
        /// <param name="stripeEmail"></param>
        /// <param name="stripeToken"></param>
        /// <returns>Redirect MyBookings</returns>
        // POST: Payments/Charge
        [HttpPost]
        public ActionResult Charge(string stripeEmail, string stripeToken)
        {
            //find the booking in the database using the tempdata booking id variable
            Booking booking = db.Bookings.Find(TempData["bID"]);
            
            //initialize stripe customers and charges variables
            var customers = new CustomerService();
            var charges = new ChargeService();

            //create a new stripe customer using email and token from stripe payment
            var customer = customers.Create(new CustomerCreateOptions
            {
                Email = stripeEmail,
                SourceToken = stripeToken,
            });

            //create a new stripe charge 
            var charge = charges.Create(new ChargeCreateOptions
            {
                Amount = (int)Math.Ceiling(booking.Total * 100),    //charge amount is the booking total * 100 for cents
                Description = "ParkEasy Airport Parking Charge",    //payment descriptor
                Currency = "gbp",   //payment currency is GBP
                CustomerId = customer.Id,   //store customer id in stripe customer
                ReceiptEmail = customer.Email,  //store receipt email from stripe customer
            });

            //update booking status
            booking.BookingStatus = BookingStatus.Confirmed;            

            //add the stripe payment to the Payments database table 
            db.Payments.Add(new ExternalPayment()
            {
                PaymentDate = DateTime.Now, //payment date is now
                Amount = charge.Amount, //payment amount is stripe total
                User = booking.User,    //set user associated with payment to the user associated with the booking
                TransactionID = charge.ReceiptNumber    //set the transaction id to the receipt number from stripe payment
            });
            db.SaveChanges();   //save changes to the database

            //if the tempdata for storing an invoice is not null
            // *IF THIS PAYMENT IS FOR A BOOKING THAT HAS AN INVOICE*
            if (TempData["Invoice"]!=null)
            {
                //update the invoice status to paid
                booking.Invoice.Status = InvoiceStatus.Paid;
                db.SaveChanges();   //save changes
                return RedirectToAction("Confirmation", "Invoice", new { id=booking.ID});   //return invoice payment confirmation view
            }

            
            //call method to email booking confirmation
            booking.EmailConfirmation();

            //return the booking confirmation
            return RedirectToAction("Confirmation", "Bookings", new { id=booking.ID});
        }

        /// <summary>
        /// ActionResult to handle the event of a staff member processing an on-the-spot booking with a cash payment
        /// </summary>
        /// <returns></returns>
        public ActionResult CashPayment()
        {
            //get the booking from the database using the booking id stored in the temp data
            Booking booking = db.Bookings.Find(TempData["bID"]);

            //update the booking status to confirmed (as staff has recieved a cash payment on-site)
            booking.BookingStatus = BookingStatus.Confirmed;

            //add the cash payment to the Payments table
            db.Payments.Add(new Cash()
            {
                PaymentDate = DateTime.Now, //payment date is now
                Amount = booking.Total, //payment amount is the booking total amount
                User = booking.User //user associated with the payment is the user associated with the booking
            });
            db.SaveChanges();   //save database changes

            //return the booking confirmation
            return RedirectToAction("Confirmation", "Bookings", new { id = booking.ID });
        }

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
        /// <param name="Cancel">Initialize cancel parameter as null</param>
        /// <returns>Booking confirmation view</returns>
        public ActionResult PaymentWithPaypal(string Cancel = null)
        {
            //find the booking being paid for using the booking id stored in tempdata
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
                        //update error message, reset viewbag total and return the payment charge view
                        TempData["Error"] = "We were unable to process your payment. Please try again.";
                        ViewBag.Total = booking.Total;
                        return View("Charge");
                    }
                }
            }
            catch (Exception ex)
            {
                //update error message, reset booking total viewbag and return the payment charge view
                TempData["Error"] = "We were unable to process your payment. Please try again.";
                ViewBag.Total = booking.Total;
                return View("Charge");
            }

            //if invoice payment is true
            //*IF THE BOOKING BEING PAID FOR HAS AN INVOICE*
            if ((bool)TempData["InvoicePayment"])
            {
                //update the invoice status to paid and return the invoice payment confirmation
                booking.Invoice.Status = InvoiceStatus.Paid;
                return RedirectToAction("Confirmation", "Invoice", new { id = booking.ID });
            }

            //update booking status to confirmed and save database changes
            booking.BookingStatus = BookingStatus.Confirmed;
            db.SaveChanges();
            booking.EmailConfirmation();    //call method to send booking email confirmation to the user
            //on successful payment, show success page to user.  
            return RedirectToAction("Confirmation", "Bookings", new { id=booking.ID});
        }
        //private attributes to store the payment and execute payment
        private PayPal.Api.Payment payment;
        private PayPal.Api.Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            //create a new payment execution and set the payer id
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            //update the payment id in the payment variable
            this.payment = new PayPal.Api.Payment()
            {
                id = paymentId
            };
            //return the payment execute result
            return this.payment.Execute(apiContext, paymentExecution);
        }
        //create paypal payment
        private PayPal.Api.Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {
            //get the booking being paid for from the database using the booking id stored in tempdata
            Booking booking = db.Bookings.Find(TempData["bID"]);

            //create itemlist and add item objects to it  
            var itemList = new ItemList()
            {
                items = new List<Item>()
            };
            //Adding Item Details like name, currency, price etc  
            itemList.items.Add(new Item()
            {
                name = "ParkEasy Airport Parking Booking",  //name of item is ParkEasy Booking
                currency = "GBP",   //use GBP currency
                price = booking.Total.ToString(),   //paypal payment price is the booking total
                quantity = "1", //one booking charge
                sku = booking.ParkingSlot.ID.ToString() //stock keeping unit number is the parking slot associated with the booking
            });
            //create a new payer and set payment method
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
                tax = "0",  //no tax (already included in subtotal)
                shipping = "0", //no shipping costs
                subtotal = booking.Total.ToString(),    //subtotal of booking is the booking total
            };
            //Final amount with details  
            var amount = new Amount()
            {
                currency = "GBP",   //currency is GBP
                total = booking.Total.ToString(), // Total must be equal to sum of tax, shipping and subtotal.  
                details = details   //set the amount details
            };
            var transactionList = new List<Transaction>();
            // Adding description about the transaction  
            transactionList.Add(new Transaction()
            {
                description = "ParkEasy Airport Parking Booking",   //transaction desc is ParkEasy Booking
                //invoice_number = "your generated invoice number", //Generate an Invoice No  
                amount = amount,    //transaction amount is booking amount
                item_list = itemList    //set transaction items to items
            });            
            this.payment = new PayPal.Api.Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };

            //save the payment for this booking
            SavePayment(booking.Total, booking.User, payment.id);

            // Create a payment using a APIContext  
            return this.payment.Create(apiContext);
        }

        /// <summary>
        /// ActionResult to Confirm a booking for corporate customers who will be invoiced for payment
        /// </summary>
        /// <returns>Booking Confirmation</returns>
        public ActionResult InvoiceCharge()
        {
            //if a corporate customer places a booking, the booking will be automatically confirmed
            //and the invoice clerk will be able to generate an invoice for them at a later date

            //find the booking from the database using the booking id stored in the tempdata
            Booking booking = db.Bookings.Find(TempData["bID"]);

            //update the booking status to confirmed
            booking.BookingStatus = BookingStatus.Confirmed;
            db.SaveChanges();   //save the database changes

            //return the booking confirmation
            return RedirectToAction("Confirmation", "Bookings", new { id = booking.ID });
        }

        /// <summary>
        /// Method for saving external payment to the database
        /// </summary>
        /// <param name="amount">payment amount charged</param>
        /// <param name="user">user associated with payment</param>
        /// <param name="transactionId">id of the transaction</param>
        private void SavePayment(double amount, User user, string transactionId)
        {
            //add the payment to the Payments table in the database
           db.Payments.Add(new ExternalPayment()
            {
                PaymentDate = DateTime.Now, //payment date is now
                Amount = amount,    //payment amount is the amount parameter passed in
                User = user,    //payment user is the user passed in
                TransactionID = transactionId   //transaction id is the transaction id passed in
            });
            //save changes to the database
            db.SaveChanges();
        }

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
