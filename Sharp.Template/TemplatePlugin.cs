using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp.EndPoints;

namespace Sharp.Template
{
    public class TemplatePlugin : IRoutePlugin
    {
        public void AddValuesAndConstraints(RouteValueDictionary values, RouteValueDictionary constraints, object type)
        {
            TemplateFile file;
            Object[] AttributesArray;
            AttributesArray = ((MethodInfo)type).GetCustomAttributes(typeof(TemplateFile), true);
            if (AttributesArray.Length > 0)
            {
                file = (TemplateFile)AttributesArray[0];
                values.Add("templatefile", file.Filename);
            }
        }

        public void OnInit(RequestContext requestContext)
        {
            if (requestContext.RouteData.Values["templatefile"] != null)
            {
                var file = requestContext.RouteData.Values["templatefile"].ToString();
                HttpContext.Current.Items.Add("template", new Template(new LocalFile(file)));
            }
        }
    }
}
