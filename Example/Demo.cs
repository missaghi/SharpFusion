using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sharp;
using Sharp.EndPoints;

namespace Example
{
    /// <summary>
    /// Demo of the various route plugins
    /// </summary>
    public class Demo : EndpointHandler
    {
        public Demo() { 
            //code here will run before begin request
        }

        [EndPointPlugin("TemplatePlugin.current")]
        [TemplatePlugin("/views/aTemplate.html")]
        public void TestEndpoint()
        {
            TemplatePlugin.current.Set("world", "Hi");
        }

        [EndPointPlugin("genKey")]
        [TemplatePlugin("/views/aTemplate.html")]
        public void TestEncryption()
        {
            var e = new Encryption();

            TemplatePlugin.current.Append("world", "Key:" + Encryption.ByteArrToString(e.Key));
            TemplatePlugin.current.Append("world", "<br>Vector:" + Encryption.ByteArrToString(e.Vector));
        }

        /// <summary>
        /// Really cool summary here
        /// </summary>
        [EndPointPlugin("")]
        [TemplatePlugin("/views/aTemplate.html")]
        public void TestHome()
        {
            TemplatePlugin.current.Set("world", "homepage");
        }

        [EndPointPlugin("shared")]
        [TemplatePlugin("/content/shared/js/test.js")]
        public void CustomeContent()
        {
            TemplatePlugin.current.Set("world", "homepage");
        }
         
    } 

    public class JSONDemo : EndpointHandler
    {  
        [EndPointPlugin("try/Json/{test}")]
        public int Cool(int id, string test)
        {
            return id + 3;
        }

        [Secure("doubleohseven")]
        [EndPointPlugin("try/safe")]
        public int Safe(int id)
        {
            return id + 3;
        }

    }
}