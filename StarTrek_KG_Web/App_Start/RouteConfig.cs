using System.Web.Routing;
using Microsoft.AspNet.FriendlyUrls;

namespace StarTrek_KG_Web
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            var settings = new FriendlyUrlSettings
            {
                AutoRedirectMode = RedirectMode.Off
            };
            routes.EnableFriendlyUrls(settings);
        }
    }
}
