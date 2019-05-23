using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Events.Calendar;
using ParkEasyV1.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using DayPilot.Web.Mvc.Enums;
using DHTMLX.Scheduler;
using DHTMLX.Scheduler.Data;
using DHTMLX.Common;

namespace ParkEasyV1.Controllers
{

    public class SchedulerController : Controller
    {
        public static int BookingId;
        public static string Email;
        public static string Errormessage = " ";
        public static Booking request;
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        private User currentUser;

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Backend()
        {
            return new Dpc().CallBack(this);
        }

        public ActionResult Booking()
        {
            int id = BookingId;
            var model = db.Bookings.Where(b => b.ID == id).Include(f => f.Flight).Single();

            return View(model);
        }


        class Dpc : DayPilotCalendar
        {
            /// <summary>
            ///     Virtual representation of the database.
            /// </summary>
            private readonly ApplicationDbContext _db = new ApplicationDbContext();

            ///<summary>
            ///     On load, update the calendar.     
            ///</summary>             
            protected override void OnInit(InitArgs e)
            {
                Update();
            }

            /// <summary>
            ///     Change the current viewed week of classes
            /// </summary>
            /// <remarks>
            ///     Check the incoming command data for its text string
            ///     If previous, change the calendar view to the previous week
            ///     If next, move the calendar on to the next week         
            /// </remarks>
            /// <param name="e">The data string gained from a calendar time control link</param>
            protected override void OnCommand(CommandArgs e)
            {
                switch (e.Command)
                {
                    case "previous":
                        StartDate = StartDate.AddDays(-7);
                        Update(CallBackUpdateType.Full);
                        break;
                    case "next":
                        StartDate = StartDate.AddDays(7);
                        Update(CallBackUpdateType.Full);
                        break;
                }
            }

            /// <summary>
            ///     Method that sets the colours of the class events on the calendar.
            /// </summary>
            /// <remarks>
            ///     Set the font colour to black.
            ///     Check the current user's class level.
            ///     If a class level is equal to the user's level, colour the event red to denote available.
            ///     Else, colour the event gray to denote not available.
            ///     However, if the user's class level is set to Open, then colour each class to be displayed depending on their level.
            /// </remarks>
            /// <param name="e">The incoming event data</param>
            protected override void OnBeforeEventRender(BeforeEventRenderArgs e)
            {
                e.FontColor = "black";

                //string userLevel = _db.Users.Single(u => u.UserName
                //    .Equals(System.Web.HttpContext.Current.User.Identity.Name)).FirstName.ToString();

                //string classlevel = e.DataItem["ClassLevel"].ToString();
                //if (!userLevel.Equals("Open"))
                //{
                //    e.BackgroundColor = classlevel.Equals(userLevel) ? "red" : "grey";
                //}
                //else
                //{
                //    if (classlevel.Equals("Beginner"))
                //        e.BackgroundColor = "red";
                //    else if (classlevel.Equals("Intermediate"))
                //        e.BackgroundColor = "yellow";
                //    else if (classlevel.Equals("Advanced"))
                //        e.BackgroundColor = "green";
                //    else if (classlevel.Equals("Womens"))
                //        e.BackgroundColor = "pink";
                //    else if (classlevel.Equals("Private"))
                //    {
                //        e.BackgroundColor = "black";
                //        e.FontColor = "white";
                //    }
                //}

                base.OnBeforeEventRender(e);
            }

