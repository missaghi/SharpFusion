using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Web;
using System.Web.Routing;
using System.Runtime.Serialization;

namespace Sharp.EndPoints
{ 

    public abstract class RoutePlugin
    {
        public virtual void AddRouteProperties(RouteParser routeParser, MethodInfo methodInfo) { }
        public virtual void PreHandlerInit(RequestContext requestContext) { }
        public virtual void PostHandlerInit(RequestContext requestContext, Handler handler) { }
    } 

    public class RouteParser : TypeParser
    {
        public RouteValueDictionary values { get; set; }
        public RouteValueDictionary constraints { get; set; }
        public string url { get; set; }
        public Type type { get; set; }
        public MethodEndpoint routehandler { get; set; }
        protected List<RoutePlugin> plugins { get; set; }

        public RouteParser(params RoutePlugin[] _plugins)
        {
            plugins = _plugins.ToList();
            plugins.Add(new RoutePluginHandler());
        }

        public void ParseType(Type _type)
        {
            type = _type;
            if (typeof(IHttpHandler).IsAssignableFrom(type))
            {
                // Method endpoint
                foreach (var method in type.GetMethods())
                    if (method.GetCustomAttributes(typeof(Endpoint), false).Length == 1)
                    {
                        Type methodEndpointType = (typeof(MethodEndpoint<>).MakeGenericType(new Type[] { type }));
                        routehandler = Instantiator.NewUpType<MethodEndpoint>(methodEndpointType); //routehandler =  (MethodEndpoint)Activator.CreateInstance(methodEndpointType);
                        routehandler.methodInfo = method;
                        values = new RouteValueDictionary();
                        constraints = new RouteValueDictionary();

                        foreach (RoutePlugin plugin in plugins)
                        {
                            plugin.AddRouteProperties(this, method);
                            routehandler.PreInitActions.Add(plugin.PreHandlerInit);
                            routehandler.PostInitActions.Add(plugin.PostHandlerInit);
                        }  

                        AddRoute();
                    }
            }
        } 
         

        private void AddRoute()
        { 
            System.Web.Routing.RouteTable.Routes.Add(new System.Web.Routing.Route(url, null, constraints, values, routehandler)); 
        }

        public abstract class MethodEndpoint: System.Web.Routing.IRouteHandler {
            public List<Action<RequestContext>> PreInitActions { get; set; }
            public List<Action<RequestContext, Handler>> PostInitActions { get; set; }
            public MethodInfo methodInfo { get; set; }

            public MethodEndpoint()
            {
                PreInitActions = new List<Action<RequestContext>>();
                PostInitActions = new List<Action<RequestContext, Handler>>();
            }

            public virtual IHttpHandler GetHttpHandler(RequestContext requestContext)
            {
                throw new NotImplementedException();
            }
        }

        [DataContract]
        public class MethodEndpoint<T> : MethodEndpoint where T : Handler, new()
        {
            public override IHttpHandler GetHttpHandler(RequestContext requestContext)
            {
                foreach (var action in PreInitActions)
                    action(requestContext);

                T t = new T();

                t.method = methodInfo;
                t.handlerChildInstance = t;

                foreach (var action in PostInitActions)
                    action(requestContext, t);

                return (IHttpHandler)(object)t;
            }
        }

    } 
}
