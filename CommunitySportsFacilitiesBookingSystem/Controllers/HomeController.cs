using System;
using System.Linq;
using System.Web.Mvc;
using CommunitySportsFacilitiesBookingSystem.Models;
using CommunitySportsFacilitiesBookingSystem.Filters;
namespace CommunitySportsFacilitiesBookingSystem.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index() { return View(); }
        public ActionResult About() { return View(); }
        public ActionResult Contact() { return RedirectToAction("Create", "Inquiries"); }
        [AdminAuthorize]
        public ActionResult Dashboard()
        {
            using (var db = new CommunitySportsFacilitiesBookingDBEntities())
            {
                ViewBag.PendingBookings = db.Bookings.Count(b => b.BookingStatus == "Pending");
                ViewBag.ActiveMembers = db.Members.Count(m => m.IsActive);
                ViewBag.NewInquiries = db.Inquiries.Count(i => i.InquiryStatus == "New");
                return View();
            }
        }
    }
}
