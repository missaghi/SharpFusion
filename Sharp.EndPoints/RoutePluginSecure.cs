﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;

namespace Sharp.EndPoints
{  
    public class RoutePluginSecure : IRoutePlugin
    {
        public void AddValuesAndConstraints(RouteParser routeBuilder, MethodInfo methodInfo)
        {
            Object[] AttributesArray = ((MethodInfo)methodInfo).GetCustomAttributes(typeof(Secure), true);
            if (AttributesArray.Length > 0)
            {
                Secure secure = (Secure)AttributesArray[0];
                routeBuilder.values.Add("SecureArea", secure); 
            }
             
        }

        public void OnInit(RequestContext requestContext, object handler)
        {
            var context = HttpContext.Current;
            Secure secure = (Secure)requestContext.RouteData.Values["SecureArea"];

            if (secure != null)
            {  
                if (!context.User.Identity.IsAuthenticated)
                {
                    if (secure.Roles.Length > 0)
                    {
                        throw new Exception("You must be a member of one of these roles to run this method: " + string.Join(",", secure.Roles));
                    }
                    else
                    {
                        throw new Exception("this method requires authentication to run");
                    }
                }

            }

        }
    }
}