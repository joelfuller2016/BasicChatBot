using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CSharpAIAssistant.Web
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Check if user is authenticated and is an admin
            if (Context.User.Identity.IsAuthenticated)
            {
                // Check if user is admin by looking at session
                bool isAdmin = Session["IsAdmin"] != null && (bool)Session["IsAdmin"];
                
                // Show/hide admin dropdown based on admin status
                adminDropdown.Visible = isAdmin;
            }
            else
            {
                // Ensure admin dropdown is hidden for anonymous users
                adminDropdown.Visible = false;
            }
        }
    }
}
