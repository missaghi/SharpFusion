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
    public class RoutePluginHandler : RoutePlugin
    {

        public override void AddRouteProperties(RouteParser routeBuilder, MethodInfo methodInfo)
        {
            Endpoint attr;
            attr = (Endpoint)((MethodInfo)methodInfo).GetCustomAttributes(typeof(Endpoint), true)[0];
            routeBuilder.url = attr.Url;
            routeBuilder.dataTokens.Add("verb", attr.Verb);
            routeBuilder.dataTokens.Add("methodInfo", methodInfo); 

            if (attr.Verb != Endpoint.HTTPVerb.ALL)
            {
                routeBuilder.constraints.Add("httpMethod", new HttpMethodConstraint(new string[] { attr.Verb.ToStringValue() }));
            } 
        } 
    }
}
