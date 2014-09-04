using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Web;
using System.IO;

namespace Sharp
{

    public class LocalFile : System.Uri
    {
        public LocalFile(string path)
            : base(HttpContext.Current.Server.MapPath(path).ToString())
        {
        }
    }

    public class Template : Attribute
    {
        private FileRepo _fileRepo { get; set; }
        
        public FileRepo fileRepo { get { 
            if (_fileRepo == null)
                _fileRepo = new  Cached();
            return _fileRepo;
        }
            set { _fileRepo = value; }
        }

        private String template;
        public Hashtable Tags { get; set; }
        public const String TokenRegex = @"\[%(.*?)%\]|token=""(.*?)""|(""~/)"; 
        private LocalFile Path { get; set; }

        public Template(String template)
        {
            Tags = new Hashtable(10);
            this.template = template;
            ProcessIncludes();
        } 

        public Template(String Template, Hashtable tags)
        {
            Tags = tags ?? new Hashtable(10);
            template = Template;
            ProcessIncludes();
        }

        public Template(LocalFile file, FileRepo caching)  { 
            Path = file;
            Tags = new Hashtable(10);
            fileRepo = caching;
            template = _fileRepo.ReadFile(file.AbsolutePath);
            ProcessIncludes();
        } 

        public Template(LocalFile file)
        {
            Path = file;
            Tags = new Hashtable(10); 
            template = fileRepo.ReadFile(file.AbsolutePath);
            ProcessIncludes();
        }

        /// <summary>
        /// Clones a template and tags but not errors
        /// </summary>
        /// <returns></returns>
        public Template Clone()
        {
            Template temp = new Template(this.template);
            temp.Tags = new Hashtable(this.Tags.Count);

            if (this.Tags.Count > 0)
                temp.Tags = (Hashtable)this.Tags.Clone();

            return temp;
        }

        /// <summary> Add a new token to be replaced on the page. Does not HTML ENcode </summary>
        /// <param name="key">String Key to match key in the template</param> 
        public Template Set(String key, object value)
        {
            if (value != null)
                this[key] = value.ToString();

            return this;
        }


        /// <summary> Append to a tag (is optimized for recursive concat), allows appending to non-existent tag.    </summary>
        /// <param name="key">String Key to match key in the template</param> 
        public void Append(String key, Object value)
        {
            if (Tags.ContainsKey(key.ToLower()))
            {
                if (Tags[key.ToLower()].GetType().Equals(typeof(System.Text.StringBuilder)))
                    ((System.Text.StringBuilder)Tags[key.ToLower()]).Append(value.ToString());
                else
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    sb.Append(Tags[key.ToLower()]);
                    sb.Append(value.ToString());
                    Tags[key.ToLower()] = sb;
                }
            }
            else
            {
                Tags.Add(key.ToLower(), value.ToString());
            }

        }

        /// <summary> Maintains a lowercase hash table of template tokens. </summary>
        /// <param name="index">String Key to match key in the template</param> 
        public String this[string index]
        {
            get
            {
                if (Tags.ContainsKey(index.ToLower()))
                    return Tags[index.ToLower()].ToString();
                else
                    return "";
            }
            private set //this is so cool
            {
                if (Tags.ContainsKey(index.ToLower()))
                    Tags[index.ToLower()] = value;
                else
                {
                    Tags.Add(index.ToLower(), value);
                }
            }
        }

        public void ReplaceTag(String oldValue, String newValue)
        {
            ReplaceString("[%" + oldValue + "%]", newValue);
        }

        private void ReplaceString(String oldValue, String newValue)
        {
            template = template.ReplaceEx(oldValue, newValue);
        }

        public String Error
        {
            get
            {
                int errcount = String.IsNullOrEmpty((String)Tags["error"]) ? 0 : ((String)Tags["error"]).Split(new string[] { "<br />" }, StringSplitOptions.RemoveEmptyEntries).Length - 1;
                return String.IsNullOrEmpty((String)Tags["error"]) ? "" : String.Format((String)Tags["error"], errcount > 1 ? "errors" : "error", errcount).Replace("{{", "{").Replace("}}", "}").ToString();
            }
            set
            {
                if (value.Length > 0) //alows blanket assignment
                {
                    if (Tags.ContainsKey("error"))
                        Tags["error"] += "<br />" + value;
                    else
                        Tags.Add("error", "<b>Please correct the following {1} {0}:</b> <br /> " + value.Replace("{", "{{").Replace("}", "}}"));
                }
            }
        }

