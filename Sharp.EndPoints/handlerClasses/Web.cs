using System;
using System.Web;
using System.Collections; 
using System.Runtime.Serialization;


namespace Sharp
{
    [DataContract]
    public class Web
    {
        [IgnoreDataMember]
        public HttpContext context { get; set; }
        public FormWrapper Form { get { return new FormWrapper() { context = context }; } }
        public FormArrayWrapper FormArray { get { return new FormArrayWrapper() { context = context }; } }
        public QueryStringWrapper Query { get { return new QueryStringWrapper() { context = context }; } }
        public RouteDataWrapper RouteDataToken { get { return new RouteDataWrapper() { context = context }; } }
        public RouteValuesWrapper RouteValue { get { return new RouteValuesWrapper() { context = context }; } }

        [DataMember]
        public static StateBag sb
        {
            get
            { 
                return StateBag.Current;
            }
            set
            {
                StateBag.Current = value;
            }
        }
        public String[] UrlParts { get { return context.Request.UrlParts(); } }
        public Boolean isPost { get { return context.Request.RequestType.Like("POST"); } }



        public Web() : this(HttpContext.Current) {  }

        public Web(HttpContext _context)
        {
            context = _context;  
        }

        public class FormArrayWrapper
        {
            private HttpContext _context { get { return context ?? HttpContext.Current; } }
            public HttpContext context { get; set; }

            public String[] this[String index]
            {
                get
                {
                    if (_context.Request.Form[index] != null)
                        return _context.Request.Form.GetValues(index);
                    else
                        return new string[] { };
                }

            }
        }
        public class FormWrapper
        {
            private HttpContext _context { get { return context ?? HttpContext.Current; } }
            public HttpContext context { get; set; }

            public String this[String index]
            {
                get { return _context.Request.Form[index] ?? ""; }

            }
        }
        public class QueryStringWrapper
        {
            private HttpContext _context { get { return context ?? HttpContext.Current; } }
            public HttpContext context { get; set; }

            public String this[String index]
            {
                get { return _context.Request.QueryString[index] ?? ""; }

            }
        } 

        public class RouteDataWrapper
        {
            private HttpContext _context { get { return context ?? HttpContext.Current; } }
            public HttpContext context { get; set; }

            public String this[String index]
            {
                get
                {
                    return _context.Request.RequestContext.RouteData.DataTokens[index] != null ? _context.Request.RequestContext.RouteData.DataTokens[index].ToString() : "";
                }

            }
        }

        public class RouteValuesWrapper
        {
            private HttpContext _context { get { return context ?? HttpContext.Current; } }
            public HttpContext context { get; set; }

            public String this[String index]
            {
                get
                {
                    return _context.Request.RequestContext.RouteData.Values[index] != null ? _context.Request.RequestContext.RouteData.Values[index].ToString() : "";
                }

            }
        }

       


        public void Write(object Txt)
        {
            if (Txt != null)
                context.Response.Write(Txt.ToString());

            //try { if (context.Session["count"] != null)
            //context.Response.Write(context.Session["count"]);
            //} catch (Exception e) {}
        }



    }
}