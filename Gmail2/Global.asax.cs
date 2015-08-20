using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Gmail2.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            foreach (string route in new[] { "inbox", "starred", "sent", "draft", "unread", "allmail", "spam", "recyclebin", "post/{id}", "tags/{tagid}", "compose", "search/{query}" })
            {
                RouteTable.Routes.MapPageRoute(route, route, "~/Default.aspx", false);
            }
        }
    }
}