        public String Warning
        {
            get { return GetMessage("Warning"); }
            set { SetMessage(value, "Warning"); }
        }

        public String Msg
        {
            get { return GetMessage("Msg"); }
            set { SetMessage(value, "Msg"); }
        }

        private void SetMessage(string value, string tagname)
        {
            if (value.Length > 0) //alows blanket assignment
            {
                if (Tags.ContainsKey(tagname))
                    Tags[tagname] += "<br />" + value;
                else
                    Tags.Add(tagname, value);
            }
        }

        private string GetMessage(string tagname)
        {
            return String.IsNullOrEmpty((String)Tags[tagname]) ? "" : _fileRepo.ReadFile("/a/msg/" + tagname + ".htm").Replace("[%" + tagname.ToLower() + "%]", Tags[tagname].ToString());
        }

        private Boolean flag { get; set; }

        private void ProcessIncludes()
        {
            flag = false;
            Regex reg = new Regex(@"\[%inc:(.*?)%\]", RegexOptions.IgnoreCase);
            MatchEvaluator replaceCallback = new MatchEvaluator(ReplaceIncludeHandler);
            template = reg.Replace(template, replaceCallback);

            if (flag)
            {
                flag = false;
                ProcessIncludes();
            }
        }
        private string ReplaceIncludeHandler(Match token)
        {
            flag = true;
            String path = token.Groups[1].Value.ToLower();
            if (Path != null && path.IndexOf("./") == 0)
                path = System.IO.Path.GetDirectoryName(Path.AbsolutePath) + path;
            return _fileRepo.ReadFile(path);
        }


        /// <summary> This just reaplces a tag now rather than all at once</summary> 
        /// <param name="absPath">Dont forget a leading /</param> 
        public Template AddInclude(String tag, String absPath)
        {
            template = template.ReplaceEx("[%" + tag + "%]", _fileRepo.ReadFile(absPath));
            return this;
        }

        /// <summary>
        /// This builds the template to it's final form
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            //Process Includes  
            ProcessIncludes();

            //tokens (including ads)
            Regex reg = new Regex(TokenRegex, RegexOptions.IgnoreCase); //@"\[%(\w+)%\]|token=""(\w+)""|(""~/)|[%ad:(.*?)%\]";
            MatchEvaluator replaceCallback = new MatchEvaluator(ReplaceTokenHandler);
            template = reg.Replace(template, replaceCallback); //replace tokens with any matching tags or string.Empty

            return template;
        }


        /// <summary>Replaces the token Tag with the token Value.</summary> 
        /// <param name="token">Token.</param> 
        private string ReplaceTokenHandler(Match token)
        {
            //first group match is the original match
            String value = token.Groups[1].ToString().ToLower();
            value = value.Trim().NOE() ? token.Groups[2].ToString().ToLower() : value;
            value = value.Trim().NOE() ? token.Groups[3].ToString().ToLower() : value;
            value = value.Trim().NOE() ? token.Groups[4].ToString().ToLower() : value;

            if (value.Equals("301"))
            {
                HttpContext.Current.Response.Redirect(template.Replace("[%301%]", "").Trim(), true);
                return "";
            }
            //else if (value.Equals("\"~/"))
            //{
            //    return "\"/";
            //} 

            else if (value.Equals("meta_title"))
            {
                return this["meta_title"].NOE() ? "" : "<title>" + this["meta_title"] + "</title>";
            }
            else if (value.Equals("error"))
            {
                return this.Error;
            }
            //else if (value.Equals("warning"))
            //{
            //    return this.Warning;
            //}
            else if (value.Equals("notification"))
            {
                return this.Msg + this.Warning + this.Error;
            }
            else if (Tags.ContainsKey(value))
            {
                return Tags[value].ToString();
            }
            //not in tags
            else
            {
                // check request.forms
                if (Array.Exists<String>(HttpContext.Current.Request.Form.AllKeys, (x) => x.Else("").ToLower() == value))
                {
                    return HttpContext.Current.Request.Form[value].ToHTMLEnc();
                }
                // check request.querystring
                else if (Array.Exists<String>(HttpContext.Current.Request.QueryString.AllKeys, (x) => x.Else("").ToLower() == value))
                {
                    return HttpContext.Current.Request.QueryString[value].ToHTMLEnc();
                }
                //not found
                else
                {
                    return String.Empty;
                }
            }
        }



    }
}