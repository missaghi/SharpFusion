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
    public interface IEndPointAttribute
    {
        void AddRouteProperties(RouteData routeData, MethodInfo methodInfo);
        void PreInitAction(RequestContext requestContext);
        void ProcessHandler(EndpointHandler endPointHandler);
        void Dispose(EndpointHandler endPointHandler);
    }

    public class RouteData
    { 
        public RouteValueDictionary dataTokens { get; set; }
        public RouteValueDictionary constraints { get; set; }
        public RouteValueDictionary values { get; set; }
        public string url { get; set; }

        public RouteData()
        {
            dataTokens = new RouteValueDictionary();
            constraints = new RouteValueDictionary();
            values = new RouteValueDictionary();
        }

        public void AddRoute(Sharp.EndPoints.RouteParser.MethodEndpoint routehandler)
        {
            System.Web.Routing.RouteTable.Routes.Add(new System.Web.Routing.Route(url, values, constraints, dataTokens, routehandler));
        } 
    }

    public class RouteParser : TypeParser
    { 
        public MethodEndpoint routehandler { get; set; } 

        public override void ParseType(Type _type)
        {
            type = _type;
            if (typeof(IHttpHandler).IsAssignableFrom(type))
            {
                // Method endpoint
                foreach (var method in type.GetMethods())
                    if (method.GetCustomAttributes(typeof(EndPointPlugin), false).Length == 1) //must have an endpoint attribute
                    { 

                        Type methodEndpointType = (typeof(MethodEndpoint<>).MakeGenericType(new Type[] { type }));
                        routehandler = (MethodEndpoint)methodEndpointType.GetInstance();
                        
                        routehandler.methodInfo = method;

                        var routeData = new RouteData(); 

                        var endPointAttributes = GetEndPointAttributes(method);

                        endPointAttributes.ForEach(x => x.AddRouteProperties(routeData, method));

                        routehandler.PreInitHandlers.AddRange(endPointAttributes.Select(x => new Action<RequestContext>(x.PreInitAction)));
                        routehandler.DisposeHandlers.AddRange(endPointAttributes.Select(x => new Action<EndpointHandler>(x.Dispose)));
                        routehandler.ProcessHandlers.AddRange(endPointAttributes.Select(x => new Action<EndpointHandler>(x.ProcessHandler)));

                        routeData.AddRoute(routehandler);
                    }
            }
        }

        private static List<IEndPointAttribute> GetEndPointAttributes(MethodInfo method)
        {
            var endPointAttributes = method.GetCustomAttributes(false)
            .Where(x => typeof(IEndPointAttribute).IsAssignableFrom(x.GetType()))
            .Select(x => (IEndPointAttribute)x).ToList();
            return endPointAttributes;
        } 
       

        public abstract class MethodEndpoint: System.Web.Routing.IRouteHandler {
            public List<Action<RequestContext>> PreInitHandlers { get; set; }
            public List<Action<EndpointHandler>> ProcessHandlers { get; set; }
            public List<Action<EndpointHandler>> DisposeHandlers { get; set; }
            public MethodInfo methodInfo { get; set; }

            public MethodEndpoint() {
                PreInitHandlers = new List<Action<RequestContext>>();
                ProcessHandlers = new List<Action<EndpointHandler>>();
                DisposeHandlers = new List<Action<EndpointHandler>>();
            }

            public virtual IHttpHandler GetHttpHandler(RequestContext requestContext)
            {
                throw new NotImplementedException();
            }
        }

        public class MethodEndpoint<T> : MethodEndpoint where T : EndpointHandler, new()
        {
            public override IHttpHandler GetHttpHandler(RequestContext requestContext)
            {
                 
                PreInitHandlers.ForEach(x => x(requestContext)); 

                T t = new T(); 

                t.method = methodInfo;
                t.handlerChildInstance = t;

                t.ProcessHandlers.AddRange(ProcessHandlers);
                t.DisposeHandlers.AddRange(DisposeHandlers); 

                return (IHttpHandler)(object)t;
            } 

        }

    } 
}
