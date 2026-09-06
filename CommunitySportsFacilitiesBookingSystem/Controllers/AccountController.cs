using System;
using System.Linq;
using System.Web.Helpers;
using System.Web.Mvc;
using CommunitySportsFacilitiesBookingSystem.Models;
using CommunitySportsFacilitiesBookingSystem.ViewModels;

namespace CommunitySportsFacilitiesBookingSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly CommunitySportsFacilitiesBookingDBEntities db =
            new CommunitySportsFacilitiesBookingDBEntities();

        [HttpGet]
        public ActionResult Register()
        {
            ViewBag.Sports = db.Sports
                .Where(s => s.IsActive)
                .OrderBy(s => s.SportName)
                .ToList();

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            ViewBag.Sports = db.Sports
                .Where(s => s.IsActive)
                .OrderBy(s => s.SportName)
                .ToList();

            if (!ModelState.IsValid)
                return View(model);

            string email = model.Email.Trim().ToLowerInvariant();

            if (db.Members.Any(m => m.Email.ToLower() == email))
            {
                ModelState.AddModelError("Email",
                    "An account already exists with this email address.");
                return View(model);
            }

            var selectedSports = (model.SelectedSportIDs ??
                new System.Collections.Generic.List<int>())
                .Distinct()
                .Where(id => db.Sports.Any(s => s.SportID == id && s.IsActive))
                .ToList();

            if (!selectedSports.Any())
            {
                ModelState.AddModelError("SelectedSportIDs",
                    "Select at least one preferred sport.");
                return View(model);
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                var member = new Member
                {
                    FirstName = model.FirstName.Trim(),
                    LastName = model.LastName.Trim(),
                    Email = email,
                    PasswordHash = Crypto.HashPassword(model.Password),
                    Phone = model.Phone.Trim(),
                    AddressLine = model.AddressLine.Trim(),
                    City = model.City.Trim(),
                    PostalCode = model.PostalCode?.Trim(),
                    RegisteredAt = DateTime.Now,
                    IsActive = true,
                    UserRole = "Member"
                };

                db.Members.Add(member);
                db.SaveChanges();

                bool firstSport = true;

                foreach (int sportID in selectedSports)
                {
                    db.MemberSports.Add(new MemberSport
                    {
                        MemberID = member.MemberID,
                        SportID = sportID,
                        IsPrimaryPreference = firstSport,
                        SelectedAt = DateTime.Now
                    });

                    firstSport = false;
                }

                db.SaveChanges();
                transaction.Commit();
            }

            TempData["SuccessMessage"] =
                "Registration successful. You can now sign in.";

            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
            if (!ModelState.IsValid)
                return View(model);

            string email = model.Email.Trim().ToLowerInvariant();

            var member = db.Members.FirstOrDefault(m =>
                m.Email.ToLower() == email && m.IsActive);

            bool passwordValid = false;

            if (member != null)
            {
                try
                {
                    passwordValid = Crypto.VerifyHashedPassword(
                        member.PasswordHash, model.Password);
                }
                catch
                {
                    passwordValid = false;
                }
            }

            if (!passwordValid)
            {
                ModelState.AddModelError("",
                    "Invalid email address or password.");
                return View(model);
            }

            Session.Clear();
            Session["MemberID"] = member.MemberID;
            Session["MemberName"] =
                member.FirstName + " " + member.LastName;
            Session["MemberEmail"] = member.Email;
            Session["UserRole"] = member.UserRole;

            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            TempData["SuccessMessage"] =
                "You have been signed out successfully.";


            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
