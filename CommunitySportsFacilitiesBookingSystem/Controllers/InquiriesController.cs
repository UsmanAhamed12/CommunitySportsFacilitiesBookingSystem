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
    public class InquiriesController : Controller
    {
        private CommunitySportsFacilitiesBookingDBEntities db = new CommunitySportsFacilitiesBookingDBEntities();

        // GET: Inquiries
        [AdminAuthorize]
        public ActionResult Index()
        {
            return View(db.Inquiries.ToList());
        }

        // GET: Inquiries/Details/5
        [AdminAuthorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Inquiry inquiry = db.Inquiries.Find(id);
            if (inquiry == null)
            {
                return HttpNotFound();
            }
            return View(inquiry);
        }

        // GET: Inquiries/Create
        public ActionResult Create()
        {
            var model = new InquiryCreateViewModel();

            if (Session["MemberID"] != null)
            {
                int memberId = Convert.ToInt32(Session["MemberID"]);
                var member = db.Members.Find(memberId);

                if (member != null)
                {
                    model.SenderName =
                        member.FirstName + " " + member.LastName;

                    model.SenderEmail = member.Email;
                    model.SenderPhone = member.Phone;
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(InquiryCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var inquiry = new Inquiry
            {
                SenderName = model.SenderName.Trim(),
                SenderEmail = model.SenderEmail.Trim().ToLower(),
                SenderPhone = model.SenderPhone?.Trim(),
                InquirySubject = model.InquirySubject.Trim(),
                InquiryMessage = model.InquiryMessage.Trim(),
                SubmittedAt = DateTime.Now,
                InquiryStatus = "New",
                ResponseMessage = null,
                RespondedAt = null
            };

            db.Inquiries.Add(inquiry);
            db.SaveChanges();

            TempData["InquirySuccess"] =
                "Your inquiry was submitted successfully. The Sports Council will respond soon.";

            return RedirectToAction("Create");
        }

        // GET: Inquiries/Edit/5
        [AdminAuthorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Inquiry inquiry = db.Inquiries.Find(id);
            if (inquiry == null)
            {
                return HttpNotFound();
            }
            return View(inquiry);
        }

        // POST: Inquiries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [AdminAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int inquiryID, string inquiryStatus, string responseMessage)
        {
            var inquiry = db.Inquiries.Find(inquiryID);
            if (inquiry == null) return HttpNotFound();
            if (!new[] { "New", "InProgress", "Resolved", "Closed" }.Contains(inquiryStatus)) ModelState.AddModelError("", "Choose a valid inquiry status.");
            if (responseMessage != null && responseMessage.Length > 1000) ModelState.AddModelError("", "Keep the response within 1,000 characters.");
            if (inquiryStatus == "Resolved" && string.IsNullOrWhiteSpace(responseMessage)) ModelState.AddModelError("", "Enter a response before marking this inquiry responded.");
            if (!ModelState.IsValid) return View(inquiry);
            inquiry.InquiryStatus = inquiryStatus;
            inquiry.ResponseMessage = responseMessage;
            inquiry.RespondedAt = string.IsNullOrWhiteSpace(responseMessage) ? (DateTime?)null : DateTime.Now;
            db.SaveChanges(); TempData["SuccessMessage"] = "Inquiry updated. Responses are recorded here; no email is sent automatically.";
            return RedirectToAction("Index");
        }
        // GET: Inquiries/Delete/5
        [AdminAuthorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Inquiry inquiry = db.Inquiries.Find(id);
            if (inquiry == null)
            {
                return HttpNotFound();
            }
            return View(inquiry);
        }

        // POST: Inquiries/Delete/5
        [AdminAuthorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Inquiry inquiry = db.Inquiries.Find(id);
            if (inquiry == null) return HttpNotFound();
            db.Inquiries.Remove(inquiry);
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
