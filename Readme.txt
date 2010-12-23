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

    * 3.3.2 RTM


Known Bugs/Issues

    * Not updating build when using Flash IDE compilation.
    * fuck ups with package path when tracking new project


Version History


     / 1.0.294 - December 23, 2010
      + Check SVN button is now functional

     / 1.0.290 - December 21, 2010
      + Now AS2 compatible

     / 1.0.285 - December 16, 2010
      + Online check of new version of this plugin

     / 1.0.284 - December 15, 2010
      * SVN Rev number is now taken from the revision of the version file

     / 1.0.283 - December 10, 2010
      * Bug while plugin is loading for the first time is now fixed
      
     / 1.0.269 - December 8, 2010
      + automatic version file discovery and add to tracked project
      
     / 1.0.254 - June 25, 2010
      * major fuckup fixed : version was lost when checking existing version file
      
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