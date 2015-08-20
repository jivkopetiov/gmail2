using Gmail2.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Gmail2
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                dropdown.DataSource = Database.GetAvailableUsers();
                dropdown.DataBind();
            }
        }

        protected void dropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            string person = dropdown.SelectedItem.Text;
            Database.GenerateRandomData(person);
            Response.Redirect("~/");
        }
    }
}