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
    public abstract class JSONHandler : EndpointHandler
    {
        public JSONHandler() {
            
        } 

        public override void ProcessHandler()
        {
            if (Query["callback"].NNOE())
            {
                contentType = Endpoint.ContentType.JSONP;
                context.Items["contentType"] = Endpoint.ContentType.JSONP;
            } 
        }

        public override void Dispose()
        {
            if (Query["callback"].NNOE())
            { 
                //this.data = Query["callback"].Else("jsonpcallback") + "(" + this.ToJSON() + ")";
            }

            base.Dispose();
             
        }

    }

    public class RoutePluginJSON : RoutePlugin
    { 

    }
}
