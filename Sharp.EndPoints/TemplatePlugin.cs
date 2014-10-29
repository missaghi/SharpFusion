using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Sharp.EndPoints
{ 

    public class TemplatePlugin : Attribute, IEndPointAttribute
    {
        public string FileName { get; set; }

        public TemplatePlugin(string fileName = null)
        {
            FileName = fileName;
        }

        public static Template current
        {
            get
            { 
                if (!HttpContext.Current.Items.Contains("Template"))
                {
                    return null; // throw new Exception("Must declare template file name"); 
                }
                return (Template)HttpContext.Current.Items["Template"];
            }
            set
            { 
                HttpContext.Current.Items["Template"] = value;
            }
        }
         

        public void ProcessHandler(EndpointHandler endpointHandler)
        {
            if (TemplatePlugin.current != null)
            {
                StateBag.Current.Reset(TemplatePlugin.current);
                TemplatePlugin.current.Set("currenttime", DateTime.Now.ToString("M/d - h:mmtt").ToLower());
                TemplatePlugin.current.Set("domain", endpointHandler.context.Request.Url.Host);
                TemplatePlugin.current.Set("like", endpointHandler.context.Request.Url.ToString().ToURL());
                try
                {
                    endpointHandler.context.Response.AppendHeader("ServeTime", (DateTime.Now.Subtract((DateTime)endpointHandler.context.Items["Time"])).TotalMilliseconds.ToString() + "ms");
                    endpointHandler.context.Response.AppendHeader("Queries", endpointHandler.context.Items["Queries"].ToString());
                }
                catch
                {
                }
            }
            
           

            //endpointHandler.ContentTypeHandlers.Add(ContentType.HTML.Description(), (EndpointHandler e) => { return e.data.ToString(); });

        }
         

        public void PreInitAction(System.Web.Routing.RequestContext requestContext)
        {
            if (FileName.NNOE())
                current = new Template(new LocalFile(FileName));
        }

        public void AddRouteProperties(RouteData routeData, System.Reflection.MethodInfo methodInfo)
        {
            //throw new NotImplementedException();
        }

        public void Dispose(EndpointHandler endpointHandler)
        {
            if (endpointHandler.AcceptType == ContentType.JSON.Description())
            {
                endpointHandler.data = TemplatePlugin.current.Tags;
            }
            else
                endpointHandler.data = TemplatePlugin.current.ToString();
        }
    } 

}
