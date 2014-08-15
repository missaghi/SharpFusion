using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing; 

namespace Sharp.EndPoints
{

    public abstract class TemplateHandler : Handler
    {
        public Template template
        {
            get
            {
                return (Template)context.Items["template"];
            }
            set { context.Items["template"] = value; }
        }

        public override void ProcessHandler()
        {
            if (template != null)// Pump out template
            {
                sb.Reset(this.template);
                template.Set("currenttime", DateTime.Now.ToString("M/d - h:mmtt").ToLower());
                if (Query["Error"].NNOE())
                    template.Error = Cached.GetFile("/a/msg/" + Query["Error"].ToInt(0).ToString() + ".htm");
                //convert to int to prevent abuse  
                if (Query["Warning"].NNOE())
                    template.Warning = Cached.GetFile("/a/msg/" + Query["Warning"].ToInt(0).ToString() + ".htm");
                if (Query["Msg"].NNOE())
                    template.Msg = Cached.GetFile("/a/msg/" + Query["Msg"].ToInt(0).ToString() + ".htm");
                template.Set("domain", context.Request.Url.Host);
                template.Set("like", context.Request.Url.ToString().ToURL());
                try
                {
                    context.Response.AppendHeader("ServeTime", (DateTime.Now.Subtract((DateTime)context.Items["Time"])).TotalMilliseconds.ToString() + "ms");
                    context.Response.AppendHeader("Queries", context.Items["Queries"].ToString());
                }
                catch
                {
                }
                this.data = template.ToString();
            }
        }

        public override void Dispose()
        {
            if (contentType == Endpoint.ContentType.JSON)
            {
                this.data = template.Tags;
            }

            base.Dispose();
        }

    }

    public class RoutePluginTemplate : IRoutePlugin
    { 
        public void AddValuesAndConstraints(RouteParser routeBuilder, MethodInfo methodInfo)
        {
            Object[] AttributesArray = ((MethodInfo)methodInfo).GetCustomAttributes(typeof(TemplateFile), true);
            if (AttributesArray.Length > 0)
            {
                TemplateFile file = (TemplateFile)AttributesArray[0];
                routeBuilder.values.Add("templatefile", file.Filename);
            }
        }

        public void OnInit(RequestContext requestContext, object handler)
        {
            if (requestContext.RouteData.Values["templatefile"] != null)
            {
                var file = requestContext.RouteData.Values["templatefile"].ToString();
                HttpContext.Current.Items.Add("template", new Template(new LocalFile(file)));
            }
        } 
    }
}
