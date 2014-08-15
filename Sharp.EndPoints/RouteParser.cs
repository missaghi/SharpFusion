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

    public interface IRoutePlugin
    {
        void AddValuesAndConstraints(RouteParser routeBuilder, MethodInfo methodInfo);
        void OnInit(RequestContext requestContext, object handler);
    } 

    public class RouteParser : TypeParser
    {
        public RouteValueDictionary values { get; set; }
        public RouteValueDictionary constraints { get; set; }
        public string url { get; set; }
        public Type type { get; set; }
        protected IMethodEndpoint routehandler { get; set; }
        protected List<IRoutePlugin> plugins { get; set; }

        public RouteParser(params IRoutePlugin[] _plugins)
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
                        routehandler = (IMethodEndpoint)Activator.CreateInstance(typeof(MethodEndpoint<>).MakeGenericType(new Type[] { type }));
                        values = new RouteValueDictionary();
                        constraints = new RouteValueDictionary();

                        foreach (var plugin in plugins)
                        {
                            plugin.AddValuesAndConstraints(this, method);
                            routehandler.AddInitAction(plugin.OnInit);
                        }

                        AddRoute();
                    }
            }
        } 

        private void AddRoute()
        { 
            System.Web.Routing.RouteTable.Routes.Add(new System.Web.Routing.Route(url, values, constraints, routehandler)); //new RouteValueDictionary() { { "httpMethod", new HttpMethodConstraint(new string[] { attr.Verb.ToStringValue() })} },
        }

        public interface IMethodEndpoint: System.Web.Routing.IRouteHandler {
            void AddInitAction(Action<RequestContext, object> action);
        }

        [DataContract]
        public class MethodEndpoint<T> : IMethodEndpoint where T : IHttpHandler, new()
        {
            public MethodEndpoint() {
                InitActions = new List<Action<RequestContext, object>>();
            }

            public void AddInitAction(Action<RequestContext, object> action) {
                InitActions.Add(action);
            }

            private List<Action<RequestContext, object>> InitActions { get; set; }

            public IHttpHandler GetHttpHandler(RequestContext requestContext)
            {
                foreach (var action in InitActions)
                {
                    action(requestContext, null);
                }

                T t = new T(); 

                //HttpContext.Current.Items.Add("httphandler", t);
                //HttpContext.Current.Items.Add("handler", typeof(T)); //this is only necessary for error handling, find a better way

                return (IHttpHandler)(object)t;
            }
        }

    } 
}
