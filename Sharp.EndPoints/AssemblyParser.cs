using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sharp.EndPoints
{ 
    public interface TypeParser
    {
        void ParseType(Type type);
    } 

    public class AssemblyParser
    {
        private Dictionary<Assembly, List<TypeParser>> assemblies { get; set; }

        public AssemblyParser() { assemblies = new Dictionary<Assembly, List<TypeParser>>(); }

        public void AddAssemblyParsers(Assembly assembly)  
        {
            AddAssemblyParsers(assembly, new RouteParser());
        }

        public void AddAssemblyParsers(Assembly assembly, params TypeParser[] parsers)
        {
            if (!assemblies.Keys.Contains(assembly))
            {
                SharpHost.Instance.Assemblies.Add(assembly);
                assemblies.Add(assembly, new List<TypeParser>());
            }

            assemblies[assembly].AddRange(parsers.ToList());
        }


        public void Parse()
        {
            foreach (Assembly assembly in assemblies.Keys)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    foreach (TypeParser parser in assemblies[assembly])
                    {
                        parser.ParseType(type);
                    }
                }
            }
        }


    }
}
