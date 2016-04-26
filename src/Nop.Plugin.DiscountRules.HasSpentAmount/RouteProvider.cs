using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.DiscountRules.HasSpentAmount
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.DiscountRules.HasSpentAmount.Configure",
                 "Plugins/DiscountRulesHasSpentAmount/Configure",
                 new { controller = "DiscountRulesHasSpentAmount", action = "Configure" },
                 new[] { "Nop.Plugin.DiscountRules.HasSpentAmount.Controllers" }
            );
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
