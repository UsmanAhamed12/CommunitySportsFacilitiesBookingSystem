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
    [AdminAuthorize]
    public class FacilitySchedulesController : Controller
    {
        private CommunitySportsFacilitiesBookingDBEntities db = new CommunitySportsFacilitiesBookingDBEntities();

        // GET: FacilitySchedules
        public ActionResult Index()
        {
            var facilitySchedules = db.FacilitySchedules.Include(f => f.Facility);
            return View(facilitySchedules.ToList());
        }

        // GET: FacilitySchedules/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FacilitySchedule facilitySchedule = db.FacilitySchedules.Find(id);
            if (facilitySchedule == null)
            {
                return HttpNotFound();
            }
            return View(facilitySchedule);
        }

        // GET: FacilitySchedules/Create
        public ActionResult Create()
        {
            ViewBag.FacilityID = new SelectList(db.Facilities, "FacilityID", "FacilityName");
            return View();
        }

        // POST: FacilitySchedules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ScheduleID,FacilityID,DayOfWeek,OpeningTime,ClosingTime,IsAvailable")] FacilitySchedule facilitySchedule)
        {
            if (ModelState.IsValid)
            {
                db.FacilitySchedules.Add(facilitySchedule);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.FacilityID = new SelectList(db.Facilities, "FacilityID", "FacilityName", facilitySchedule.FacilityID);
            return View(facilitySchedule);
        }

        // GET: FacilitySchedules/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FacilitySchedule facilitySchedule = db.FacilitySchedules.Find(id);
            if (facilitySchedule == null)
            {
                return HttpNotFound();
            }
            ViewBag.FacilityID = new SelectList(db.Facilities, "FacilityID", "FacilityName", facilitySchedule.FacilityID);
            return View(facilitySchedule);
        }

        // POST: FacilitySchedules/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ScheduleID,FacilityID,DayOfWeek,OpeningTime,ClosingTime,IsAvailable")] FacilitySchedule facilitySchedule)
        {
            if (ModelState.IsValid)
            {
                db.Entry(facilitySchedule).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.FacilityID = new SelectList(db.Facilities, "FacilityID", "FacilityName", facilitySchedule.FacilityID);
            return View(facilitySchedule);
        }

        // GET: FacilitySchedules/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FacilitySchedule facilitySchedule = db.FacilitySchedules.Find(id);
            if (facilitySchedule == null)
            {
                return HttpNotFound();
            }
            return View(facilitySchedule);
        }

        // POST: FacilitySchedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FacilitySchedule facilitySchedule = db.FacilitySchedules.Find(id);
            db.FacilitySchedules.Remove(facilitySchedule);
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
