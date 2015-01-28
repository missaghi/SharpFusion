using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp.EndPoints
{
    public abstract class TypeParser
    { 
        public Type type { get; set; }

        public virtual void ParseType(Type _type) { }
         
    }
}
