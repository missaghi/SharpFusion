using System;
using System.Data;
using System.Reflection;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Web.Routing;
using System.Runtime.Serialization;
using Sharp;

namespace Sharp.EndPoints
{ 

    [DataContract]
    public abstract class EndpointHandler : Web, IDisposable, IHttpHandler
    {
        [DataMember]
        public object data { get; set; }
          
        private String path { get; set; }
        public Endpoint.ContentType contentType { get; set; }
        private Endpoint.HTTPVerb verb { get; set; }

        public MethodInfo method { get; set; }
        public object handlerChildInstance { get; set; } 

        public bool SerializeEntireHandler = true;  

        public bool IsReusable
        { get { return false; } }
        
        public abstract void ProcessHandler();

        public virtual void ProcessRequest(HttpContext context)
        {
            SetHeaders();
            
            InvokeMethod(); 

            ProcessHandler();

            Dispose();
        }

        public void SetHeaders()
        {
            context.Response.CacheControl = "no-cache";

            //if no content type is defined then pick one based on the request header: Accepted Type
            if (contentType == Endpoint.ContentType.DEFAUT)
                contentType = (context.Request.AcceptTypes ?? (new string[] { })).Count(x => (x ?? "").LikeOne(new string[] { "application/json", "text/javascript" })) > 0 ? Endpoint.ContentType.JSON : Endpoint.ContentType.HTML;  // ((HttpHandler.ContentType)(RouteTable.Routes.GetRouteData((HttpContextBase)new HttpContextWrapper(context)).Values["content-type"] ?? HttpHandler.ContentType.HTML));
            context.Response.ContentType = contentType.ToStringValue();


            try
            {
                verb = RouteDataToken["verb"].Else("ALL").ToEnum<Endpoint.HTTPVerb>();   // ((Endpoint.HTTPVerb)(RouteTable.Routes.GetRouteData((HttpContextBase)new HttpContextWrapper(context)).Values["verb"] ?? Endpoint.HTTPVerb.ALL));
            }
            catch (Exception e)
            {
                throw new Exception("Handler can't get route data:" + e.Message);
            }
        }

        private void InvokeMethod()
        {
            if (method != null && this.handlerChildInstance != null && context.AllErrors == null)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                var methodParameters = method.GetParameters();

                {
                    foreach (var methodParam in methodParameters)
                    {
                        string value = Form[methodParam.Name].Else(Query[methodParam.Name]).Else(RouteDataToken[methodParam.Name]);
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            try
                            {
                                var newparm = typeof(JSONHelper).GetMethod("ParseJSON").MakeGenericMethod(new Type[] { methodParam.ParameterType }).Invoke(this, new Object[] { value });
                                dict.Add(methodParam.Name, newparm);
                            }
                            catch (Exception e)
                            {
                                sb.ErrorMsg = methodParam.Name + " is not the right format";
                            }

                        }
                        else if (methodParam.IsOptional)
                        { //add default value
                            dict.Add(methodParam.Name, methodParam.DefaultValue);
                        }
                    }
                }

                if (dict.Count == methodParameters.Length)
                {
                    //Store output of action "method"
                    var tempdata = method.Invoke(handlerChildInstance, dict.Count == 0 ? new object[] { } : dict.Select(x => x.Value).ToArray());
                    if (data == null && tempdata != null)
                        this.data = tempdata;
                }
                else
                {
                    var missingparams = methodParameters.Where(x => !x.Name.LikeOne(dict.Select(y => y.Key).ToArray())).Select(x => x.ParameterType.ToString() + " " + x.Name).ToArray();
                    sb.Required = methodParameters.Where(x => !x.Name.LikeOne(dict.Select(y => y.Key).ToArray())).Select(x => x.Name).ToArray();
                    if (contentType == Endpoint.ContentType.JSON)
                    {
                        sb.ErrorMsg = "Missing " + (methodParameters.Length - dict.Count) + " required fields " + (context.IsDebuggingEnabled ? String.Join(", ", sb.Required) : "");
                    }
                    else
                    {
                        throw new Exception("Missing " + (methodParameters.Length - dict.Count) + " parameters needed to invoke this method: \n\n" + String.Join("\n", missingparams) + "\n\n");
                    }
                }
            }
        }  



        public virtual void Dispose()
        {
            context.Response.Expires = 0;
            {
                if (contentType == Endpoint.ContentType.JSON)
                {
                    Write(this.data != null || SerializeEntireHandler ? this.ToJSON() : this.data.ToJSON());
                }
                else if (contentType == Endpoint.ContentType.JSONP)
                {
                    Write(this.data);
                }
                else if (contentType == Endpoint.ContentType.HTML)
                {
                    if (this != null)
                    {
                        if (this.data == null)
                        {
                            foreach (string error in sb.Errors)
                                Write(error);
                        }
                        else if (this.data.GetType() == typeof(String))
                        {
                            Write(this.data);
                        }
                        else
                        {
                            Write(this.data == null || SerializeEntireHandler ? this.ToJSON() : this.data.ToJSON()); //if it's an object and not a string then serialize it even though it's requesting html
                        }
                    }
                }
                else
                { //serialize to XML ;
                    throw new Exception("content type not implemented");
                }
            }

            if (sb.Tn != null)
                sb.Tn.Dispose();
                 
                context.Response.End();

        }
    }
}