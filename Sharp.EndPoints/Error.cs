using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Sharp.EndPoints
{
    public class Error : EndpointHandler
    {
        public Error() { }


        [EndPointPlugin("error")]
        [TemplatePlugin]
        public void ErrorPage()
        {
            RenderError(context.ApplicationInstance);
        }

        public void RenderError(HttpApplication app)
        {

            if (app.Context.AllErrors != null)
                foreach (var err in app.Context.AllErrors)
                {
                    if (context.IsDebuggingEnabled)
                        sb.ErrorMsg = ("error" + ": " + err);
                    else
                        sb.ErrorMsg = ("error" + ": " + err.Message);

                }
             
        }

        public Error(object sender, EventArgs e) //: base(((Handler<NoSession>)HttpContext.Current.Handler).template.ToString())
        {
            HttpApplication app = ((HttpApplication)sender);
            //if (app.Context.AllErrors[0].Message == "No http handler was found for request type 'GET'")
            //{
            //    //Some things used to require a project name, this adds it to the web.config.
            //    if (ConfigurationManager.AppSettings["ProjectName"] == null)
            //    {
            //        var config = WebConfigurationManager.OpenWebConfiguration("~");
            //        var section = (AppSettingsSection)config.GetSection("appSettings");
            //        section.Settings.Add("ProjectName", "Sharp");
            //        config.Save();
            //    }
            //    app.Context.Response.Redirect("/sitemap");
            //}
            //else

            RenderError(app);

            //app.Context.Response.End();
            //ProcessRequest(context);
            ProcessHandler();
            //data = template.ToString();
            Dispose();
        }


        public void ProcessHandler()
        {
            SetHeaders();


            if (AcceptType == ContentType.HTML.Description() || AcceptType == ContentType.DEFAULT.Description())
            {
                if (TemplatePlugin.current == null)
                    TemplatePlugin.current = new Template(Resources<Error>.Read["Error.html"]);

                TemplatePlugin.current.Set("errors", sb.ErrorMsg);
            }

        }
    }
}
