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

                return  JsonConvert.DeserializeObject<T>(json, new Newtonsoft.Json.JsonConverter[] {}); // new FieldConverter<T>() });
            }
        }
    }
}