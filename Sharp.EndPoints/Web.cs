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
        public RouteDataWrapper RouteDataValue { get { return new RouteDataWrapper() { context = context }; } }

        [DataMember]
        public StateBag sb
        {
            get
            {
                IDictionary items = HttpContext.Current.Items;
                if (!items.Contains("StateBag"))
                {
                    items["StateBag"] = new StateBag();
                }
                return (StateBag)items["StateBag"];
            }
            set
            {
                IDictionary items = HttpContext.Current.Items;
                if (!items.Contains("StateBag"))
                {
                    items["StateBag"] = new StateBag();
                }
                items["StateBag"] = value;
            }
        }
        public String[] UrlParts { get { return context.Request.UrlParts(); } }
        public Boolean isPost { get { return context.Request.RequestType.Like("POST"); } }



        public Web()
        {

            context = HttpContext.Current; 
            sb = new StateBag();
        }

        public Web(HttpContext _context)
        {
            context = _context; 
            sb = new StateBag();
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