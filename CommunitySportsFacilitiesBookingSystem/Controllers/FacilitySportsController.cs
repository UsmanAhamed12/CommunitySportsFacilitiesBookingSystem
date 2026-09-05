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
    public class FacilitySportsController : Controller
    {
        private CommunitySportsFacilitiesBookingDBEntities db = new CommunitySportsFacilitiesBookingDBEntities();

        // GET: FacilitySports
        public ActionResult Index()
        {
            var facilitySports = db.FacilitySports.Include(f => f.Facility).Include(f => f.Sport);
            return View(facilitySports.ToList());
        }

        // GET: FacilitySports/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FacilitySport facilitySport = db.FacilitySports.Find(id);
            if (facilitySport == null)
            {
                return HttpNotFound();
            }
            return View(facilitySport);
        }

        // GET: FacilitySports/Create
        public ActionResult Create()
        {
            ViewBag.FacilityID = new SelectList(db.Facilities, "FacilityID", "FacilityName");
            ViewBag.SportID = new SelectList(db.Sports, "SportID", "SportName");
            return View();
        }

        // POST: FacilitySports/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FacilitySportID,FacilityID,SportID,IsPrimarySport,AddedAt")] FacilitySport facilitySport)
        {
            if (ModelState.IsValid)
            {
                db.FacilitySports.Add(facilitySport);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.FacilityID = new SelectList(db.Facilities, "FacilityID", "FacilityName", facilitySport.FacilityID);
            ViewBag.SportID = new SelectList(db.Sports, "SportID", "SportName", facilitySport.SportID);
            return View(facilitySport);
        }

        // GET: FacilitySports/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FacilitySport facilitySport = db.FacilitySports.Find(id);
            if (facilitySport == null)
            {
                return HttpNotFound();
            }
            ViewBag.FacilityID = new SelectList(db.Facilities, "FacilityID", "FacilityName", facilitySport.FacilityID);
            ViewBag.SportID = new SelectList(db.Sports, "SportID", "SportName", facilitySport.SportID);
            return View(facilitySport);
        }

        // POST: FacilitySports/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FacilitySportID,FacilityID,SportID,IsPrimarySport,AddedAt")] FacilitySport facilitySport)
        {
            if (ModelState.IsValid)
            {
                db.Entry(facilitySport).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.FacilityID = new SelectList(db.Facilities, "FacilityID", "FacilityName", facilitySport.FacilityID);
            ViewBag.SportID = new SelectList(db.Sports, "SportID", "SportName", facilitySport.SportID);
            return View(facilitySport);
        }

        // GET: FacilitySports/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FacilitySport facilitySport = db.FacilitySports.Find(id);
            if (facilitySport == null)
            {
                return HttpNotFound();
            }
            return View(facilitySport);
        }

        // POST: FacilitySports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FacilitySport facilitySport = db.FacilitySports.Find(id);
            db.FacilitySports.Remove(facilitySport);
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
