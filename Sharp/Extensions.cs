using System;
using System.Web;
//using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using System.IO;
using System.Collections.Generic;
//using System.Web.Script.Serialization;
using System.Reflection;
using System.ComponentModel;
using System.Dynamic;


namespace Sharp
{
    public class SString
    {
        public static string Parse(string str)
        {
            return str;
        }
    }

    public static partial class GenericExtensions
    {
        public static List<T> AddToList<T>(this List<T> list, T add)
        {
            list.Add(add);
            return list;
        }
    }

    public static class DynamicExtensions
    {
        public static dynamic ToDynamic(this object value)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
                expando.Add(property.Name, property.GetValue(value));

            return expando as ExpandoObject;
        }
    }

    public static partial class Extensions
    {

        //nullabel friendly
        public static string ToString(this DateTime? obj, String param)
        {
            if (obj.HasValue)
                return obj.Value.ToString(param);
            else
                return "";

        }

        public static string ToString(this int? obj, String param)
        {
            if (obj.HasValue)
                return obj.Value.ToString(param);
            else
                return "";

        }



        //Makes a string array of enum objects
        public static String[][] Enumerate<T>()
        {
            List<String[]> str = new List<string[]>();
            foreach (Object o in Enum.GetValues(typeof(T)))
            {
                str.Add(new string[] { o.ToString(), o.ToString() });
            }
            return str.ToArray();
        }

        public static String Ellipsis(this String text, int maxLength)
        {
            if (text.NNOE())
            {
                if (maxLength > 3)
                { maxLength = maxLength - 3; }
                else
                {
                    return "...";
                }
                if (text.Length > maxLength)
                {
                    text = text.Substring(0, maxLength);
                    int lastspace = text.LastIndexOf(" ");

                    if (lastspace < 0)
                        text = text.Substring(0, maxLength) + "...";
                    else
                        text = text.Substring(0, lastspace) + "...";
                }
            }
            return text;
        }

        public static String[] Split(this String str, String split)
        {
            if (!String.IsNullOrEmpty(str))
            {
                String[] splut = Regex.Split(str, split);
                List<String> diet = new List<string>(splut.Length);
                foreach (string splite in splut)
                { if (!splite.NOE()) { diet.Add(splite); } }
                return diet.ToArray();
            }
            else
                return new String[] { };
        }

        /// <summary>
        /// Splits by Comma
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static String[] Split(this String str)
        {
            return str.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static String Else(this String str, String other)
        {
            return String.IsNullOrEmpty(str) ? other : str;
        }
        public static String Else(this Object str, String other)
        {
            return str != null ? (String.IsNullOrEmpty(str.ToString()) ? other : str.ToString()) : other;
        }
        public static Object Else(this Object str, Object other)
        {
            return string.IsNullOrEmpty(str.ToString()) ? other : str;
        }

        public static Boolean Else(this Boolean? str, Boolean other)
        {
            return str.HasValue ? (String.IsNullOrEmpty(str.ToString()) ? other : str.Value) : other;
        }

        public static T Else<T>(this T str, T other)
        {
            return str != null ? (String.IsNullOrEmpty(str.ToString()) ? other : str) : other;
        }

        public static string[] Else(this string[] str, string[] other)
        {
            return str != null ? (str.Length == 0 ? other : str) : other;
        }

        public static string[][] Else(this string[][] str, string[][] other)
        {
            return str != null ? (str.Length == 0 ? other : str) : other;
        }

        public static String ToYN(this Boolean yn)
        {
            return yn.ToCustom("Y", "N");
        }

        public static String ToCustom(this Boolean yn, String yes, String no)
        {
            return yn == true ? yes : no;
        }

        public static String ToCustom(this Boolean? yn, String yes, String no)
        {
            if (yn.HasValue)
                return yn == true ? yes : no;
            else
                return no;
        }

        public static String MakeLineBreaksHTML(this String str)
        {
            return str.Else(string.Empty).Replace("\n", "<br />");
        }

        /// <summary>Encode to html safe</summary>
        public static String ToHTMLEnc(this String str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return "";
            }
            else
            {
                return HttpUtility.HtmlEncode(str).Replace("'", "&#39;"); //.Replace("\"", "&#34;").Replace("&quot;", "&#34;");
            }
        } /// <summary>Encode to html safe</summary>

        public static String ToHTMLDecode(this String str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return "";
            }
            else
            {
                return HttpUtility.HtmlDecode(str); //.Replace("&#34;", "\"").Replace("&quot;", "\"").Replace("&#39;", "'");
            }
        }

        public static String MakeFileSystemSafe(this String str)
        {
            string alpha = "1234567890abcdefghijklmnopqrstuvwxyz_-.";
            return FilterString(str, alpha);
        }

        public static String MakeURLSafe(this String str)
        {
            string alpha = "1234567890abcdefghijklmnopqrstuvwxyz_-. ";
            return FilterString(str, alpha);
        }

        public static String MakeSEOURL(this String str)
        {
            if (str.NOE()) str = "";
            string alpha = "1234567890abcdefghijklmnopqrstuvwxyz ";
            return FilterString(str, alpha).Ellipsis(100).Replace("...", "").Trim().Replace(" ", "-");
        }

        public static String MakeAlphaNumeric(this String str)
        {
            string alpha = "1234567890abcdefghijklmnopqrstuvwxyz";
            return FilterString(str, alpha).Ellipsis(50).Trim();
        }

        public static String MakeFullTextSearchSafe(this String str)
        {
            string alpha = "1234567890abcdefghijklmnopqrstuvwxyz ";
            return FilterString(str, alpha).Ellipsis(50).Trim().Replace(" ", "|");
        }

        private static string FilterString(String str, string alpha)
        {
            string clean = str;
            foreach (char c in str)
            {
                if (!alpha.Contains(c.ToString().ToLower()))
                {
                    clean = clean.Replace(c.ToString(), string.Empty);
                }
            }

            return clean;
        }



        /// <summary>Encode to URL safe EscapeDataString</summary>
        public static String ToURL(this String str)
        {
            str = str.Else("");
            //return HttpUtility.UrlEncode(str);
            return System.Uri.EscapeDataString(str);
        }

        public static String URLDecode(this String str)
        {
            return HttpUtility.UrlDecode(str);
        }

        public static Boolean ToBool(this string yn)
        {
            String[] TrueValues = new String[] { "true", "y", "yes", "ok", "1", "on" };
            return Array.Exists<String>(TrueValues, (s) => s.Equals(yn.Else("").ToLower()));
        }

        /// <summary>
        /// checked="checked"
        /// </summary>
        /// <param name="yn"></param>
        /// <returns></returns>
        public static String ToChecked(this Boolean yn)
        {
            return yn.ToCustom("checked=\"checked\"", "");
        }
        public static String ToChecked(this Boolean? yn)
        {
            if (yn.HasValue)
                return yn.Value.ToCustom("checked=\"checked\"", "");
            else
                return "";
        }

        public static String ToJScript(this String str)
        {
            return str.ToString().Replace("\\n", "\n").Replace("\n", "\\n").Replace("\\r", "\r").Replace("\r", "\\r").Replace("\\\"", "\"").Replace("\"", "\\\"");
        }
        /// <summary>Case in-sensitive equals </summary> 
        public static Boolean Like(this String str, String like)
        {
            if (str.NNOE() && like.NNOE())
                return str.Equals(like, StringComparison.CurrentCultureIgnoreCase);
            else
                return (str.NOE() && like.NOE());
        }

        public static String Capitalize(this String str)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.Else(""));
        }

        public static String CapitalizeFirstLetterOnly(this String str)
        {
            return str[0].ToString().ToUpper() + str.Substring(1);
        }

        public static Boolean NOE(this String str)
        {
            return String.IsNullOrEmpty(str);
        }

        /// <summary>
        /// Not null or empty
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Boolean NNOE(this String str)
        {
            return !NOE(str);
        }

        //public static String Ellipsis(this string str, int length)
        //{
        //    return Tools.StringEllipsis(str, length);
        //}
        public static T[] Ellipsis<T>(this T[] str, int length)
        {
            if (str != null)
            {
                if (str.Length > length)
                    Array.Resize<T>(ref str, length);
            }
            return str;
        }

        public static Int32 ToInt(this string str, Int32 def)
        {
            Int32.TryParse(str, out def);
            return def;
        }

        public static long? ToNLong(this string str)
        {
            long def = 0;
            long.TryParse(str, out def);
            return def;
        }

        public static long ToLong(this string str, long def)
        {
            long.TryParse(str, out def);
            return def;
        }

        public static double ToDbl(this string str, double def)
        {
            double.TryParse(str, out def);
            return def;
        }

        public static System.Boolean IsNumeric(this System.Object Expression)
        {
            if (Expression == null || Expression is DateTime)
                return false;

            if (Expression is Int16 || Expression is Int32 || Expression is Int64 || Expression is Decimal || Expression is Single || Expression is Double || Expression is Boolean)
                return true;

            try
            {
                if (Expression is string)
                    Double.Parse(Expression as string);
                else
                    Double.Parse(Expression.ToString());
                return true;
            }
            catch { } // just dismiss errors but return false
            return false;
        }

        public static String RestCommand(this HttpRequest request)
        {
            return request.Path.TrimEnd(new char[] { '/' }).Substring(request.Path.TrimEnd(new char[] { '/' }).LastIndexOf("/") + 1);
        }

        /// <summary>
        /// Ubber fast case insensitive string replace
        /// </summary>
        public static string ReplaceEx(this string original, string pattern, string replacement)
        {
            int count, position0, position1;
            count = position0 = position1 = 0;
            string upperString = original.ToUpper();
            string upperPattern = pattern.ToUpper();
            int inc = (original.Length / pattern.Length) *
                      (replacement.Length - pattern.Length);
            char[] chars = new char[original.Length + Math.Max(0, inc)];
            while ((position1 = upperString.IndexOf(upperPattern,
                                              position0)) != -1)
            {
                for (int i = position0; i < position1; ++i)
                    chars[count++] = original[i];
                for (int i = 0; i < replacement.Length; ++i)
                    chars[count++] = replacement[i];
                position0 = position1 + pattern.Length;
            }
            if (position0 == 0) return original;
            for (int i = position0; i < original.Length; ++i)
                chars[count++] = original[i];
            return new string(chars, 0, count);
        }

        public static Boolean LikeTheEnd(this String str, String[] ofThese)
        {
            return Array.Exists<String>(ofThese, (x) => x.EndsWith(str, StringComparison.CurrentCultureIgnoreCase));
        }

        public static Boolean EndsLike(this String str, String[] ofThese)
        {
            return Array.Exists<String>(ofThese, (x) => str.EndsWith(x, StringComparison.CurrentCultureIgnoreCase));
        }

        public static Boolean LikeOne(this String str, String[] ofThese)
        {
            return Array.Exists<String>(ofThese.Else<string[]>(new string[] { }), (x) =>
                str.Like(x));
        }

        public static Boolean Contains(this String str, String[] anyofThese)
        {
            return Array.Exists<String>(anyofThese, (x) =>
                str.ToLower().Contains(x.ToLower()));
        }

        public static String[] FindByKey(this String[][] jaggedArray, String key)
        {
            key = key.Else("");
            foreach (String[] options in jaggedArray)
            {
                if (options[0].Like(key))
                {
                    return options;
                }
            }
            return new String[] { };
        }

        public static String[] FindByVal(this String[][] jaggedArray, String val)
        {
            val = val.Else("");
            foreach (String[] options in jaggedArray)
            {
                bool alike = options[1].Like(val);
                if (alike)
                {
                    val = options[1];
                    return options;
                }
            }
            return new String[] { };
        }

        ///// <summary>
        ///// Depreciateing because it ignores catacontracts
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <returns></returns>
        //public static String ToJSON(this object obj)
        //{
        //    JavaScriptSerializer serializer = new JavaScriptSerializer(); 
        //    return serializer.Serialize(obj);
        //}


        #region dates

        public static string ToPrintTimeSpan(this TimeSpan t)
        {
            string answer;
            if (t.TotalHours < 1.0)
            {
                answer = String.Format("{0}min", t.Minutes, t.Seconds);
            }
            else // more than 1 hour
            {
                answer = String.Format("{0}h {1:D2}min", (int)t.TotalHours, t.Minutes, t.Seconds).Replace(" 00min", "");
            }

            return answer;
        }


        public static string Ordinal(this int number)
        {
            var work = number.ToString();
            if (number == 11 || number == 12 || number == 13)
                return work + "th";
            switch (number % 10)
            {
                case 1: work += "st"; break;
                case 2: work += "nd"; break;
                case 3: work += "rd"; break;
                default: work += "th"; break;
            }
            return work;
        }


        public static double ToMsSinceEpoch(this DateTime dt)
        {
            DateTime d1 = new DateTime(1970, 1, 1);
            DateTime d2 = dt.ToUniversalTime();
            TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);
            return ts.TotalMilliseconds;
        }

        public static DateTime NextOccuranceOfDay(this DateTime startdate, DayOfWeek dayoftheweek)
        {
            while (startdate.DayOfWeek != dayoftheweek)
            { startdate = startdate.AddDays(1); }
            return startdate;
        }

        public static string ToDateString(this DateTime? dateTime, string param)
        {
            if (dateTime.HasValue)
                return dateTime.Value.ToString(param);
            else
                return "";
        }

        public static string ToRelativeDate(this DateTime? dateTime)
        {
            if (dateTime.HasValue)
                return dateTime.Value.ToRelativeDate();
            else
                return "";
        }

        public static string ToRelativeDate(this DateTime? dateTime, String Prefix)
        {
            if (dateTime.HasValue)
                return dateTime.Value.ToString(Prefix).ToLower() + " " + dateTime.ToRelativeDate();
            else return "";
        }

        public static string ToRelativeDate(this DateTime dateTime, String Prefix)
        {
            return dateTime.ToString(Prefix).ToLower() + " " + dateTime.ToRelativeDate();
        }


        /// <summary>  Converts the specified DateTime to its relative date. </summary>       
        /// <param name="dateTime">The DateTime to convert.</param> 
        /// <returns>A string value based on the relative date
        /// of the datetime as compared to the current date.</returns> 
        public static string ToRelativeDate(this DateTime dateTime)
        {
            bool past = DateTime.Now > dateTime;
            var timeSpan = past ? DateTime.Now - dateTime : dateTime - DateTime.Now;
            int time;
            string units;
            string ago = " ago";
            string fromnow = " in ";

            if (timeSpan <= TimeSpan.FromMilliseconds(1000))
            {
                time = timeSpan.Milliseconds; units = "ms";
            }
            // span is less than or equal to 60 seconds, measure in seconds. 
            else if (timeSpan <= TimeSpan.FromSeconds(60))
            {
                time = timeSpan.Seconds; units = "s";
            }

            // span is less than or equal to 60 minutes, measure in minutes. 
            else if (timeSpan <= TimeSpan.FromMinutes(60))
            {
                time = timeSpan.Minutes; units = "m";

            }

            // span is less than or equal to 24 hours, measure in hours. 
            else if (timeSpan <= TimeSpan.FromHours(24))
            {
                time = timeSpan.Hours; units = "hr";

            }

            // span is less than or equal to 30 days (1 month), measure in days.
            else if (timeSpan <= TimeSpan.FromDays(30))
            {
                time = timeSpan.Days; units = " day";

            }
            //just give the date
            else
            {
                return dateTime.Month + "/" + dateTime.Day + "/" + dateTime.Year;

            }

            if (past)
                return time + units + (time == 1 ? ago : "s" + ago).ToLower();
            else
                return fromnow + time + units + (time == 1 ? "" : "s").ToLower();

        }


        #endregion


    }

    public static class EnumUtils
    {
        /// <summary>
        /// Return the enum description attribute value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Description(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return value.ToString();
            }
        }

        public static object enumValueOf(this string value, Type enumType)
        {
            string[] names = Enum.GetNames(enumType);
            foreach (string name in names)
            {
                if (Description((Enum)Enum.Parse(enumType, name)).Equals(value))
                {
                    return Enum.Parse(enumType, name);
                }
            }

            throw new ArgumentException("The string is not a description or value of the specified enum.");
        }

        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }
    }

    public static class CSV
    {
        public static String[][] ParseCSV(this string csv)
        {
            // declare the Regular Expression that will match versus the input string
            Regex re = new Regex("((?<field>[^\",\\r\\n]+)|\"(?<field>([^\"]|\"\")+)\")(,|(?<rowbreak>\\r\\n|\\n|$))");

            List<String> colArray = new List<String>();
            List<String[]> rowArray = new List<String[]>();

            int colCount = 0;
            int maxColCount = 0;
            string rowbreak = "";
            string field = "";

            MatchCollection mc = re.Matches(csv);

            foreach (Match m in mc)
            {

                // retrieve the field and replace two double-quotes with a single double-quote
                field = m.Result("${field}").Replace("\"\"", "\"");

                rowbreak = m.Result("${rowbreak}");

                if (field.Length > 0)
                {
                    colArray.Add(field);
                    colCount++;
                }

                if (rowbreak.Length > 0)
                {

                    // add the column array to the row Array List
                    rowArray.Add(colArray.ToArray());

                    // create a new Array List to hold the field values
                    colArray = new List<String>(maxColCount);

                    if (colCount > maxColCount)
                        maxColCount = colCount;

                    colCount = 0;
                }
            }

            if (rowbreak.Length == 0)
            {
                // this is executed when the last line doesn't
                // end with a line break
                rowArray.Add(colArray.ToArray());
                if (colCount > maxColCount)
                    maxColCount = colCount;
            }


            return rowArray.ToArray();

        }

        public static String JoinCSV(this String[][] csv)
        {
            StringBuilder sb = new StringBuilder(csv.Length);
            foreach (String[] row in csv)
            {
                sb.AppendLine("\n\n" + String.Join("\n", row));
            }

            return sb.ToString();
        }

        public static String[] UrlParts(this HttpRequest request)
        {
            return request.Path.Split("/");
        }

        public static String Then(this Boolean result, string then)
        {
            return result == true ? then : string.Empty;
        }




    }

    /// <summary>
    /// Extension methods for HTTP Request.
    /// <remarks>
    /// See the HTTP 1.1 specification http://www.w3.org/Protocols/rfc2616/rfc2616.html
    /// for details of implementation decisions.
    /// </remarks>
    /// </summary>
    public static class HttpRequestExtensions
    {

        /// <summary>
        /// Dump the raw http request to a string. 
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> that should be dumped.               </param>
        /// <returns>The raw HTTP request.</returns>
        public static string ToRaw(this HttpRequest request)
        {
            StringWriter writer = new StringWriter();

            WriteStartLine(request, writer);
            WriteHeaders(request, writer);
            WriteBody(request, writer);

            return writer.ToString();
        }

        public static string GetBody(this HttpRequest request)
        {
            StringWriter writer = new StringWriter();
            WriteBody(request, writer);

            return writer.ToString();
        }

        private static void WriteStartLine(HttpRequest request, StringWriter writer)
        {
            const string SPACE = " ";

            writer.Write(request.HttpMethod);
            writer.Write(SPACE + request.Url);
            writer.WriteLine(SPACE + request.ServerVariables["SERVER_PROTOCOL"]);
        }


        private static void WriteHeaders(HttpRequest request, StringWriter writer)
        {
            foreach (string key in request.Headers.AllKeys)
            {
                writer.WriteLine(string.Format("{0}: {1}", key, request.Headers[key]));
            }

            writer.WriteLine();
        }


        private static void WriteBody(HttpRequest request, StringWriter writer)
        {
            StreamReader reader = new StreamReader(request.InputStream);

            try
            {
                string body = reader.ReadToEnd();
                writer.WriteLine(body);
            }
            finally
            {
                reader.BaseStream.Position = 0;
            }
        }
    }
}