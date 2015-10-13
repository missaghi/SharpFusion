The framework consists of 4 major parts: The SQL generator, the Data Access Layer, the Templating System, and automatic url end-point mapping to eliminate the need for runtime reflection.

Usage Guides:

-Pages
-Object Relational Mapper
-SQL DDL Generator
- Validation tools

**Pages** Instead of using .aspx pages that muddled the tiers we wanted to use html templates with just tokens, both for performance and to further separate presentation layer which enhances the ability to separate the duties of the developers.

The overarching model is to build each layer as a service to other layers, that means that your front end layer ends up being a stand alone app that consumes your web app via an API. The side effect is that you don't have to make a second project out of exposing an API. This also forces the application to consider security in a more uniform way.

Features Ajax test pages Auto sitemap

**Object Relational Mappe**r To connect to databases we wanted simple objects to be created from tables using the lightest methods, ie: no data-adapters, no dynamic SQL.

Features: Completely open classes. Object oriented allows for easy customization

SQL DDL Generator The SQL generator takes a psudo-code like syntax and generates SQL DML and many handy stored procs, including selects by foreign keys.

Features: Index control select by cascading FK

Other Features
-Many Extention classes, eg (String ellipsis, Simple Encryption, Search Arrays, Parse CSV, Serialize to JSON, Parse URL parts, Else values )
-Image utilities like resizing and cropping
-Caching wrappers
-Base36 conversion (base31 to eliminate easily misread letters eg 5 and S )
