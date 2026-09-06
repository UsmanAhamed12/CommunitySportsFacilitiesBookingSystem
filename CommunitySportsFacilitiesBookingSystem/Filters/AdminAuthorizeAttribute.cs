using System;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace CommunitySportsFacilitiesBookingSystem.Filters
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(
            HttpContextBase httpContext)
        {
            if (httpContext.Session["MemberID"] == null)
                return false;

            using (var db = new CommunitySportsFacilitiesBookingSystem.Models.CommunitySportsFacilitiesBookingDBEntities())
            {
                var member = db.Members.Find(Convert.ToInt32(httpContext.Session["MemberID"]));
                return member != null && member.IsActive && member.UserRole == "Admin";
            }
        }

        protected override void HandleUnauthorizedRequest(
            AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Session["MemberID"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                    {
                        { "controller", "Account" },
                        { "action", "Login" }
                    });

                return;
            }

            filterContext.Result = new HttpStatusCodeResult(
                HttpStatusCode.Forbidden,
                "Administrator access is required.");
        }
    }
}
