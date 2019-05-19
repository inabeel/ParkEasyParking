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
    public class PaymentsController : Controller
    {
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
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            ViewBag.StripePublishableKey = ConfigurationManager.AppSettings["StripePublishableKey"];

            Booking booking=null;

            //check if the payment is linked to a specific booking id parameter
            //new bookings will not have an id parameter - but invoice booking payments will
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

            Models.Customer customer = userManager.FindByEmail(User.Identity.GetUserName()) as Models.Customer;
            ViewBag.Corporate = customer.Corporate;

            ViewBag.Total = booking.Total;
            ViewBag.StripeTotal = (int)Math.Ceiling(booking.Total*100);

            TempData["bID"] = booking.ID;
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
            Booking booking = db.Bookings.Find(TempData["bID"]);
            
            var customers = new CustomerService();
            var charges = new ChargeService();

            var customer = customers.Create(new CustomerCreateOptions
            {
                Email = stripeEmail,
                SourceToken = stripeToken,
            });

            var charge = charges.Create(new ChargeCreateOptions
            {
                Amount = (int)Math.Ceiling(booking.Total * 100),
                Description = "ParkEasy Airport Parking Charge",
                Currency = "gbp",
                CustomerId = customer.Id,
                ReceiptEmail = customer.Email,
            });

            booking.BookingStatus = BookingStatus.Confirmed;            

            db.Payments.Add(new ExternalPayment()
            {
                PaymentDate = DateTime.Now,
                Amount = charge.Amount,
                User = booking.User,
                TransactionID = charge.ReceiptNumber
            });
            db.SaveChanges();

            if (TempData["Invoice"]!=null)
            {
                booking.Invoice.Status = InvoiceStatus.Paid;
                db.SaveChanges();
                return RedirectToAction("Confirmation", "Invoice", new { id=booking.ID});
            }

            

            booking.EmailConfirmation();

            return RedirectToAction("Confirmation", "Bookings", new { id=booking.ID});
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
                //payment.PaymentDate = DateTime.Now;

                //Booking booking = db.Bookings.Find(TempData["bookingID"]);

                //payment.User = userManager.FindByName(User.Identity.Name);

                //booking.BookingStatus = BookingStatus.Confirmed;

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
        /// <param name="Cancel"></param>
        /// <returns></returns>
        public ActionResult PaymentWithPaypal(string Cancel = null)
        {
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
            catch (Exception ex)
            {
                TempData["Error"] = "We were unable to process your payment. Please try again.";
                ViewBag.Total = booking.Total;
                return View("Charge");
            }

            if ((bool)TempData["InvoicePayment"])
            {
                booking.Invoice.Status = InvoiceStatus.Paid;
                return RedirectToAction("Confirmation", "Invoice", new { id = booking.ID });
            }

            booking.BookingStatus = BookingStatus.Confirmed;
            db.SaveChanges();
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
        /// <returns></returns>
        public ActionResult InvoiceCharge()
        {
            Booking booking = db.Bookings.Find(TempData["bID"]);

            booking.BookingStatus = BookingStatus.Confirmed;
            db.SaveChanges();

            return RedirectToAction("Confirmation", "Bookings", new { id = booking.ID });
        }

        private void SavePayment(double amount, User user, string transactionId)
        {
           db.Payments.Add(new ExternalPayment()
            {
                PaymentDate = DateTime.Now,
                Amount = amount,
                User = user,
                TransactionID = transactionId
            });

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
