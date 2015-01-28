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

            if (TemplatePlugin.current == null)
                TemplatePlugin.current = new Template(Resources<Sharp.EndPoints.Error>.Read["handlers.Error.html"]);

            if (app.Context.AllErrors != null)
            {
                foreach (var err in app.Context.AllErrors)
                {
                    string error = err.Message;

                    if (context.IsDebuggingEnabled)
                        error = err.ToString();
                    else
                    {
                        if (err.InnerException != null)
                            error += err.InnerException.Message;
                    }

                    sb.ErrorMsg = ("error" + ": " + error);

                }

                TemplatePlugin.current.Set("errors", sb.ErrorMsg);
            }
        }

        public Error(object sender, EventArgs e) //: base(((Handler<NoSession>)HttpContext.Current.Handler).template.ToString())
        {
            HttpApplication app = ((HttpApplication)sender);

            sb.HTTPCode = 500;

            RenderError(app);

            this.DisposeHandlers.Add(x=> new TemplatePlugin().Dispose(x));

            ProcessRequest(context);
             
        } 
    }
}
