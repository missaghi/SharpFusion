using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Sharp;

namespace Sharp.EndPoints
{ 

    public class TenantRepo : FileRepo
    {
        public string SharedPath = "/content/shared/";

        public override String ReadFile(String file)
        {
            file = file.Replace("\\", "/").ToLower();
            
            if (!file.Contains(":"))
                file = HttpContext.Current.Server.MapPath(file).Replace("\\", "/").ToLower();

            var sharedPath = HttpContext.Current.Server.MapPath(SharedPath).Replace("\\", "/").ToLower();
            if (file.ToLower().Contains(sharedPath))
            {
                string domainfile = file.Replace(SharedPath, "/content/" + HttpContext.Current.Request.Url.DnsSafeHost + "/");
                if (HttpContext.Current.Cache["File: " + domainfile] == null)
                {
                    if (File.Exists(domainfile))
                        file = domainfile;
                }  
            }

            if (HttpContext.Current.Cache["File: " + file] == null)
            {
                if (File.Exists(file))
                    HttpContext.Current.Cache.Add("File: " + file, File.ReadAllText(file), new System.Web.Caching.CacheDependency(file), DateTime.MaxValue, System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.BelowNormal, null);
                else
                {
                    throw new FileNotFoundException("File Not Found:" + file);
                }
            }
            return (String)HttpContext.Current.Cache["File: " + file];
        }

        public static string GetFile(string file)
        {
            return new TenantRepo().ReadFile(file);
        }
    }
}