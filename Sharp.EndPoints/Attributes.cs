using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Sharp.EndPoints
{
    public class Endpoint : Attribute
    {
        public string Url { get; set; }
        public enum ContentType { [Description("application/json")]JSON, [Description("application/javascript")] JSONP,[Description("text/html")] HTML, [Description("application/javascript")] JAVASCRIPT }
        public enum HTTPVerb { POST, GET, DELETE, PUT,[Description("*")] ALL }
        public ContentType Type { get; set; }
        public HTTPVerb Verb { get; set; }
        public String[] Parameters { get; set; }

        public Endpoint(string URL, ContentType type = ContentType.HTML, HTTPVerb verb = HTTPVerb.ALL)
        {
            Url = URL;
            Type = type;// ContentType.HTML;
            Verb = verb; // HTTPVerb.ALL;
        }

        public Endpoint(string URL, HTTPVerb verb = HTTPVerb.ALL)
        {
            Url = URL;
            Type = ContentType.HTML;
            Verb = verb; // HTTPVerb.ALL;
        }

        /// <summary>
        /// Define an end point
        /// </summary>
        /// <param name="URL">
        /// /folder
        /// </param>
        public Endpoint(string URL)
        {
            Url = URL;
            Type = ContentType.HTML;
            Verb = HTTPVerb.ALL;
        }
    }

    public class Secure : Attribute
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
    }


    public class Validation : Attribute
    {
        public Validation()
        {

        }
    }

    public class TemplateFile : Attribute
    {
        public string Filename { get; set; }

        public TemplateFile(string fileName)
        {
            Filename = fileName;
        }
    }



}