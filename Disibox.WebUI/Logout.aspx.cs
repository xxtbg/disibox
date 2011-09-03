using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Disibox.Data.Client;

namespace Disibox.WebUI.Account
{
    public partial class Logout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e) {
            var dataSource = (ClientDataSource) Session["ClientDataSource"];

            if (dataSource!=null) {
                dataSource.Logout();
                Session.Abandon();
            }

            //Response.Redirect("../Default.aspx");
        }
    }
}