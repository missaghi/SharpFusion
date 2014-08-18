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
    public abstract class Handler : Web, IDisposable, IHttpHandler
    {
        [DataMember]
        public object data { get; set; }
          
        private String path { get; set; }
        protected Endpoint.ContentType contentType { get; set; }
        private Endpoint.HTTPVerb verb { get; set; }

        public MethodInfo method { get; set; }
        public object handlerChildInstance { get; set; } 

        public bool SerializeEntireHandler = true;  

        public Handler()
        { 
            contentType = (context.Request.AcceptTypes ?? (new string[] { })).Count(x => (x ?? "").LikeOne(new string[] { "application/json", "text/javascript" })) > 0 ? Endpoint.ContentType.JSON : Endpoint.ContentType.HTML;  // ((HttpHandler.ContentType)(RouteTable.Routes.GetRouteData((HttpContextBase)new HttpContextWrapper(context)).Values["content-type"] ?? HttpHandler.ContentType.HTML));

            try
            {
                verb = ((Endpoint.HTTPVerb)(RouteTable.Routes.GetRouteData((HttpContextBase)new HttpContextWrapper(context)).Values["verb"] ?? Endpoint.HTTPVerb.ALL));
                //responseType = ((Endpoint.ContentType)(RouteTable.Routes.GetRouteData((HttpContextBase)new HttpContextWrapper(context)).Values["content-type"] ?? Endpoint.ContentType.HTML));
            }
            catch (Exception e)
            {
                throw new Exception("Handler can't get route data.");
            }

            //if (responseType == Endpoint.ContentType.JSON) contentType = Endpoint.ContentType.JSON;
            SetContentType(); 
        }   

        private void SetContentType()
        {
            context.Response.CacheControl = "no-cache";

            if (contentType == Endpoint.ContentType.JSON)
                context.Response.ContentType = "application/json";
            else if (contentType == Endpoint.ContentType.HTML)
                context.Response.ContentType = "text/html";
            else
                throw new Exception(string.Format("content type {0} not implemented", this.contentType.ToStringValue()));
        } 
         
       

        public bool IsReusable
        { get { return false; } }

        public virtual void ProcessRequest(HttpContext context)
        {   
            if (method != null && this.handlerChildInstance != null && context.AllErrors == null)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                var methodParameters = method.GetParameters();

                {
                    foreach (var methodParam in methodParameters)
                    {
                        string value = Form[methodParam.Name].Else(Query[methodParam.Name]).Else(RouteDataValue[methodParam.Name]);
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
           

            ProcessHandler();

            Dispose();

        }  


        public abstract void ProcessHandler();

        public virtual void Dispose()
        {

            context.Response.Expires = 0;
            {
                if (contentType == Endpoint.ContentType.JSON)
                {
                    Write(this.data != null || SerializeEntireHandler ? this.ToJSON() : this.data.ToJSON());
                }
                if (contentType == Endpoint.ContentType.JSONP)
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