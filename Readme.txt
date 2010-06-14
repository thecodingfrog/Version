Description
This plugin will generate a Version.as file at compile time with major, minor, build, SVN revision, timestamp and author informations.

What Version do:

    * Update Versions.as file with :
		- major version
		- minor version
		- build version
		- SVN revision
		- timestamp
		- author
    * Increments build number at compile time if compilation succeed
    * Change class name of version file


What Version does NOT do:

    * Compile with the next commit revision number


Thanks in advance to anyone who tries it out!


Tested on FlashDevelop versions:

    * 3.2.1 RTM


Known Bugs/Issues

    * Not updating build when using Flash IDE compilation.


Version History

     / 1.0.252 - June 14, 2010
      + Check consistency of ignored projects and tracked projetcs lists
      
     / 1.0.188 - June 3, 2010
      + Link in panel to track project

     / 1.0.137 - May 10, 2010
      + Relative now allowed when project added to tracked projects
    
     / 1.0.124 - May 4, 2010
      + New setting to allow update at testing, build or both
    
     / 1.0.123 - April 27, 2010
      * Implementation of SharpSvn instead of homemade trick to read revision
      * Change the maximum value for major, minor and build NumericUpDown controls
    
     / 1.0.112 - April 26, 2010
      * Version file is now encoded using default encoding
      + New feature to support AIR application.xml file update
    
    / 1.0.105 - April 8, 2010
      + New setting to disable auto increment build number
    
    / 1.0.103 - March 31, 2010
      + New setting to define class name of version file
    
    / 1.0.96 - March 29, 2010
      * Fix a regexp problem with date format that didn't update timestamp
      * Fix a no reset of build values when creating a new project
      
    / 1.0.75 - March, 2010
      - initial implementation of Version
      

Sources

    * http://thecodingfrog.googlecode.com