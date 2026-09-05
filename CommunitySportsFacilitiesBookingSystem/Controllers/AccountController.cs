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

            string email = model.Email.Trim().ToLower();

            if (db.Members.Any(m => m.Email.ToLower() == email))
            {
                ModelState.AddModelError("Email",
                    "An account already exists with this email address.");
                return View(model);
            }

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

            var selectedSports = model.SelectedSportIDs ??
                                 new System.Collections.Generic.List<int>();

            bool firstSport = true;

            foreach (int sportID in selectedSports.Distinct())
            {
                if (!db.Sports.Any(s => s.SportID == sportID && s.IsActive))
                    continue;

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

            TempData["SuccessMessage"] =
                "Registration successful. You can now sign in.";

            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string email = model.Email.Trim().ToLower();

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

            Session["MemberID"] = member.MemberID;
            Session["MemberName"] =
                member.FirstName + " " + member.LastName;
            Session["MemberEmail"] = member.Email;
            Session["UserRole"] = member.UserRole;

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