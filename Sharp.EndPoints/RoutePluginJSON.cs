using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;

namespace Sharp.EndPoints
{
    [DataContract]
    public abstract class JSONHandler : Handler
    { 
        public override void ProcessHandler()
        {
             
        }

        public override void Dispose()
        {
            if (Query["callback"] != null)
            {
                this.contentType = Endpoint.ContentType.JSONP;
                this.data = Query["callback"] + "(" + this.data.ToJSON() + ")";
            }

            base.Dispose();
        }

    }

    public class RoutePluginJSON : IRoutePlugin
    {
        public void AddValuesAndConstraints(RouteParser routeBuilder, MethodInfo methodInfo)
        { 
        }

        public void OnInit(RequestContext requestContext, object handler)
        {
            
        }
    }
}
