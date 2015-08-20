using Gmail2.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Gmail2
{
    public partial class Default : System.Web.UI.Page
    {
        protected string currentUserString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Database.IsLoggedIn())
            {
                Response.Redirect("~/Login.aspx");
            }

            var user = Database.GetCurrentUser();
            currentUserString = new JavaScriptSerializer().Serialize(user);
        }
    }
}