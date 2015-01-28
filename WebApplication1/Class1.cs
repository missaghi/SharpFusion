using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace WebApplication1
{
    public class Class1 : ApiController
    {
        [Route("test")]
        public int test() {
            return 3;
        }

    }
}