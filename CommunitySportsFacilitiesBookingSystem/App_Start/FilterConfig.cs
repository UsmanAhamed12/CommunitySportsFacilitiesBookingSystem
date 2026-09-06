using System.Web;
using System.Web.Mvc;

namespace CommunitySportsFacilitiesBookingSystem
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new Filters.ApplicationExceptionAttribute());
            filters.Add(new Filters.ActiveMemberAttribute());
        }
    }
}
