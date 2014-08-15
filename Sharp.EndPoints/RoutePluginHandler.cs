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
    public class RoutePluginHandler : IRoutePlugin
    {

        public void AddValuesAndConstraints(RouteParser routeBuilder, MethodInfo methodInfo)
        {
            Endpoint attr;
            attr = (Endpoint)((MethodInfo)methodInfo).GetCustomAttributes(typeof(Endpoint), true)[0];
            routeBuilder.url = attr.Url;

            routeBuilder.values.Add("verb", attr.Verb);
            routeBuilder.values.Add("content-type", attr.Type);
            routeBuilder.values.Add("method", methodInfo);

            if (attr.Verb != Endpoint.HTTPVerb.ALL)
            {
                routeBuilder.constraints.Add("httpMethod", new HttpMethodConstraint(new string[] { attr.Verb.ToStringValue() }));
            } 
        }

        public void OnInit(RequestContext requestContext, object handler)
        { 
            HttpContext.Current.Items.Add("method", (MethodInfo)requestContext.RouteData.Values["method"]); 
        }



    }
}
