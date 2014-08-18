using System;
using System.IO;
using System.Reflection;

namespace Sharp
{

    public class Resources<T>
    {
        static Read<T> _LoadFile = new Read<T>();
        public static Read<T> Read
        {
            get
            {
                return _LoadFile;
            }
        }
    }

    public class Read<T>
    {

        public string this[string i]
        {
            get
            {
                string result;
                Assembly assembly = typeof(T).Assembly;
                using (Stream stream = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + i))
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
                return result;

            }
            set { }
        }
    }
}