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
    public class MemberSportsController : Controller
    {
        private CommunitySportsFacilitiesBookingDBEntities db = new CommunitySportsFacilitiesBookingDBEntities();

        // GET: MemberSports
        public ActionResult Index()
        {
            var memberSports = db.MemberSports.Include(m => m.Member).Include(m => m.Sport);
            return View(memberSports.ToList());
        }

        // GET: MemberSports/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MemberSport memberSport = db.MemberSports.Find(id);
            if (memberSport == null)
            {
                return HttpNotFound();
            }
            return View(memberSport);
        }

        // GET: MemberSports/Create
        public ActionResult Create()
        {
            ViewBag.MemberID = new SelectList(db.Members, "MemberID", "FirstName");
            ViewBag.SportID = new SelectList(db.Sports, "SportID", "SportName");
            return View();
        }

        // POST: MemberSports/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MemberSportID,MemberID,SportID,IsPrimaryPreference,SelectedAt")] MemberSport memberSport)
        {
            if (ModelState.IsValid)
            {
                db.MemberSports.Add(memberSport);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MemberID = new SelectList(db.Members, "MemberID", "FirstName", memberSport.MemberID);
            ViewBag.SportID = new SelectList(db.Sports, "SportID", "SportName", memberSport.SportID);
            return View(memberSport);
        }

        // GET: MemberSports/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MemberSport memberSport = db.MemberSports.Find(id);
            if (memberSport == null)
            {
                return HttpNotFound();
            }
            ViewBag.MemberID = new SelectList(db.Members, "MemberID", "FirstName", memberSport.MemberID);
            ViewBag.SportID = new SelectList(db.Sports, "SportID", "SportName", memberSport.SportID);
            return View(memberSport);
        }

        // POST: MemberSports/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MemberSportID,MemberID,SportID,IsPrimaryPreference,SelectedAt")] MemberSport memberSport)
        {
            if (ModelState.IsValid)
            {
                db.Entry(memberSport).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MemberID = new SelectList(db.Members, "MemberID", "FirstName", memberSport.MemberID);
            ViewBag.SportID = new SelectList(db.Sports, "SportID", "SportName", memberSport.SportID);
            return View(memberSport);
        }

        // GET: MemberSports/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MemberSport memberSport = db.MemberSports.Find(id);
            if (memberSport == null)
            {
                return HttpNotFound();
            }
            return View(memberSport);
        }

        // POST: MemberSports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MemberSport memberSport = db.MemberSports.Find(id);
            db.MemberSports.Remove(memberSport);
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
