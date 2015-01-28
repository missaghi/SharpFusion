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
using System.ComponentModel;
using System.IO;
using System.Dynamic;

namespace Sharp.EndPoints
{

    public enum ContentType { [Description("text/html")] DEFAULT, [Description("text/html")] HTML, [Description("application/json")] JSON, [Description("application/javascript")] JSONP,[Description("application/javascript")] JAVASCRIPT }


    [DataContract]
    public abstract class EndpointHandler : Web, IDisposable, IHttpHandler
    {
        [DataMember]
        public object data { get; set; }

        public List<Action<EndpointHandler>> ProcessHandlers { get; set; }
        public List<Action<EndpointHandler>> DisposeHandlers { get; set; }
        //public Dictionary<String, Func<EndpointHandler, String>> ContentTypeHandlers { get; set; }

        private String path { get; set; }

        private String _RequestContent { get; set; }

        public String RequestContent
        {
            get
            {
                if (_RequestContent == null)
                {

                    using (Stream receiveStream = context.Request.InputStream)
                    {
                        using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                        {
                            _RequestContent = readStream.ReadToEnd();
                        }
                    }
                }

                return _RequestContent;

            }
        } 

        public string AcceptType { get; set; }

        private EndPointPlugin.HTTPVerb verb { get; set; }

        public MethodInfo method { get; set; }
        public object handlerChildInstance { get; set; } 

        public bool SerializeEntireHandler = true;   

        public bool IsReusable
        { get { return false; } }

        public EndpointHandler() {
            ProcessHandlers = new List<Action<EndpointHandler>>();
            DisposeHandlers = new List<Action<EndpointHandler>>();
            //ContentTypeHandlers = new Dictionary<string, Func<EndpointHandler, String>>();
        }

        public virtual void ProcessRequest(HttpContext context)
        { 
            SetHeaders();

            ProcessHandlers.ForEach(x => x(this)); 

            InvokeMethod();

            DisposeHandlers.ForEach(x => x(this));
 
            Dispose();   

            context.Response.End();
        } 
        

        public void SetHeaders()
        {
            context.Response.CacheControl = "no-cache";  

            try
            {
                verb = RouteDataToken["verb"].Else("ALL").ToEnum<EndPointPlugin.HTTPVerb>();   // ((Endpoint.HTTPVerb)(RouteTable.Routes.GetRouteData((HttpContextBase)new HttpContextWrapper(context)).Values["verb"] ?? Endpoint.HTTPVerb.ALL));
                
                if (context.Request.RequestType.Like("GET"))
                {
                    if (Query["callback"].NNOE())
                        AcceptType = ContentType.JSONP.Description();
                    else
                    {
                        if (context.Request.AcceptTypes != null && context.Request.AcceptTypes.Length > 0)
                            AcceptType = context.Request.AcceptTypes[0];
                        else
                            AcceptType = ContentType.DEFAULT.Description();
                    }
                }
                else
                {
                    AcceptType = context.Request.AcceptTypes[0]; 
                }
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

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                var methodParameters = method.GetParameters();
                {  
                    //Spin through all of the parameters on the method and see if you can find matches in the request
                    foreach (var methodParam in methodParameters)
                    {
                        //check the post values, query values, route values
                        string value = Form[methodParam.Name].Else(Query[methodParam.Name]).Else(RouteDataToken[methodParam.Name]);

                        //if it's post and there is only one value, try and deseriazlise the whole request content
                        if (value.NOE())
                        {
                            if (context.Request.HttpMethod.Like("POST") && methodParameters.Count() == 1)
                            {
                                value = RequestContent;
                            } 
                        }

                        if (value.NNOE())
                        {
                            try
                            {
                                var newparm = typeof(JSONHelper).GetMethod("ParseJSON").MakeGenericMethod(new Type[] { methodParam.ParameterType }).Invoke(this, new Object[] { value });
                                parameters.Add(methodParam.Name, newparm);
                            }
                            catch (Exception e)
                            {
                                throw new Exception(methodParam.Name + " is not the right format:" + e.InnerException.Message);
                            }

                        }  
                        else if (methodParam.IsOptional)
                        { //add default value
                            parameters.Add(methodParam.Name, methodParam.DefaultValue);
                        }
                    }
                }

                //if the number of parameters built up matches the number of parameters in the request then we got a match
                if (parameters.Count == methodParameters.Length)
                {
                    //Store output of action "method"
                    var tempdata = method.Invoke(handlerChildInstance, parameters.Count == 0 ? new object[] { } : parameters.Select(x => x.Value).ToArray());
                    if (data == null && tempdata != null)
                        this.data = tempdata;
                }
                else //thow some errors
                {
                    var missingparams = methodParameters.Where(x => !x.Name.LikeOne(parameters.Select(y => y.Key).ToArray())).Select(x => x.ParameterType.ToString() + " " + x.Name).ToArray();
                    sb.Required = methodParameters.Where(x => !x.Name.LikeOne(parameters.Select(y => y.Key).ToArray())).Select(x => x.Name).ToArray();
                    if (AcceptType == ContentType.JSON.Description() || AcceptType == ContentType.JSONP.Description())
                    {
                        sb.ErrorMsg = "Missing " + (methodParameters.Length - parameters.Count) + " required fields " + (context.IsDebuggingEnabled ? String.Join(", ", sb.Required) : "");
                    }
                    else
                    {
                        throw new Exception("Missing " + (methodParameters.Length - parameters.Count) + " parameters needed to invoke this method via " + AcceptType + ": \n\n" + String.Join("\n", missingparams) + "\n\n");
                    }
                }
            }
        } 

        public virtual void Dispose()
        {
            context.Response.Expires = 0;

            if (context.Request.AcceptTypes == null) {
                Write(this.data != null || SerializeEntireHandler ? this.ToJSON() : this.data.ToJSON());
            } 
            else if (context.Request.AcceptTypes.Any(x=> x.Like(ContentType.JSON.Description())))
            {
                Write(this.data != null || SerializeEntireHandler ? this.ToJSON() : this.data.ToJSON());
            }
            else if (context.Request.AcceptTypes.Any(x => x.Like(ContentType.JSONP.Description())) || (context.Request.HttpMethod.Like("GET") && Query["callback"].NNOE()))
            {
                this.data = Query["callback"].Else("jsonpcallback") + "(" + this.ToJSON() + ")";
                Write(this.data);
            }
            else if (context.Request.AcceptTypes.Any(x => x.Like(ContentType.HTML.Description())))
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

            if (sb.Tn != null)
                sb.Tn.Dispose(); 

        }
    }
}