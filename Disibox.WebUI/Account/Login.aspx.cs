using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Disibox.Data.Client;
using Disibox.Data.Client.Exceptions;
using Disibox.Data.Exceptions;


namespace Disibox.WebUI.Account
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Login_click(object sender, EventArgs e) {
            var dataSource = new ClientDataSource();

            var tmp = "";
            try {
                dataSource.Login(LoginUser.UserName, LoginUser.Password);
            } catch (ArgumentNullException) {
                tmp = "Username and/or password are null, please retry!";
            } catch (InvalidEmailException) {
                tmp = "Email is not valid, please retry!";                
            } catch (InvalidPasswordException) {
                tmp = "Password is not valid, please retry!";                
            } catch (UserNotExistingException) {
                tmp = "User with this credentials does not exists, please retry!";                
            } finally {
                LoginUser.FailureText = tmp;
            }

            if (LoginUser.FailureText != "")
                return;
            Session["ClientDataSource"] = dataSource;
            Session["UserEmail"] = LoginUser.UserName;
            Response.Redirect("../MemberOnly.aspx"); 
        }
    }
}
