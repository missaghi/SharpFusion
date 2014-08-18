using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;

namespace Sharp.EndPoints
{
    public class Sitemap : TemplateHandler
    {

        public Sitemap() { }

        [Endpoint("sitemap")]
        public void RenderSitemap() {
            template = new Template(Resources<Sitemap>.Read["Sitemap.html"]);

            foreach (var route in RouteTable.Routes)
            {
                dynamic expando = JSONHelper.ParseJSON<dynamic>(route.ToJSON());
                template.Append("links", "<li><a href='/meta?url=" + ((string)expando.Url).ToURL() + "'>" + expando.Url + "</a>" + route.ToJSON() + "</li>");
            }



        }

    }
}
