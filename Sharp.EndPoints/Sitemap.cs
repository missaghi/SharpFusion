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

            foreach (Route route in RouteTable.Routes)
            {
                template.Append("links", "<li><a href='/meta?url=" + route.Url.ToURL() + "'>" + route.Url + "</a>" + route.ToJSON() + "</li>");
            }



        }

    }
}
