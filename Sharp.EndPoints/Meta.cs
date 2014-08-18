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
    public class Meta : TemplateHandler
    {
        [Endpoint("meta")]
        public void TestMethod(string url) {
            template = new Template(Resources<Sitemap>.Read["Meta.html"]);
            template.Set("url", url);

            Endpoint endpoint = FindEndpoint(url);
            if (endpoint != null)
            {
                RenderTestForm(endpoint);
            } 
        }

        private Type handlerType { get; set; }
        private MethodInfo methodInfo { get; set; }

        public void RenderTestForm(Endpoint endpoint)
        {
            this.contentType = Endpoint.ContentType.HTML;
            //gen test form
            StringBuilder sb = new StringBuilder();
            template = new Template(Resources<Meta>.Read["api.html"]);

            String Field = @"<label>{0}</label>{1}<br /><input name=""{0}"" placeholder=""{2}"" value=""[%{0}%]"" /><br />";

            template.Set("RequestType", endpoint.Verb.ToString());


            if (methodInfo != null)
            {
                foreach (var parms in methodInfo.GetParameters())
                {
                    sb.AppendFormat(Field, parms.Name, parms.IsOptional ? " - Optional, default (" + parms.DefaultValue.ToString() + ")" : "", parms.DefaultValue ?? "");
                }
                template.ReplaceTag("fields", sb.ToString());
            }
            else
            {  
                foreach (Object attr in handlerType.GetCustomAttributes(typeof(Endpoint), false))
                {
                    //Gen Form Fields
                    if (((Endpoint)attr).Parameters != null)
                    {
                        foreach (String str in ((Endpoint)attr).Parameters)
                        {
                            sb.AppendFormat(Field, str);
                        }
                        template.Set("fields", sb.ToString());
                    }
                }
            }
        }

        private Endpoint FindEndpoint(string url)
        {
            foreach (Assembly assembly in Sharp.SharpHost.Instance.Assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (typeof(IHttpHandler).IsAssignableFrom(type))
                    {
                        // Method endpoint
                        foreach (var method in type.GetMethods())
                        {
                            if (method.GetCustomAttributes(typeof(Endpoint), false).Length == 1)
                            {
                                Endpoint endpoint = ((Endpoint)method.GetCustomAttributes(typeof(Endpoint), false)[0]);
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
