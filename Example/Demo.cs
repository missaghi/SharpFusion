using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sharp;
using Sharp.EndPoints;

namespace Example
{
    public class Demo : TemplateHandler
    {
        public Demo() { 
            //code here will run before begin request
        }

        [Endpoint("template")]
        [TemplateFile("/views/aTemplate.html")]
        public void TestEndpoint()
        {
            template.Set("world", "Hi");
        }

        [Endpoint("")]
        [TemplateFile("/views/aTemplate.html")]
        public void TestHome()
        {
            template.Set("world", "homepage");
        }

        [Endpoint("shared")]
        [TemplateFile("/content/shared/js/test.js")]
        public void CustomeContent()
        {
            template.Set("world", "homepage");
        }
         
    } 

    public class JSONDemo : JSONHandler
    {  
        [Endpoint("try/Json/{test}")]
        public int Cool(int id, string test)
        {
            return id + 3;
        }

        [Secure("doubleohseven")]
        [Endpoint("try/safe")]
        public int Safe(int id)
        {
            return id + 3;
        }

    }
}