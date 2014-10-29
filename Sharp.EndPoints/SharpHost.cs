using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sharp;

namespace Sharp.EndPoints
{
    //App Singleton
    public sealed class SharpHost
    {
        private static readonly Lazy<SharpHost> lazy =
            new Lazy<SharpHost>(() => new SharpHost());

        public static SharpHost Instance { get { return lazy.Value; } }

        public HashSet<Assembly> Assemblies { get; set; }

        private SharpHost()
        {
            Assemblies = new HashSet<Assembly>();  
        }

    }
}
