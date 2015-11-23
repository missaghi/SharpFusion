﻿using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text; 
using Newtonsoft.Json;
using System.Dynamic;
using Newtonsoft.Json.Converters;

namespace Sharp
{
    public static class JSONHelper
    {
        public static string ToJSON<T>(this T obj)
        {
            //System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(obj.GetType());
            //MemoryStream ms = new MemoryStream();
            //serializer.WriteObject(ms, obj);
            //string retVal = Encoding.Default.GetString(ms.ToArray());
            //return retVal;

            return   JsonConvert.SerializeObject(obj, Formatting.Indented);
        }  

        public static T ParseJSON<T>(this string json)
        {

            if (typeof(T) == typeof(string))
                return (T)(object)json.ToString();
            else
            {
                //T obj = Field<T>.invoke(); // Activator.CreateInstance<T>();
                //MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
                //System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(obj.GetType());
                //obj = (T)serializer.ReadObject(ms);
                //ms.Close();
                //return obj;

                if (!json.StartsWith("{") && !json.StartsWith("\"") && !json.StartsWith("["))
                    json = "\"" + json + "\"";

                if (typeof(T).Equals(typeof(ExpandoObject)))
                    return JsonConvert.DeserializeObject<T>(json, new ExpandoObjectConverter()); // new FieldConverter<T>() });
                else if (typeof(T).Equals(typeof(DateTime)))
                {
                    if (json.IndexOf("(") > -1)
                        json = json.Substring(0, json.IndexOf("("));

                    DateTime date;
                    if (DateTime.TryParse(json, out date))
                        return (T)(object)date;
                    else if (DateTime.TryParseExact(json, "ddd MMM dd yyyy h:mm:ss tt zzz", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date))
                        return (T)(object)date;
                    else
                        return JsonConvert.DeserializeObject<T>(json, new Newtonsoft.Json.JsonConverter[] { });        
                }
                else
                    return JsonConvert.DeserializeObject<T>(json, new Newtonsoft.Json.JsonConverter[] { }); // new FieldConverter<T>() });
            }
        }
    }
}