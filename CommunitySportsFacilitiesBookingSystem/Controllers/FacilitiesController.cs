using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CommunitySportsFacilitiesBookingSystem.Models;
using CommunitySportsFacilitiesBookingSystem.Filters;

namespace CommunitySportsFacilitiesBookingSystem.Controllers
{
    public class FacilitiesController : Controller
    {
        private CommunitySportsFacilitiesBookingDBEntities db = new CommunitySportsFacilitiesBookingDBEntities();

        // GET: Facilities
        public ActionResult Index(
            string city,
            int? facilityTypeId,
            int? sportId,
            DateTime? bookingDate,
            TimeSpan? startTime,
            TimeSpan? endTime)
        {
            bool isMember = Session["MemberID"] != null;

            // Guests receive the broad search required by the coursework.
            // Date/time availability is a member-only search feature.
            if (!isMember)
            {
                bookingDate = null;
                startTime = null;
                endTime = null;
            }

            var facilities = db.Facilities
                .Include(f => f.FacilityType)
                .Include(f => f.FacilitySports)
                .Where(f => f.IsActive);

            if (!string.IsNullOrWhiteSpace(city))
            {
                facilities = facilities.Where(f =>
                    f.City.Contains(city));
            }

            if (facilityTypeId.HasValue)
            {
                facilities = facilities.Where(f =>
                    f.FacilityTypeID == facilityTypeId.Value);
            }

            if (sportId.HasValue)
            {
                facilities = facilities.Where(f =>
                    f.FacilitySports.Any(fs =>
                        fs.SportID == sportId.Value));
            }

            bool completeAvailabilitySearch = bookingDate.HasValue &&
                                              startTime.HasValue &&
                                              endTime.HasValue;

            if (isMember && completeAvailabilitySearch)
            {
                if (bookingDate.Value.Date < DateTime.Today)
                {
                    ViewBag.SearchError = "The availability date cannot be in the past.";
                }
                else if (endTime.Value <= startTime.Value)
                {
                    ViewBag.SearchError = "End time must be later than start time.";
                }
                else
                {
                    DateTime selectedDate = bookingDate.Value.Date;
                    int dayNumber = selectedDate.DayOfWeek == DayOfWeek.Sunday
                        ? 7
                        : (int)selectedDate.DayOfWeek;

                    facilities = facilities.Where(f =>
                        f.FacilitySchedules.Any(s =>
                            s.DayOfWeek == dayNumber &&
                            s.IsAvailable &&
                            s.OpeningTime <= startTime.Value &&
                            s.ClosingTime >= endTime.Value) &&
                        !f.Bookings.Any(b =>
                            b.BookingDate == selectedDate &&
                            (b.BookingStatus == "Pending" ||
                             b.BookingStatus == "Confirmed") &&
                            startTime.Value < b.EndTime &&
                            endTime.Value > b.StartTime));
                }
            }
            else if (isMember &&
                     (bookingDate.HasValue || startTime.HasValue || endTime.HasValue))
            {
                ViewBag.SearchError =
                    "Choose a date, start time and end time to check availability.";
            }

            ViewBag.FacilityTypes = new SelectList(
                db.FacilityTypes.Where(t => t.IsActive),
                "FacilityTypeID",
                "TypeName",
                facilityTypeId);

            ViewBag.Sports = new SelectList(
                db.Sports.Where(s => s.IsActive),
                "SportID",
                "SportName",
                sportId);

            ViewBag.City = city;
            ViewBag.BookingDate = bookingDate?.ToString("yyyy-MM-dd");
            ViewBag.StartTime = startTime?.ToString(@"hh\:mm");
            ViewBag.EndTime = endTime?.ToString(@"hh\:mm");
            ViewBag.IsMember = isMember;

            return View(facilities
                .OrderBy(f => f.FacilityName)
                .ToList());
        }

        // GET: Facilities/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Facility facility = db.Facilities
                .Include(f => f.FacilityType)
                .Include(f => f.FacilitySports.Select(fs => fs.Sport))
                .Include(f => f.FacilitySchedules)
                .SingleOrDefault(f => f.FacilityID == id.Value && f.IsActive);
            if (facility == null)
            {
                return HttpNotFound();
            }
            return View(facility);
        }

        // GET: Facilities/Create
        [AdminAuthorize]
        public ActionResult Create()
        {
            ViewBag.FacilityTypeID = new SelectList(db.FacilityTypes, "FacilityTypeID", "TypeName");
            return View();
        }

        // POST: Facilities/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [AdminAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Facility facility)
        {
            if (ModelState.IsValid)
            {
                db.Facilities.Add(facility);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.FacilityTypeID = new SelectList(db.FacilityTypes, "FacilityTypeID", "TypeName", facility.FacilityTypeID);
            return View(facility);
        }

        // GET: Facilities/Edit/5
        [AdminAuthorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Facility facility = db.Facilities.Find(id);
            if (facility == null)
            {
                return HttpNotFound();
            }
            ViewBag.FacilityTypeID = new SelectList(db.FacilityTypes, "FacilityTypeID", "TypeName", facility.FacilityTypeID);
            return View(facility);
        }

        // POST: Facilities/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [AdminAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Facility facility)
        {
            if (ModelState.IsValid)
            {
                db.Entry(facility).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.FacilityTypeID = new SelectList(db.FacilityTypes, "FacilityTypeID", "TypeName", facility.FacilityTypeID);
            return View(facility);
        }

        // GET: Facilities/Delete/5
        [AdminAuthorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Facility facility = db.Facilities.Find(id);
            if (facility == null)
            {
                return HttpNotFound();
            }
            return View(facility);
        }

        // POST: Facilities/Delete/5
        [AdminAuthorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Facility facility = db.Facilities.Find(id);
            db.Facilities.Remove(facility);
            db.SaveChanges();
            return RedirectToAction("Index");
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
