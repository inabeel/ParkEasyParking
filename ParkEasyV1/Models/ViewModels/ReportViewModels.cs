using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models.ViewModels
{
    public class ReportViewModels
    {
        public class DailyBookingReportViewModel
        {
            public List<Booking> DailyBookings { get; set; }
            public List<Vehicle> MyProperty { get; set; }
        }
    }
}