            /// <summary>
            ///     Get all class events to be listed on the calendar and display them
            /// </summary>
            /// <remarks>
            ///     If there has been no update request made, then just refresh the calendar view
            ///     If there is an update request,
            ///     Get all lessons from the database, and bind their attributues to the calendar attributes
            /// </remarks>
            protected override void OnFinish()
            {
                if (UpdateType == CallBackUpdateType.None)
                {
                    return;
                }

                //string username = System.Web.HttpContext.Current.User.Identity.Name;
                //ApplicationUser user = db.Users.First(i => i.Email == username);
                //ClassLevel level = (ClassLevel) user.ClassLevel;
                Events = from ev in _db.Bookings
                         select ev;
                //if (level == ClassLevel.Open)
                //    Events = from ev in db.Lessons
                //             select ev;
                //else
                //    Events = from ev in db.Lessons
                //             where ev.ClassLevel == level || ev.ClassLevel == ClassLevel.Private
                //             select ev ;

                DataIdField = "ID";
                DataTextField = "UserId";
                DataStartField = "BookingStart";
                DataEndField = "BookingEnd";
            }

            /// <summary>
            ///     Handle a mouse click on a scheduled lesson.
            /// </summary>
            /// <remarks>
            ///     Check that the user's current ClassLevel matches the level of the lesson clicked.
            ///     If it does match, or the user level is Open.
            ///         Set the global eventId to the id of the clicked lesson.
            ///         Redirect the user to the book attendance View.
            ///     Else
            ///         Call the OnFinish with no update request. 
            /// </remarks>
            /// <param name="e">The dataset of the lesson that was clicked</param>
            protected override void OnEventClick(EventClickArgs e)
            {
                ////Check e.Text against user classlevel
                //string userLevel = _db.Users.Single(u => u.UserName.Equals(System.Web.HttpContext.Current.User.Identity.Name)).ClassLevel.ToString();
                //if (userLevel.Equals(e.Text) || userLevel.Equals("Open"))
                //{
                    
                //}
                
                //Parse the event ID for processing
                    BookingId = int.Parse(e.Id);
                    //Redirect to the booking page
                    Redirect("/Scheduler/Index");
            }

            /// <summary>
            ///     Check the clicked time slot is available for a private session booking.
            /// </summary>
            /// <remarks>
            ///     Get all lessons on the same date as the clicked time slots.
            ///     Check against all lessons to see if there an overlap for another class.
            ///     If so, refresh the calendar age with a failure notification saying the slot cannot be booked.
            ///     if no overlap exists, create a PrivateSession and redirect the user to the PrivateSession request view.
            /// </remarks>
            /// <param name="e">The dataset of the timeslots selected by the user.</param>
            protected override void OnTimeRangeSelected(TimeRangeSelectedArgs e)
            {
                //Set a LINQ query to get all lessons that take place the same day as the requested session
                //DateTime startDate = e.Start;

                var lessons = _db.Bookings
                              .Where(l => DbFunctions.TruncateTime(l.BookingStart) == e.Start.Date)
                              .ToList();


                //If there is are any other classes that day, loop through them to check for a time conflict
                if (lessons.Count != 0)
                {
                    //Set a flag to denote if an overlap exists
                    bool overlapExists = false;

                    //Loop though all the day's classes and check if the start or end overlaps with the request time
                    foreach (var lesson in lessons)
                    {
                        bool startCondition = (e.Start.TimeOfDay > lesson.BookingStart.TimeOfDay) &&
                                              (e.Start.TimeOfDay < lesson.BookingEnd.TimeOfDay);
                        bool endCondition = (e.End.TimeOfDay > lesson.BookingStart.TimeOfDay) &&
                                            (e.End.TimeOfDay < lesson.BookingEnd.TimeOfDay);

                        if (startCondition || endCondition)
                        {
                            //Break out the loop if a match is found
                            overlapExists = true;
                            break;
                        }
                    }

                    //If there's a match, set an error message for the page alert and refresh the page to show it
                    if (overlapExists)
                    {
                        Errormessage = "Overlap";
                        Redirect("/Scheduler/Index");
                    }
                }

                //If no overlap occurs or there are no lessons booked on that day:
                //Create new session
                request = new Booking
                {
                    BookingStart = e.Start,
                    BookingEnd = e.End
                };
                //Set the start and end times
                //Send the model to the request form
                Redirect("/Bookings/Create");
            }
        }
    }
}