using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using CommunitySportsFacilitiesBookingSystem.Models;
using CommunitySportsFacilitiesBookingSystem.ViewModels;
using CommunitySportsFacilitiesBookingSystem.Filters;

namespace CommunitySportsFacilitiesBookingSystem.Controllers
{
    public class BookingsController : Controller
    {
        private readonly CommunitySportsFacilitiesBookingDBEntities db = new CommunitySportsFacilitiesBookingDBEntities();
        private bool IsAdmin { get { return Convert.ToString(Session["UserRole"]) == "Admin"; } }
        private ActionResult SignIn() { return RedirectToAction("Login", "Account", new { returnUrl = Request.RawUrl }); }
        [AdminAuthorize]
        public ActionResult Index() { return View(db.Bookings.Include(b => b.Facility).Include(b => b.Member).OrderByDescending(b => b.CreatedAt).ToList()); }
        public ActionResult Details(int? id)
        {
            if (Session["MemberID"] == null) return SignIn();
            int memberId = Convert.ToInt32(Session["MemberID"]);
            var booking = db.Bookings.Include(b => b.Facility).Include(b => b.Member).SingleOrDefault(b => b.BookingID == id);
            if (booking == null || (!IsAdmin && booking.MemberID != memberId)) return HttpNotFound();
            return View(booking);
        }
        private void SetSummary(Facility facility, BookingCreateViewModel model)
        {
            model.FacilityName = facility.FacilityName;
            model.HourlyRate = facility.HourlyRate;
            model.Capacity = facility.Capacity;
            ViewBag.Schedules = db.FacilitySchedules.Where(s => s.FacilityID == facility.FacilityID).OrderBy(s => s.DayOfWeek).ToList();
        }
        public ActionResult Create(int? facilityId, DateTime? bookingDate, TimeSpan? startTime, TimeSpan? endTime)
        {
            if (Session["MemberID"] == null) return SignIn();
            if (!facilityId.HasValue) return RedirectToAction("Index", "Facilities");
            var facility = db.Facilities.Find(facilityId.Value);
            if (facility == null || !facility.IsActive) return HttpNotFound();
            var model = new BookingCreateViewModel { FacilityID = facility.FacilityID,
                BookingDate = bookingDate.HasValue && bookingDate.Value.Date >= DateTime.Today ? bookingDate.Value.Date : DateTime.Today.AddDays(1),
                StartTime = startTime ?? TimeSpan.FromHours(9), EndTime = endTime ?? TimeSpan.FromHours(10), NumberOfParticipants = 1 };
            SetSummary(facility, model);
            return View(model);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(BookingCreateViewModel model)
        {
            if (Session["MemberID"] == null) return SignIn();
            // Lock the facility until commit so competing requests cannot both reserve the same slot.
            using (var transaction = db.Database.BeginTransaction())
            {
                db.Database.SqlQuery<int>("SELECT FacilityID FROM dbo.Facility WITH (UPDLOCK, HOLDLOCK) WHERE FacilityID = @p0", model.FacilityID).ToList();
                var facility = db.Facilities.Find(model.FacilityID);
                if (facility == null || !facility.IsActive) return HttpNotFound();
                SetSummary(facility, model);
                model.BookingDate = model.BookingDate.Date;
                if (model.StartTime < TimeSpan.Zero || model.EndTime >= TimeSpan.FromDays(1) || model.EndTime <= model.StartTime)
                    ModelState.AddModelError("EndTime", "Choose a valid end time after the start time on the same day.");
                if (model.StartTime >= TimeSpan.Zero && model.StartTime < TimeSpan.FromDays(1) && model.BookingDate.Add(model.StartTime) <= DateTime.Now)
                    ModelState.AddModelError("BookingDate", "Choose a date and start time in the future.");
                if (model.NumberOfParticipants > facility.Capacity)
                    ModelState.AddModelError("NumberOfParticipants", "This space accommodates up to " + facility.Capacity + " participants.");
                if (!ModelState.IsValid) return View(model);
                int day = model.BookingDate.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)model.BookingDate.DayOfWeek;
                if (!db.FacilitySchedules.Any(s => s.FacilityID == model.FacilityID && s.DayOfWeek == day && s.IsAvailable && s.OpeningTime <= model.StartTime && s.ClosingTime >= model.EndTime))
                    ModelState.AddModelError("", "Your selected time is outside this facility's opening hours.");
                if (db.Bookings.Any(b => b.FacilityID == model.FacilityID && b.BookingDate == model.BookingDate && (b.BookingStatus == "Pending" || b.BookingStatus == "Confirmed") && model.StartTime < b.EndTime && model.EndTime > b.StartTime))
                    ModelState.AddModelError("", "That time has already been reserved. Please choose another slot.");
                if (!ModelState.IsValid) return View(model);
                db.Bookings.Add(new Booking { MemberID = Convert.ToInt32(Session["MemberID"]), FacilityID = model.FacilityID,
                    BookingDate = model.BookingDate, StartTime = model.StartTime, EndTime = model.EndTime,
                    NumberOfParticipants = model.NumberOfParticipants, Purpose = model.Purpose,
                    BookingStatus = "Pending", TotalAmount = Math.Round((decimal)(model.EndTime - model.StartTime).TotalHours * facility.HourlyRate, 2, MidpointRounding.AwayFromZero), CreatedAt = DateTime.Now });
                db.SaveChanges(); transaction.Commit();
            }
            TempData["SuccessMessage"] = "Booking requested. You can track its confirmation below.";
            return RedirectToAction("MyBookings");
        }
        [AdminAuthorize]
        public ActionResult Edit(int? id)
        {
            var booking = db.Bookings.Include(b => b.Facility).Include(b => b.Member).SingleOrDefault(b => b.BookingID == id);
            if (booking == null) return HttpNotFound();
            return View(booking);
        }
        [AdminAuthorize, HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(int bookingID, string bookingStatus)
        {
            var booking = db.Bookings.Include(b => b.Facility).Include(b => b.Member).SingleOrDefault(b => b.BookingID == bookingID);
            if (booking == null) return HttpNotFound();
            bool allowed = bookingStatus == booking.BookingStatus ||
                (booking.BookingStatus == "Pending" && new[] { "Confirmed", "Rejected", "Cancelled" }.Contains(bookingStatus)) ||
                (booking.BookingStatus == "Confirmed" && new[] { "Completed", "Cancelled" }.Contains(bookingStatus));
            if (!allowed) ModelState.AddModelError("", "This status change is not allowed. Finalised bookings cannot be reopened.");
            if (bookingStatus == "Completed" && booking.BookingDate.Add(booking.EndTime) > DateTime.Now)
                ModelState.AddModelError("", "A booking can only be completed after its session ends.");
            if (!ModelState.IsValid) return View(booking);
            int changed = db.Database.ExecuteSqlCommand("UPDATE dbo.Booking SET BookingStatus=@p0, CancelledAt=CASE WHEN @p0='Cancelled' THEN @p1 ELSE CancelledAt END WHERE BookingID=@p2 AND BookingStatus=@p3", bookingStatus, DateTime.Now, bookingID, booking.BookingStatus);
            if (changed == 0) { ModelState.AddModelError("", "This booking changed while you were editing it. Reload the page and try again."); return View(booking); }
            TempData["SuccessMessage"] = "Booking status updated.";
            return RedirectToAction("Index");
        }
        [AdminAuthorize]
        public ActionResult Delete(int? id) { return RedirectToAction("Edit", new { id }); }
        [AdminAuthorize, HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TempData["ErrorMessage"] = "Booking history is retained. Use the booking status to cancel or reject a reservation.";
            return RedirectToAction("Edit", new { id });
        }
        public ActionResult MyBookings()
        {
            if (Session["MemberID"] == null) return SignIn();
            int memberId = Convert.ToInt32(Session["MemberID"]);
            ViewBag.ReviewedBookingIDs = db.Reviews.Where(r => r.Booking.MemberID == memberId).Select(r => r.BookingID).ToList();
            return View(db.Bookings.Include(b => b.Facility).Where(b => b.MemberID == memberId).OrderByDescending(b => b.BookingDate).ThenByDescending(b => b.StartTime).ToList());
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            if (Session["MemberID"] == null) return SignIn();
            int memberId = Convert.ToInt32(Session["MemberID"]);
            var booking = db.Bookings.SingleOrDefault(b => b.BookingID == id && b.MemberID == memberId);
            if (booking == null) return HttpNotFound();
            // Conditional update also prevents a concurrent admin transition being overwritten.
            int updated = db.Database.ExecuteSqlCommand("UPDATE dbo.Booking SET BookingStatus = 'Cancelled', CancelledAt = @p0 WHERE BookingID = @p1 AND MemberID = @p2 AND BookingStatus IN ('Pending','Confirmed') AND (BookingDate > @p3 OR (BookingDate = @p3 AND StartTime > @p4))", DateTime.Now, id, memberId, DateTime.Today, DateTime.Now.TimeOfDay);
            TempData[updated == 1 ? "SuccessMessage" : "ErrorMessage"] = updated == 1 ? "Your booking has been cancelled." : "Only upcoming pending or confirmed bookings can be cancelled.";
            return RedirectToAction("MyBookings");
        }
        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
