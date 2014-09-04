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
    public class RoutePluginSharedTemplate : RoutePlugin
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
                ((TemplateHandler)handler).template = new Template(new LocalFile(tfile.Filename), new TenantRepo());
            }
        } 
         
    }
}
