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
         
    } 

    public class JSONDemo : JSONHandler
    {  
        [Endpoint("try/Json")]
        public int Cool(int id)
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