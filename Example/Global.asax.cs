using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Sharp;
using Sharp.EndPoints;

namespace Example
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        { 
            var app = new AssemblyParser();

            app.AddAssemblyParsers(typeof(Sharp.EndPoints.Sitemap).Assembly);
            app.AddAssemblyParsers(typeof(Global).Assembly);
            
            app.Parse();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {
            //replace TemplatePlugin.current file with your own
            TemplatePlugin.current = new Template(Resources<Sharp.EndPoints.Error>.Read["Error.html"]);
            new Sharp.EndPoints.Error(sender, e);
        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}