using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sharp.EndPoints
{  

    public class AssemblyParser
    {
        private Dictionary<Assembly, List<TypeParser>> assemblies { get; set; }

        public AssemblyParser() { assemblies = new Dictionary<Assembly, List<TypeParser>>(); }

        public void AddAssembly(Assembly assembly)  
        {
            AddAssemblyWithParser(assembly, null);
        }

        public void AddAssemblyWithParser(Assembly assembly, params TypeParser[] parsers)
        {
            if (!assemblies.Keys.Contains(assembly))
            {
                SharpHost.Instance.Assemblies.Add(assembly);
                assemblies.Add(assembly, new List<TypeParser>());
            }

            if (parsers != null)
             assemblies[assembly].AddRange(parsers);
        }

        public void AddGlobalParsers(params TypeParser[] parsers)
        {
            foreach (var assembly in assemblies) {
                var newParsers = parsers.Where(x => !assembly.Value.Any(v => v.GetType() == x.GetType()));
                assembly.Value.AddRange(newParsers);
            }
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
