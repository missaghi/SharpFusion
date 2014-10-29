using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using System.Xml;
using JimBlackler.DocsByReflection;

namespace Sharp.EndPoints
{
    /// <summary>
    /// Displays all of the runtime registered endpoints
    /// </summary>
    public class Sitemap : EndpointHandler
    {

        public Sitemap() { }

        [EndPointPlugin("sitemap")]
        [TemplatePlugin]
        public void RenderSitemap() {
            TemplatePlugin.current = new Template(Resources<Sitemap>.Read["Sitemap.html"]);

            foreach (var routeGroup in RouteTable.Routes.GroupBy(x =>  ((MethodInfo)((Route)x).DataTokens["methodInfo"]).DeclaringType.ToString()))
            {
                TemplatePlugin.current.Append("links", "<h3 style=\"margin-bottom:0; background:#eee; padding-bottom:0; padding-left:10px; \">" + routeGroup.Key + "</h3>");

                try
                {
                    XmlElement documentation = DocsByReflection.XMLFromType(((MethodInfo)((Route)routeGroup.FirstOrDefault()).DataTokens["methodInfo"]).DeclaringType);
                    TemplatePlugin.current.Append("links", " " + documentation["summary"] == null ? "" : documentation["summary"].InnerText.Trim() + "<br><br>");

                }
                catch (Exception e) {
                    TemplatePlugin.current.Append("links", "<br>");
                } 


                foreach (Route route in routeGroup)
                {
                    var methodInfo = ((MethodInfo)route.DataTokens["methodInfo"]);
                    string summary = "";
                    try
                    {
                        XmlElement documentation = DocsByReflection.XMLFromMember(((MethodInfo)route.DataTokens["methodInfo"]));
                        summary = documentation["summary"] == null ? "" : documentation["summary"].InnerText.Trim();
                    }
                    catch (Exception e)
                    {
                        //summary = "no description found: " + e.Message;
                    } 

                    TemplatePlugin.current.Append("links", "<div style=\"padding-left:10px; margin-left:10px; border-left:5px solid #ccc;\"><a href='/meta?url=" + route.Url.ToURL().Trim().Else("/") + "'>" + route.Url.Trim().Else("/") + "</a>\t\t" +
                        "<b>" + methodInfo.Name + "</b>(" +
                        string.Join(",", methodInfo.GetParameters().Select(x => "<i>" + x.ParameterType.ToString() + "</i> <b>" + x.Name + "</b>" + (x.IsOptional ? " = " + x.DefaultValue : "")).ToArray())
                        + ") returns <i>" + methodInfo.ReturnType.ToString() + "</i><p style=\"color:#999; font-size:smaller; margin-top:0;\">"+ summary +"</p></div>");
                }
            }



        }

    }
}
