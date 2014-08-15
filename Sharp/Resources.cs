using System;
using System.IO;
using System.Reflection;

namespace Sharp
{

    public class Resources
    {
        static Read _LoadFile = new Read();
        public static Read Read
        {
            get
            {
                return _LoadFile;
            }
        }
    }

    public class Read
    {

        public string this[string i]
        {
            get
            {
                string result;
                using (Stream stream = Assembly.GetExecutingAssembly()
                               .GetManifestResourceStream(i))
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