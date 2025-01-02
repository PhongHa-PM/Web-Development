using System.Web;
using System.Web.Mvc;

namespace QL_Cua_Hang_Chan_ga_Goi_Nem
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            
        }
    }
}