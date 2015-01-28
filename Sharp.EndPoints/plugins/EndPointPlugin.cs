using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;

namespace Sharp.EndPoints
{ 

    public class EndPointPlugin : Attribute, IEndPointAttribute
    {
        public string Url { get; set; }
        public enum HTTPVerb { POST, GET, DELETE, PUT,[Description("*")] ALL }
        public HTTPVerb Verb { get; set; }
        public String[] Parameters { get; set; }

        public EndPointPlugin(string URL, HTTPVerb verb = HTTPVerb.ALL)
        {
            Url = URL;
            Verb = verb; // HTTPVerb.ALL;
        }
         

        /// <summary>
        /// Define an end point
        /// </summary>
        /// <param name="URL">
        /// /folder
        /// </param>
        public EndPointPlugin(string URL)
        {
            Url = URL;
            Verb = HTTPVerb.ALL;
        } 

        public void AddRouteProperties(RouteData routeData, System.Reflection.MethodInfo methodInfo)
        {
            EndPointPlugin attr;
            attr = (EndPointPlugin)((MethodInfo)methodInfo).GetCustomAttributes(typeof(EndPointPlugin), true)[0];
            routeData.url = attr.Url;
            routeData.dataTokens.Add("verb", attr.Verb);
            routeData.dataTokens.Add("methodInfo", methodInfo);

            if (attr.Verb != EndPointPlugin.HTTPVerb.ALL)
            {
                routeData.constraints.Add("httpMethod", new HttpMethodConstraint(new string[] { attr.Verb.Description() }));
            }
        }
         

        public void PreInitAction(System.Web.Routing.RequestContext requestContext)
        {
            //throw new NotImplementedException();
        }

        public void ProcessHandler(EndpointHandler endPointHandler)
        {
            //throw new NotImplementedException();
        }

        public void Dispose(EndpointHandler endPointHandler)
        {
            //throw new NotImplementedException();
        }
    }
}
