using System;
using System.Linq;
using System.Web.Mvc;
using CommunitySportsFacilitiesBookingSystem.Models;
using CommunitySportsFacilitiesBookingSystem.Filters;
namespace CommunitySportsFacilitiesBookingSystem.Controllers
{
    [AdminAuthorize]
    public class MembersController : Controller
    {
        private readonly CommunitySportsFacilitiesBookingDBEntities db = new CommunitySportsFacilitiesBookingDBEntities();
        public ActionResult Index() { return View(db.Members.OrderBy(m => m.FirstName).ThenBy(m => m.LastName).ToList()); }
        public ActionResult Details(int? id) { var member = db.Members.Find(id); if (member == null) return HttpNotFound(); return View(member); }
        public ActionResult Create() { return RedirectToAction("Register", "Account"); }
        public ActionResult Edit(int? id) { var member = db.Members.Find(id); if (member == null) return HttpNotFound(); return View(member); }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(int memberID, bool isActive)
        {
            var member = db.Members.Find(memberID); if (member == null) return HttpNotFound();
            if (!isActive && member.MemberID == Convert.ToInt32(Session["MemberID"])) { ModelState.AddModelError("", "You cannot deactivate your own administrator account."); return View(member); }
            member.IsActive = isActive; db.SaveChanges(); TempData["SuccessMessage"] = "Member access updated."; return RedirectToAction("Index");
        }
        public ActionResult Delete(int? id) { return RedirectToAction("Edit", new { id }); }
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id) { return Edit(id, false); }
        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}
