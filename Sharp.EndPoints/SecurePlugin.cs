using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Sharp.EndPoints
{
    public class Secure : Attribute, IEndPointAttribute
    {
        public string[] Roles { get; set; }
        public Secure()
        {
            Roles = new string[] { };
        }
        public Secure(string Role)
        {
            Roles = new string[] { Role };
        }
        public Secure(string[] Roles)
        {
            this.Roles = Roles;
        } 

        public void PreInitAction(System.Web.Routing.RequestContext requestContext)
        {
            var context = HttpContext.Current; 
            if (!context.User.Identity.IsAuthenticated)
            {
                StateBag.Current.SecurityViolation = true;
                throw new Exception("this method requires authentication to run");
            }

            if (Roles.Length > 0)
            {
                if (!Roles.Any(x => context.User.IsInRole(x)))
                {
                    StateBag.Current.SecurityViolation = true;
                    throw new Exception("You must be a member of one of these roles to run this method: " + string.Join(",", Roles));
                }
            }
             
        }

        public void ProcessHandler(EndpointHandler endPointHandler)
        {
            //throw new NotImplementedException();
        }

        public void Dispose(EndpointHandler endPointHandler)
        {
            //throw new NotImplementedException();
        }

        public void AddRouteProperties(RouteData routeData, System.Reflection.MethodInfo methodInfo)
        {
            //throw new NotImplementedException();
        }
    }
}
