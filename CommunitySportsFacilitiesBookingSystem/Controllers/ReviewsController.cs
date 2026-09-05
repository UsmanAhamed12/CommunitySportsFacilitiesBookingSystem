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

namespace CommunitySportsFacilitiesBookingSystem.Controllers
{
    public class ReviewsController : Controller
    {
        private CommunitySportsFacilitiesBookingDBEntities db = new CommunitySportsFacilitiesBookingDBEntities();

        // GET: Reviews
        public ActionResult Index(int? facilityId, string search)
        {
            var reviews = db.Reviews
                .Include(r => r.Booking.Facility)
                .Include(r => r.Booking.Member)
                .Where(r => r.IsApproved);

            if (facilityId.HasValue)
            {
                reviews = reviews.Where(r =>
                    r.Booking.FacilityID == facilityId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                reviews = reviews.Where(r =>
                    r.ReviewTitle.Contains(search) ||
                    r.ReviewComment.Contains(search) ||
                    r.Booking.Facility.FacilityName.Contains(search));
            }

            ViewBag.Facilities = new SelectList(
                db.Facilities
                    .Where(f => f.IsActive)
                    .OrderBy(f => f.FacilityName),
                "FacilityID",
                "FacilityName",
                facilityId);

            ViewBag.Search = search;

            return View(reviews
                .OrderByDescending(r => r.SubmittedAt)
                .ToList());
        }

        // GET: Reviews/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Review review = db.Reviews
                .Include(r => r.Booking.Facility)
                .Include(r => r.Booking.Member)
                .SingleOrDefault(r => r.ReviewID == id.Value && r.IsApproved);
            if (review == null)
            {
                return HttpNotFound();
            }
            return View(review);
        }

        // GET: Reviews/Create
        public ActionResult Create(int? bookingId)
        {
            if (Session["MemberID"] == null)
                return RedirectToAction("Login", "Account");

            if (!bookingId.HasValue)
                return RedirectToAction("MyBookings", "Bookings");

            int memberId = Convert.ToInt32(Session["MemberID"]);

            var booking = db.Bookings
                .Include(b => b.Facility)
                .SingleOrDefault(b =>
                    b.BookingID == bookingId.Value &&
                    b.MemberID == memberId);

            if (booking == null)
                return HttpNotFound();

            if (booking.BookingStatus != "Completed")
            {
                TempData["ErrorMessage"] =
                    "Reviews can only be submitted for completed bookings.";

                return RedirectToAction("MyBookings", "Bookings");
            }

            if (db.Reviews.Any(r => r.BookingID == booking.BookingID))
            {
                TempData["ErrorMessage"] =
                    "A review has already been submitted for this booking.";

                return RedirectToAction("MyBookings", "Bookings");
            }

            var model = new ReviewCreateViewModel
            {
                BookingID = booking.BookingID,
                FacilityName = booking.Facility.FacilityName,
                OverallRating = 5,
                ConditionRating = 5,
                AmenitiesRating = 5
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ReviewCreateViewModel model)
        {
            if (Session["MemberID"] == null)
                return RedirectToAction("Login", "Account");

            int memberId = Convert.ToInt32(Session["MemberID"]);

            var booking = db.Bookings
                .Include(b => b.Facility)
                .SingleOrDefault(b =>
                    b.BookingID == model.BookingID &&
                    b.MemberID == memberId);

            if (booking == null)
                return HttpNotFound();

            model.FacilityName = booking.Facility.FacilityName;

            if (booking.BookingStatus != "Completed")
            {
                ModelState.AddModelError(
                    "",
                    "Reviews can only be submitted for completed bookings.");
            }

            if (db.Reviews.Any(r => r.BookingID == model.BookingID))
            {
                ModelState.AddModelError(
                    "",
                    "A review has already been submitted for this booking.");
            }

            if (!ModelState.IsValid)
                return View(model);

            var review = new Review
            {
                BookingID = model.BookingID,
                OverallRating = (byte)model.OverallRating,
                ConditionRating = (byte)model.ConditionRating,
                AmenitiesRating = (byte)model.AmenitiesRating,
                ReviewTitle = model.ReviewTitle,
                ReviewComment = model.ReviewComment,
                SubmittedAt = DateTime.Now,
                IsApproved = true
            };

            db.Reviews.Add(review);
            db.SaveChanges();

            TempData["SuccessMessage"] =
                "Your review was submitted successfully.";

            return RedirectToAction("MyBookings", "Bookings");
        }

        // GET: Reviews/Edit/5
        [CommunitySportsFacilitiesBookingSystem.Filters.AdminAuthorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Review review = db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            ViewBag.BookingID = new SelectList(db.Bookings, "BookingID", "Purpose", review.BookingID);
            return View(review);
        }

        // POST: Reviews/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [CommunitySportsFacilitiesBookingSystem.Filters.AdminAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ReviewID,BookingID,OverallRating,ConditionRating,AmenitiesRating,ReviewTitle,ReviewComment,SubmittedAt,IsApproved")] Review review)
        {
            if (ModelState.IsValid)
            {
                db.Entry(review).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BookingID = new SelectList(db.Bookings, "BookingID", "Purpose", review.BookingID);
            return View(review);
        }

        // GET: Reviews/Delete/5
        [CommunitySportsFacilitiesBookingSystem.Filters.AdminAuthorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Review review = db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            return View(review);
        }

        // POST: Reviews/Delete/5
        [CommunitySportsFacilitiesBookingSystem.Filters.AdminAuthorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Review review = db.Reviews.Find(id);
            db.Reviews.Remove(review);
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
