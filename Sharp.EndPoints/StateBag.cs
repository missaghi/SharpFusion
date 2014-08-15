using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
//using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using System.Transactions; 

namespace Sharp
{
    public enum StringType
    {
        Cookie, IP, Email, URL, Phone, AlphaNumeric, YN, Free, SelectBox, Numeric, Date
    }

    [DataContract]
    public class StateBag
    {

        public TransactionScope Tn { get; set; }

        [DataMember]
        public Boolean Valid { get; set; }

        [DataMember]
        public String[] Required { get; set; } 

        [DataMember]
        public String[] Errors { get { return errors.ToArray(); } set { } }
        private List<string> errors { get; set; }

        public String ErrorMsg
        {
            get { return string.Join("<br />", errors); }
            set
            {
                if (value.NNOE())
                {
                    errors.Add(value);
                } 
                Valid = false;
            }
        }   

        public StateBag()
        {
            Valid = true; 
            errors = new List<string>();
            // Errors = new Dictionary<string, string>();
        }

        public void Reset(Template template)
        {
            if (!Valid)
                template.Error = ErrorMsg;
            Valid = true;
            errors = new List<string>(); 
        }

        public void Int(int? dal, object value, string error)
        {
            int i;
            if (int.TryParse(value.ToString(), out i))
            {
                dal = i;
            }
            else
            {
                ErrorMsg = error;
            }
        }

        public int Int(object value, string error)
        {
            int i;
            if (int.TryParse(value.ToString(), out i))
            {
                return i;
            }
            else
            {
                ErrorMsg = error;
                return 0;
            }
        }


        public void Long(long? dal, object value, string error)
        {
            int i;
            if (int.TryParse(value.ToString(), out i))
            {
                dal = i;
            }
            else
            {
                ErrorMsg = error;
            }
        }

        public long Long(object value, string error)
        {
            long i;
            if (long.TryParse(value.ToString(), out i))
            {
                return i;
            }
            else
            {
                ErrorMsg = error;
                return 0;
            }
        }


        public static String Pattern(StringType stype)
        {
            string pattern = string.Empty;
            switch (stype)
            {
                case StringType.Cookie:
                    pattern = @"([a-zA-Z0-9_-]+)$";
                    break;
                case StringType.SelectBox:
                    pattern = @"([,a-zA-Z0-9_-]+)$";
                    break;
                case StringType.AlphaNumeric:
                    pattern = @"([a-zA-Z0-9_-]+)$";
                    break;
                case StringType.Numeric:
                    pattern = @"^\d+(\.\d\d?)?$";
                    break;
                case StringType.Date:
                    pattern = @"([0-9\/]+)$";
                    break;
                case StringType.IP:
                    pattern = @"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b";
                    break;
                case StringType.Email:
                    pattern = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";
                    break;
                case StringType.URL:
                    pattern = @"^(?#Protocol)(?:(?:ht|f)tp(?:s?)\:\/\/|~/|/)?(?#Username:Password)(?:\w+:\w+@)?(?#Subdomains)(?:(?:[-\w]+\.)+(?#TopLevel Domains)(?:com|org|net|gov|mil|biz|info|mobi|name|aero|jobs|museum|travel|[a-z]{2}))(?#Port)(?::[\d]{1,5})?(?#Directories)(?:(?:(?:/(?:[-\w~!$+|.,=]|%[a-f\d]{2})+)+|/)+|\?|#)?(?#Query)(?:(?:\?(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)(?:&(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)*)*(?#Anchor)(?:#(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)?$";
                    break;
                case StringType.YN:
                    pattern = @"Y|y|N|n";
                    break;
                case StringType.Phone:
                    pattern = @"^[\(]?(\d{3})[\)]?[\s]?[\-]?[\.]?(\d{3})[\s]?[\-]?[\.]?(\d{4})[\s]?[x]?[\s]?(\d{0,5})$";
                    break;
                case StringType.Free:
                    pattern = @".*";
                    break;
                default:
                    break;
            }
            return pattern;
        }

        public T Test<T>(Boolean pass, T str, String error)
        {
            if (!pass)
            {
                ErrorMsg = error;
            }
            return str;
        }


        public StateBag Test(Boolean pass, String error)
        {
            Test(pass, "", error);
            return this;
        }

        public object TestCast<T>(object val, String error)
        {
            try
            {
                return Convert.ChangeType(val, typeof(T));
            }
            catch
            {
                ErrorMsg = error;
                return null;
            }
        }

        public struct TestResult
        {
            public String Str;
            public Boolean Pass;
        }

        public String TestPattern(String str, StringType pattern, String error)
        {
            return Test(TestPattern(str, pattern), str, error);
        }

        public static Boolean TestPattern(String str, StringType pattern)
        {
            TestResult tr = new TestResult();
            tr.Str = str;
            RegexOptions options = new RegexOptions();
            options |= RegexOptions.IgnoreCase;
            Regex reg = new Regex(Pattern(pattern), options);
            return reg.IsMatch(tr.Str);
        }

        public DateTime TestDate(String str, String error)
        {
            DateTime dt = new DateTime();
            DateTime.TryParse(str, out dt);
            return dt;
        }

        public String TestEmpty(String str, String error)
        {
            return Test(!str.NOE(), str, error);
        }

        public String TestSelect(String str, String[][] ValidChoices, String error)
        {
            return Test(Array.Exists<String[]>(ValidChoices, (x) => x[0] == str), str, error); ;
        }

        public String TestSelectByKey(String str, String[][] ValidChoices, String error)
        {
            String key = str;
            if (Array.Exists<String[]>(ValidChoices, (x) => x[1].Like(str)))
                key = Array.Find<String[]>(ValidChoices, (x) => x[1].Like(str))[0];
            return Test(Array.Exists<String[]>(ValidChoices, (x) => x[1].Like(str)), key, error); ;
        }

        //strips duplicates, tests for existance in a selectbox  
        public String[] TestSelect(String[] str, String[][] ValidChoices, String error)
        {
            //strip duplicates
            List<String> distinct = new List<string>(str.Length);
            foreach (String item in str)
            {
                if (!distinct.Contains(item))
                    distinct.Add(item);
            }
            str = distinct.ToArray();

            return Test<String[]>(
                Array.TrueForAll<String>(str, (x) => Array.Exists<String[]>(ValidChoices, (y) => y[0] == x)) && str.Length > 0
                , str
                , error);
        }



    }
}