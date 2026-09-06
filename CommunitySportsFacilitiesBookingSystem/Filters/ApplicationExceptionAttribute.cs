using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Web.Mvc;
using CommunitySportsFacilitiesBookingSystem.Models;
namespace CommunitySportsFacilitiesBookingSystem.Filters
{
    public class ApplicationExceptionAttribute : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.ExceptionHandled) return;
            Trace.TraceError(context.Exception.ToString());
            var error = context.Exception;
            bool conflict = error is DbUpdateException || error is DbEntityValidationException;
            bool database = false;
            for (var cause = error; cause != null; cause = cause.InnerException)
                if (cause is SqlException) { database = true; var sql = (SqlException)cause; conflict = conflict || sql.Number == 2601 || sql.Number == 2627 || sql.Number == 547; }
            context.ExceptionHandled = true;
            context.HttpContext.Response.StatusCode = error is HttpAntiForgeryException ? 400 : conflict ? 409 : database ? 503 : 500;
            context.HttpContext.Response.TrySkipIisCustomErrors = true;
            var result = new ViewResult { ViewName = "~/Views/Shared/Error.cshtml" };
            result.ViewBag.Title = conflict ? "Check your request" : "We couldn't complete that request";
            result.ViewBag.ErrorMessage = conflict ? "This change conflicts with an existing record or a required field. Check for duplicate entries and linked records, then try again." : database ? "The booking service cannot reach its database right now. Please try again shortly or contact the council." : "Something went wrong while processing your request. Please try again. If it continues, contact the council.";
            context.Result = result;
        }
    }
    public class ActiveMemberAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            if (session == null || session["MemberID"] == null) return;
            using (var db = new CommunitySportsFacilitiesBookingDBEntities())
            {
                var member = db.Members.Find(Convert.ToInt32(session["MemberID"]));
                if (member == null || !member.IsActive) {
                    session.Clear();
                    context.Result = new RedirectResult("~/Account/Login");
                    return;
                }
                session["UserRole"] = member.UserRole;
            }
            context.HttpContext.Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            context.HttpContext.Response.Cache.SetNoStore();
        }
    }
}

