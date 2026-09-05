using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CommunitySportsFacilitiesBookingSystem.Models;
using CommunitySportsFacilitiesBookingSystem.ViewModels;
using CommunitySportsFacilitiesBookingSystem.Filters;

namespace CommunitySportsFacilitiesBookingSystem.Controllers
{
    public class BookingsController : Controller
    {
        private CommunitySportsFacilitiesBookingDBEntities db = new CommunitySportsFacilitiesBookingDBEntities();
        // GET: Bookings
        [AdminAuthorize]
        public ActionResult Index()
        {
            var bookings = db.Bookings.Include(b => b.Facility).Include(b => b.Member);
            return View(bookings.ToList());
        }

        // GET: Bookings/Details/5
        public ActionResult Details(int? id)
        {
            if (Session["MemberID"] == null)
                return RedirectToAction("Login", "Account");

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int memberId = Convert.ToInt32(Session["MemberID"]);
            Booking booking = db.Bookings
                .Include(b => b.Facility)
                .SingleOrDefault(b => b.BookingID == id.Value &&
                                      b.MemberID == memberId);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // GET: Bookings/Create
        public ActionResult Create(int? facilityId)
        {
            if (Session["MemberID"] == null)
                return RedirectToAction("Login", "Account");

            if (!facilityId.HasValue)
                return RedirectToAction("Index", "Facilities");

            var facility = db.Facilities.Find(facilityId.Value);

            if (facility == null || !facility.IsActive)
                return HttpNotFound();

            var model = new BookingCreateViewModel
            {
                FacilityID = facility.FacilityID,
                FacilityName = facility.FacilityName,
                BookingDate = DateTime.Today.AddDays(1),
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(10, 0, 0),
                NumberOfParticipants = 1
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BookingCreateViewModel model)
        {
            if (Session["MemberID"] == null)
                return RedirectToAction("Login", "Account");

            var facility = db.Facilities.Find(model.FacilityID);

            if (facility == null || !facility.IsActive)
                return HttpNotFound();

            model.FacilityName = facility.FacilityName;

            if (model.BookingDate.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    "BookingDate",
                    "The booking date cannot be in the past.");
            }

            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError(
                    "EndTime",
                    "End time must be later than start time.");
            }

            if (model.NumberOfParticipants > facility.Capacity)
            {
                ModelState.AddModelError(
                    "NumberOfParticipants",
                    "The number of participants exceeds facility capacity.");
            }

            int dayNumber = model.BookingDate.DayOfWeek ==
                            DayOfWeek.Sunday
                ? 7
                : (int)model.BookingDate.DayOfWeek;

            bool withinOpeningHours =
                db.FacilitySchedules.Any(s =>
                    s.FacilityID == model.FacilityID &&
                    s.DayOfWeek == dayNumber &&
                    s.IsAvailable &&
                    s.OpeningTime <= model.StartTime &&
                    s.ClosingTime >= model.EndTime);

            if (!withinOpeningHours)
            {
                ModelState.AddModelError(
                    "",
                    "The facility is unavailable during the selected time.");
            }

            bool hasConflict = db.Bookings.Any(b =>
                b.FacilityID == model.FacilityID &&
                b.BookingDate == model.BookingDate &&
                (b.BookingStatus == "Pending" ||
                 b.BookingStatus == "Confirmed") &&
                model.StartTime < b.EndTime &&
                model.EndTime > b.StartTime);

            if (hasConflict)
            {
                ModelState.AddModelError(
                "",
                "This facility is already booked during the selected time.");
            }

            if (!ModelState.IsValid)
                return View(model);

            decimal durationHours =
                (decimal)(model.EndTime - model.StartTime).TotalHours;

            var booking = new Booking
            {
                MemberID = Convert.ToInt32(Session["MemberID"]),
                FacilityID = model.FacilityID,
                BookingDate = model.BookingDate.Date,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                NumberOfParticipants = model.NumberOfParticipants,
                Purpose = model.Purpose,
                BookingStatus = "Pending",
                TotalAmount = durationHours * facility.HourlyRate,
                CreatedAt = DateTime.Now
            };

            db.Bookings.Add(booking);
            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Your facility booking was submitted successfully.";

            return RedirectToAction("MyBookings");
        }

        // GET: Bookings/Edit/5
        [AdminAuthorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            ViewBag.FacilityID = new SelectList(db.Facilities, "FacilityID", "FacilityName", booking.FacilityID);
            ViewBag.MemberID = new SelectList(db.Members, "MemberID", "FirstName", booking.MemberID);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [AdminAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Booking booking)
        {
            if (ModelState.IsValid)
            {
                db.Entry(booking).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.FacilityID = new SelectList(db.Facilities, "FacilityID", "FacilityName", booking.FacilityID);
            ViewBag.MemberID = new SelectList(db.Members, "MemberID", "FirstName", booking.MemberID);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        [AdminAuthorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // POST: Bookings/Delete/5
        [AdminAuthorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Booking booking = db.Bookings.Find(id);
            db.Bookings.Remove(booking);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult MyBookings()
        {
            if (Session["MemberID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int memberId = Convert.ToInt32(Session["MemberID"]);

            var bookings = db.Bookings
                .Include(b => b.Facility)
                .Where(b => b.MemberID == memberId)
                .OrderByDescending(b => b.BookingDate)
                .ThenByDescending(b => b.StartTime)
                .ToList();

            ViewBag.ReviewedBookingIDs = db.Reviews
                .Where(r => r.Booking.MemberID == memberId)
                .Select(r => r.BookingID)
                .ToList();

            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            if (Session["MemberID"] == null)
                return RedirectToAction("Login", "Account");

            int memberId = Convert.ToInt32(Session["MemberID"]);

            var booking = db.Bookings.SingleOrDefault(b =>
                b.BookingID == id &&
                b.MemberID == memberId);

            if (booking == null)
                return HttpNotFound();

            if (booking.BookingStatus != "Pending" &&
                booking.BookingStatus != "Confirmed")
            {
                TempData["ErrorMessage"] =
                    "Only pending or confirmed bookings can be cancelled.";

                return RedirectToAction("MyBookings");
            }

            booking.BookingStatus = "Cancelled";
            booking.CancelledAt = DateTime.Now;

            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Your booking was cancelled successfully.";

            return RedirectToAction("MyBookings");
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
