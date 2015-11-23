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
    public class Meta : EndpointHandler
    {
        [EndPointPlugin("meta")]
        [TemplatePlugin]
        public void TestMethod(string url) { 


            EndPointPlugin endpoint = FindEndpoint(url);
            if (endpoint != null)
            {
                RenderTestForm(endpoint);
            } 
        }

        private Type handlerType { get; set; }
        private MethodInfo methodInfo { get; set; }

        public void RenderTestForm(EndPointPlugin endpoint)
        {
            this.AcceptType = ContentType.HTML.Description();
            //gen test form
            StringBuilder sb = new StringBuilder();
            TemplatePlugin.current = new Template(Resources<Meta>.Read["handlers.Meta.html"]);

            String Field = @"<label>{0}</label>{1}<br /><input name=""{0}"" placeholder=""{2}"" value=""[%{0}%]"" /><br />";

            TemplatePlugin.current.Set("RequestType", endpoint.Verb.ToString()); 


            if (methodInfo != null)
            {
                foreach (var parms in methodInfo.GetParameters())
                {
                    sb.AppendFormat(Field, parms.Name, parms.IsOptional ? " - Optional, default (" + (parms.DefaultValue ?? "").ToString() + ")" : "", parms.DefaultValue ?? "");
                }
                TemplatePlugin.current.ReplaceTag("fields", sb.ToString());
            }
            else
            {  
                foreach (Object attr in handlerType.GetCustomAttributes(typeof(EndPointPlugin), false))
                {
                    //Gen Form Fields
                    if (((EndPointPlugin)attr).Parameters != null)
                    {
                        foreach (String str in ((EndPointPlugin)attr).Parameters)
                        {
                            sb.AppendFormat(Field, str);
                        }
                        TemplatePlugin.current.Set("fields", sb.ToString());
                    }
                }
            }
        }

        private EndPointPlugin FindEndpoint(string url)
        {
            foreach (Assembly assembly in SharpHost.Instance.Assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (typeof(IHttpHandler).IsAssignableFrom(type))
                    {
                        // Method endpoint
                        foreach (var method in type.GetMethods())
                        {
                            if (method.GetCustomAttributes(typeof(EndPointPlugin), false).Length == 1)
                            {
                                EndPointPlugin endpoint = ((EndPointPlugin)method.GetCustomAttributes(typeof(EndPointPlugin), false)[0]);
                                if (endpoint.Url.Like(url))
                                {
                                    handlerType = type;
                                    methodInfo = method;
                                    return endpoint;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        } 
    }
}
