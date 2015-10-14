SharpFusion
------------------
A simple web framework for service oriented architectures, with a  page template, and ORM.

Nuget: https://www.nuget.org/packages/Sharp/

The framework consists of 4 major parts: The SQL generator, the Data Access Layer, the Templating System, and automatic url end-point mapping to eliminate the need for runtime reflection.

Usage Guides:

-Pages
-Object Relational Mapper
-SQL DDL Generator
- Validation tools

**Pages** Instead of using .aspx pages that muddled the tiers we wanted to use html templates with just tokens, both for performance and to further separate presentation layer which enhances the ability to separate the duties of the developers.

The overarching model is to build each layer as a service to other layers, that means that your front end layer ends up being a stand alone app that consumes your web app via an API. The side effect is that you don't have to make a second project out of exposing an API. This also forces the application to consider security in a more uniform way.

Features Ajax test pages Auto sitemap

**Object Relational Mapper** To connect to databases we wanted simple objects to be created from tables using the lightest methods, ie: no data-adapters, no dynamic SQL.

**Features**: Completely open classes. Object oriented allows for easy customization

**SQL DDL Generator** The SQL generator takes a psudo-code like syntax and generates SQL DML and many handy stored procs, including selects by foreign keys.

Features: Index control select by cascading FK

Other Features
-Many Extention classes, eg (String ellipsis, Simple Encryption, Search Arrays, Parse CSV, Serialize to JSON, Parse URL parts, Else values )
-Image utilities like resizing and cropping
-Caching wrappers
-Base36 conversion (base31 to eliminate easily misread letters eg 5 and S )


# Introduction

A class that is decorated with a WebPage attribute will be wired to a url via the web config. To wire it up run the site and open _/MapHandlers_

All endpoints need to be HttpHandlers so inherit SharpFusion[?](https://code.google.com/p/sharpfusion/w/edit/SharpFusion).Page which implements IHttpHandler and has a Template property that will get written to the response when IHttpHanlder.ProcessRquest is called.

# <a name="Details"></a>Details

ChatRoom[?](https://code.google.com/p/sharpfusion/w/edit/ChatRoom).cs

<pre> [WebPage("/r/*")]
    public class ChatRoom : Page
    {
        public ChatRoom()
            : base("/a/html/room.htm")
        {

            template.Set("servetime", DateTime.Now);</pre>

/a/html/room.htm

<pre><!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
    <title>Room Name</title>
    [%inc:/a/html/head.htm%]
</head>
<body>
    Served at: [%servetime%]
</body>
</html></pre>

In the example you can see the WebPage[?](https://code.google.com/p/sharpfusion/w/edit/WebPage) attribute which will result in a line int the web.config file like this:

<pre> <add verb="*" path="/r/*" type="HelloWorld.Pages.ChatRoom" /></pre>

The template file is defined in the Page class constructor which reads the file to memory and caches it with a file system dependency in place.

<tt>Template.Set("tokenname", object)</tt> adds the object to a dictionary.

The template parses for <tt>[%token%]</tt> and if it matches an item in the dictionary will replace it.

Includes can be done, it will replace <tt>[%inc:/a/html/head.htm%]</tt> with the contents of the file head.htm.

If a token can not be found in the dictionary then it will check the query string for Form Post values, this is helpful for having posted form retain their values after submission.

There is also an append method which will use StringBuilder to concat values, That is helpful for loops like when building a table of values from a dataset.

<h1>Introduction</h1>

<p>Once you have used the SQL gen to create your database you can run /dal and /buildobjects to create objects from your database.</p>

<p>The DAL contains a class for each stored proc and a ResultSet object with a collection of the columns and rows of the result set.</p>

<p>You can use stored procs via the DAL directly</p>

<pre>
using (DAL.Procs.usp_room_sel dal = new DAL.Procs.usp_room_sel())
{
&nbsp; &nbsp; dal.id = ID;
&nbsp; &nbsp; dal.Execute(val);

&nbsp; &nbsp; foreach (DAL.Procs.usp_room_sel.ResultSet1 rs1 in dal.RS1)
&nbsp; &nbsp; { 
&nbsp; &nbsp; &nbsp; &nbsp; this.id = rs1.id; 
&nbsp; &nbsp; &nbsp; &nbsp; this.name = rs1.name; 
&nbsp; &nbsp; &nbsp; &nbsp; if (rs1.posts.HasValue) this.posts = rs1.posts.Value;
&nbsp; &nbsp; &nbsp; &nbsp; template.Append(&quot;rooms&quot;, this.name)
&nbsp; &nbsp; }
}</pre>

<p>Or you can access them via the Objects wrapper:</p>

<pre>
foreach(Room room in new User(1, val).room_user_ids)
{
&nbsp; &nbsp; template.Append(&quot;room&quot;, room.name);
}</pre>

<p>User.room_user_ids runs a proc that selects rooms by foreign key user.id and build a collection of Rooom objects from the result set.</p>

<h1><a name="Build_the_DAL"></a>Build the DAL</h1>

<p>Open the Builders project. Add the connection string. Edit the appSetting key &quot;projectName&quot; to be the name of the project you are working on.</p>

<p>View&nbsp;<em>/dal</em>&nbsp;in a browser. That will open the database listed in the web.config and build a file called DAL.cs in the DataAccessLayer project.</p>

<p>The DAL.cs contains a class for every stored procedure</p>

<h1><a name="Build_the_Objects"></a>Build the Objects</h1>

<p>Now visit&nbsp;<em>/BuildObjects</em></p>

<p>That will create a class for each table in the Project specified in appSettings in the folder /Objects. It is a partial class so that you can add new business logic to the object.</p>

<p>&nbsp;</p>

SQLGen  _How to use the SQL Gen_

Updated Dec 12, 2011 by [easymovet](https://code.google.com/u/easymovet/)

# <a name="Introduction"></a>Introduction

The SQL Gen parses schema written in a pseudo-code like format and creates scripts to create the DB table, indexes, and stored procedures, including selects by FK and even parent or multi column FK's

# <a name="Details"></a>Details

edit the file test.txt in the project root then run the exe in the debug directory. the outputs are placed in the "generatedscripts" directory.

**syntax**

<pre>tablenamme
  columnname datatype [notnull, unique, fk(ref table)]</pre>

The first "table" is the default columns

Validate  _The validate object_

Updated Dec 12, 2011 by [easymovet](https://code.google.com/u/easymovet/)

# <a name="Introduction"></a>Introduction

The validate object is sort of like the system exception handling but for softer errors like validating user content or returning SQL errors.

Every Page class has a validate property and you pass that Validate object around. Once an error occurs any further code that checks the Validate object will be skipped and the error message will be added to the Page.Template dictionary under "error"

There are many handly validation classes in the Validate object.

Test Empty

<pre>val.TestPattern(Form["email_address"], StringType.Email, "Please enter a valid email");</pre>

Custom Test

<pre>user.age = val.Test<long>(Form["age"].ToLong(0) > 18, Form["age"].ToLong(0), "You must be over 18");</pre>
