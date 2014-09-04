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

    public abstract class TemplateHandler : EndpointHandler
    {
        public Template template {  get;  set; }

        public override void ProcessHandler()
        {
            if (template != null)// Pump out template
            {
                sb.Reset(this.template);
                template.Set("currenttime", DateTime.Now.ToString("M/d - h:mmtt").ToLower());
                if (Query["Error"].NNOE())
                    template.Error = template.fileRepo.ReadFile("/Content/Shared/msg/" + Query["Error"].ToInt(0).ToString() + ".htm");
                //convert to int to prevent abuse  
                if (Query["Warning"].NNOE())
                    template.Warning = template.fileRepo.ReadFile("/Content/Shared/msg/" + Query["Warning"].ToInt(0).ToString() + ".htm");
                if (Query["Msg"].NNOE())
                    template.Msg = template.fileRepo.ReadFile("/Content/Shared/msg/" + Query["Msg"].ToInt(0).ToString() + ".htm");
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

    public class RoutePluginTemplate : RoutePlugin
    { 
        public override void AddRouteProperties(RouteParser routeBuilder, MethodInfo methodInfo)
        {
            Object[] AttributesArray = ((MethodInfo)methodInfo).GetCustomAttributes(typeof(TemplateFile), true);
            if (AttributesArray.Length > 0)
            {
                TemplateFile tfile = (TemplateFile)AttributesArray[0];
                routeBuilder.dataTokens.Add("templatefile", tfile); 
            }
        } 

        public override void PostHandlerInit(RequestContext requestContext, EndpointHandler handler)
        {  
            if (requestContext.RouteData.DataTokens["templatefile"] != null)
            {
                var tfile = (TemplateFile)requestContext.RouteData.DataTokens["templatefile"];
                ((TemplateHandler)handler).template = new Template(new LocalFile(tfile.Filename));
            }
        } 
         
    }
}
