using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Collections.Specialized;


namespace Version
{
    public delegate void SettingChangeHandler(string setting);
    
    public enum CompilationModes
    {
        Both = 0,
        Build = 1,
        Test = 2
    }

    [Serializable]
    public class Settings
    {
        public event SettingChangeHandler Changed;

        const string DEFAULT_CLASSNAME = "Version";
        const Keys DEFAULT_SHORTCUT = Keys.Control | Keys.Shift | Keys.V;
        const bool DEFAULT_AUTOINCREMENT = true;
        const CompilationModes DEFAULT_COMPILATIONMODE = 0;
        
        private string __oldClassName = DEFAULT_CLASSNAME;
        private string __className = DEFAULT_CLASSNAME;
        private Keys __versionShortcut = DEFAULT_SHORTCUT;
        private bool __autoIncrement = DEFAULT_AUTOINCREMENT;
        private CompilationModes __compilationMode = DEFAULT_COMPILATIONMODE;
        private string[] __trackedProjects = new string[] { };
        private string[] __ignoredProjects = new string[] { };

        [Browsable(false)]
        public string OldClassName
        {
            get { return this.__oldClassName; }
            set {  }
        }

        /// <summary>
        /// Gets or sets the name of the class.
        /// </summary>
        /// <value>The name of the class.</value>
        [DisplayName("Class name"), DefaultValue(DEFAULT_CLASSNAME)]
        public string ClassName 
        {
            get { return this.__className; }
            set {
                this.__oldClassName = this.__className;
                this.__className = value;
            }
        }

        /// <summary> 
        /// Get and sets the sampleShortcut
        /// </summary>
        [DisplayName("Shortcut"), DefaultValue(DEFAULT_SHORTCUT)]
        public Keys VersionShortcut
        {
            get { return this.__versionShortcut; }
            set { this.__versionShortcut = value; }
        }

        /// <summary> 
        /// Get and sets the trackedProjects
        /// </summary>
        [DisplayName("Tracked Projects")]
        public string[] TrackedProjects
        {
            get { return this.__trackedProjects; }
            set
            {
                this.__trackedProjects = value;
                FireChanged("trackedProjects");
            }
        }

        /// <summary> 
        /// Get and sets the ignoredProjects
        /// </summary>
        [DisplayName("Ignored Projects")]
        public string[] IgnoredProjects
        {
            get { return this.__ignoredProjects; }
            set
            {
                this.__ignoredProjects = value;
                FireChanged("ignoredProjects");
            }
        }

        /// <summary> 
        /// Get and sets the ignoredProjects
        /// </summary>
        [DisplayName("Auto increment build"), DefaultValue(DEFAULT_AUTOINCREMENT)]
        public bool AutoIncrement
        {
            get { return this.__autoIncrement; }
            set
            {
                this.__autoIncrement = value;
                //FireChanged("autoIncrement");
            }
        }

        /// <summary> 
        /// Get and sets the ignoredProjects
        /// </summary>
        [DisplayName("Action required to update version"), DefaultValue(DEFAULT_COMPILATIONMODE)]
        public CompilationModes CompilationMode
        {
            get { return this.__compilationMode; }
            set
            {
                this.__compilationMode = value;
                //FireChanged("compilationMode");
            }
        }

        private void FireChanged(string setting)
        {
            if (Changed != null)
                Changed(setting);
        }

    }

}
