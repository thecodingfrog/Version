using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using WeifenLuo.WinFormsUI.Docking;
using Version.Resources;
using PluginCore.Localization;
using PluginCore.Utilities;
using PluginCore.Managers;
using PluginCore.Helpers;
using PluginCore;
using System.Text.RegularExpressions;
using Version.Helpers;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using System.Net;
using System.Reflection;
using SharpSvn;

namespace Version
{
	public enum Language
	{
		AS2,
		AS3
	}
	
	public class PluginMain : IPlugin
	{
		private String pluginName = "Version";
        private String pluginGuid = "F55FB962-3F2E-4575-845F-2FFAB8A0CC56";
        private String pluginHelp = "www.flashdevelop.org/community/";
        private String pluginDesc = "Version number plugin for FlashDevelop 3";
        private String pluginAuth = "Jean-Louis PERSAT";
        private String settingFilename;
        private String settingBackup;
        private Settings settingObject;
        private DockContent pluginPanel;
        private PluginUI pluginUI;
        private Image pluginImage;
        private decimal __vMajor = 1;
        private decimal __vMinor = 0;
        private decimal __vBuild = 0;
        private int __vRevision;
        private String __vAuthor;
        private String __projectPath;
        private String __packagePath;
        private CompilationModes __lastAction;
		private Language __language;

	    #region Required Properties

        /// <summary>
        /// Name of the plugin
        /// </summary> 
        public String Name
		{
			get { return this.pluginName; }
		}

        /// <summary>
        /// GUID of the plugin
        /// </summary>
        public String Guid
		{
			get { return this.pluginGuid; }
		}

        /// <summary>
        /// Author of the plugin
        /// </summary> 
        public String Author
		{
			get { return this.pluginAuth; }
		}

        /// <summary>
        /// Description of the plugin
        /// </summary> 
        public String Description
		{
			get { return this.pluginDesc; }
		}

        /// <summary>
        /// Web address for help
        /// </summary> 
        public String Help
		{
			get { return this.pluginHelp; }
		}

        /// <summary>
        /// Object that contains the settings
        /// </summary>
        [Browsable(false)]
        public Object Settings
        {
            get { return this.settingObject; }
        }
		
		#endregion
		
		#region Required Methods
		
		/// <summary>
		/// Initializes the plugin
		/// </summary>
		public void Initialize()
		{
            this.InitBasics();
            this.LoadSettings();
            this.AddEventHandlers();
            this.InitLocalization();
            this.CreatePluginPanel();
            this.CreateMenuItem();
        }
		
		/// <summary>
		/// Disposes the plugin
		/// </summary>
		public void Dispose()
		{
            this.SaveSettings();
		}
		
		/// <summary>
		/// Handles the incoming events
		/// </summary>
		public void HandleEvent(Object sender, NotifyEvent e, HandlingPriority prority)
        {
            switch (e.Type)
            {
                // Catches Project change event and display the active project path
                case EventType.Command:
                    string cmd = (e as DataEvent).Action;
                    //MessageBox.Show(cmd);
                    switch (cmd)
                    {
                        case "ProjectManager.Project":
                            IProject project = PluginBase.CurrentProject;
                            if (project == null)
                            {
                                //
                            }
                            else
                            {
                                CheckProject();
                            }
                            break;

                        case "ProjectManager.BuildingProject":
                            __lastAction = CompilationModes.Build;
                            break;

                        case "ProjectManager.TestingProject":
                            __lastAction = CompilationModes.Test;
                            break;
                        
                        case "ProjectManager.BuildFailed":
                            //MessageBox.Show("ProjectManager.BuildFailed");
                            break;
                    }
                    
                    break;
                case EventType.ProcessStart:
                    //MessageBox.Show("EventType.ProcessStart");
                    if (settingObject.AutoIncrement
                            && (settingObject.CompilationMode == CompilationModes.Both
                                || (settingObject.CompilationMode == CompilationModes.Build && __lastAction == CompilationModes.Build)
                                || (settingObject.CompilationMode == CompilationModes.Test && __lastAction == CompilationModes.Test)
                            )
                        )
                        IncrementVersion();
                    break;
                case EventType.ProcessEnd:
                    //MessageBox.Show("EventType.ProcessEnd");
                    string res = (e as TextEvent).Value;
                    //MessageBox.Show(res);
                    //MessageBox.Show(__lastAction.ToString());
                    if (hasCompilationError(res)
                            && settingObject.AutoIncrement
                            && (settingObject.CompilationMode == CompilationModes.Both
                                || (settingObject.CompilationMode == CompilationModes.Build && __lastAction == CompilationModes.Build)
                                || (settingObject.CompilationMode == CompilationModes.Test && __lastAction == CompilationModes.Test)
                            )
                        )
                    {
                        DecrementVersion();
                    }
                    break;
            }
            
		}

        /// <summary>
        /// Determines whether [has compilation error] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// 	<c>true</c> if [has compilation error] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        private bool hasCompilationError(string value)
        {
            RegexOptions options = new RegexOptions();
            options |= RegexOptions.Multiline;
            options |= RegexOptions.IgnoreCase;

            return Regex.IsMatch(value, @"Done\s*(0)", options);
        }

        /// <summary>
        /// Find the version file if exists.
        /// </summary>
        private string FindVersionFile(string __path)
        {
            string[] __filePaths;
            string __foundPath = string.Empty;
            
            //MessageBox.Show(__path);

			__filePaths = Directory.GetFiles(__path, settingObject.ClassName + ".as", SearchOption.AllDirectories);
            foreach (string __item in __filePaths)
            {
				//MessageBox.Show(__item);
                __foundPath = __item;
            }
            return __foundPath;
        }

        /// <summary>
        /// Checks the project.
        /// </summary>
        private void CheckProject()
        {
            IProject project = PluginBase.CurrentProject;
            
            pluginUI.Frame.Text = String.Format(LocaleHelper.GetString("Info.CurrentVersion"), project.Name);
            
            char[] splitchar = { ';' };

            checkConsistency();

            foreach (string __ignoredProjectName in this.settingObject.IgnoredProjects)
            {
                //pluginUI.Debug.Text += "project: " + __ignoredProjectName.Split(splitchar)[0] + "\n";
                if (__ignoredProjectName.Split(splitchar)[0] == project.Name)
                {
                    pluginUI.disableVersion();
                    return;
                }
            }
            
            bool inTrackList = false;

            foreach (string __trackedProject in this.settingObject.TrackedProjects)
            {
                if (__trackedProject.Split(splitchar)[0] == project.Name)
                {
                    pluginUI.enableVersion();
                    inTrackList = true;
                    __projectPath = __trackedProject.Split(splitchar)[1];
                    break;
                }
            }
			
			//MessageBox.Show(inTrackList.ToString());
            
            if (!inTrackList)
            {

                string __foundVersionFilePath = FindVersionFile(GetPath());
                if (__foundVersionFilePath != string.Empty)
                {
                    //MessageBox.Show(GetPath(__foundVersionFilePath));
					trackProject(GetPath(__foundVersionFilePath));
                }
                else
                {
                    if (MessageBox.Show(String.Format(LocaleHelper.GetString("Info.TrackProject"), project.Name), LocaleHelper.GetString("Title.UnTrackedProject"), MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        trackProject(project, inTrackList);
                    }
                    else
                    {
                        ignoreProject(project);
                    }
                }
            }
            else
            {
                trackProject(project, inTrackList);
            }
        }

        private void checkConsistency()
        {
            int __size = this.settingObject.IgnoredProjects.Length;
            string[] __tempArray = new string[0];
            int count = 0;

            for (int i = 0; i < __size; i++)
            {
                if (this.settingObject.IgnoredProjects[i] != string.Empty)
                {
                    count++;
                    Array.Resize<string>(ref __tempArray, count);
                    __tempArray.SetValue(this.settingObject.IgnoredProjects[i], count - 1);
                }
            }

            this.settingObject.Changed -= SettingObjectChanged;
            this.settingObject.IgnoredProjects = new string[__tempArray.Length];
            __tempArray.CopyTo(this.settingObject.IgnoredProjects, 0);
            this.settingObject.Changed += SettingObjectChanged;

            __size = this.settingObject.TrackedProjects.Length;
            __tempArray = new string[0];
            count = 0;

            for (int i = 0; i < __size; i++)
            {
                if (this.settingObject.TrackedProjects[i] != string.Empty)
                {
                    count++;
                    Array.Resize<string>(ref __tempArray, count);
                    __tempArray.SetValue(this.settingObject.TrackedProjects[i], count - 1);
                }
            }

            this.settingObject.Changed -= SettingObjectChanged;
            this.settingObject.TrackedProjects = new string[__tempArray.Length];
            __tempArray.CopyTo(this.settingObject.TrackedProjects, 0);
            this.settingObject.Changed += SettingObjectChanged;
        }

        /// <summary>
        /// Ignores the project.
        /// </summary>
        /// <param name="project">The project.</param>
        private void ignoreProject(IProject project)
        {
            pluginUI.disableVersion();
            string[] tempIgnoredProjects = new string[this.settingObject.IgnoredProjects.Length + 1];
            this.settingObject.IgnoredProjects.CopyTo(tempIgnoredProjects, 0);
            tempIgnoredProjects.SetValue(project.Name, this.settingObject.IgnoredProjects.Length);
            this.settingObject.IgnoredProjects = tempIgnoredProjects;
        }

        /// <summary>
        /// Tracks the project.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="inTrackList">if set to <c>true</c> [in track list].</param>
        private void trackProject(string __path)
        {
            pluginUI.enableVersion();

            string[] tempTrackedProjects = new string[this.settingObject.TrackedProjects.Length + 1];
            this.settingObject.TrackedProjects.CopyTo(tempTrackedProjects, 0);
			tempTrackedProjects.SetValue(PluginBase.CurrentProject.Name + ";" + __path, this.settingObject.TrackedProjects.Length);
            this.settingObject.TrackedProjects = tempTrackedProjects;
			CheckVersionFile();
        }

        private void trackProject(IProject project, bool inTrackList)
        {
            pluginUI.enableVersion();
                    
            if (!inTrackList)
            {
                PathEntryDialog prompt;
                if (PluginBase.CurrentProject.SourcePaths.Length > 0)
                {
                    prompt = new PathEntryDialog(LocaleHelper.GetString("Title.ProjectPath"), LocaleHelper.GetString("Info.Path"), LocaleHelper.GetString("Info.Relative"), PluginBase.CurrentProject.GetAbsolutePath(PluginBase.CurrentProject.SourcePaths[0]), LocaleHelper.GetString("Info.Package"));
                }
                else
                {
                    //MessageBox.Show(PluginBase.CurrentProject.GetAbsolutePath(PluginBase.CurrentProject.ProjectPath).ToString());
                    int __pos = PluginBase.CurrentProject.ProjectPath.LastIndexOf("\\");
                    string __path = PluginBase.CurrentProject.ProjectPath.Substring(0, __pos + 1);
                    //MessageBox.Show(__path);
                    prompt = new PathEntryDialog(LocaleHelper.GetString("Title.ProjectPath"), LocaleHelper.GetString("Info.Path"), LocaleHelper.GetString("Info.Relative"), __path, LocaleHelper.GetString("Info.Package"));
                }
                if (prompt.ShowDialog() == DialogResult.OK)
                {
                    string[] tempTrackedProjects = new string[this.settingObject.TrackedProjects.Length + 1];
                    this.settingObject.TrackedProjects.CopyTo(tempTrackedProjects, 0);
                    
                    __projectPath = (prompt.RelativePath) ? PluginBase.CurrentProject.GetAbsolutePath(prompt.ProjectPath) : prompt.ProjectPath;
                    
                    __packagePath = prompt.PackagePath;

                    tempTrackedProjects.SetValue(project.Name + ";" + __projectPath, this.settingObject.TrackedProjects.Length);
                    this.settingObject.TrackedProjects = tempTrackedProjects;
                    
                    if (!Directory.Exists(__projectPath))
                    {
                        Directory.CreateDirectory(__projectPath);
                    }
                }
            }
            CheckVersionFile();
        }

        private void removeIgnoredProject(IProject __project)
        {
            char[] __splitchar = { ';' };
            string[] __tempIgnoredProjects = new string[this.settingObject.IgnoredProjects.Length - 1];
            string[] __ignoredProjects = this.settingObject.IgnoredProjects;
            int count = 0;
            for (int i = 0; i < this.settingObject.IgnoredProjects.Length; i++)
            {
                if (__ignoredProjects[i].Split(__splitchar)[0] != __project.Name)
                {
                    __tempIgnoredProjects[i] = __ignoredProjects[i];
                    count++;
                }
            }
            if (count == 0)
               __tempIgnoredProjects = null;
            this.settingObject.IgnoredProjects = __tempIgnoredProjects;
        }

        /// <summary>
        /// Checks the version file.
        /// </summary>
        private void CheckVersionFile()
        {
			IProject __project = PluginBase.CurrentProject;
			
			//pluginUI.Debug.Text += "PluginBase.CurrentProject.Name: " + PluginBase.CurrentProject.Name + "\n";
            //pluginUI.Debug.Text += "PluginBase.CurrentProject.SourcePaths: " + PluginBase.CurrentProject.SourcePaths + "\n";
            //pluginUI.Debug.Text += "__projectPath: " + __projectPath + "\n";
            //pluginUI.Debug.Text += "PluginBase.CurrentProject.OutputPathAbsolute: " + PluginBase.CurrentProject.OutputPathAbsolute + "\n";
            

            __vAuthor = CheckAuthorName();

            checkSVN();

			switch (__project.Language)
			{
				case "as2":
					CheckAS2VersionFile();
					__language = Language.AS2;
					break;
				case "as3":
					CheckAS3VersionFile();
					__language = Language.AS3;
					break;
			}
            
            this.pluginUI.Changed -= VersionChanged;
            this.pluginUI.Major = __vMajor;
            this.pluginUI.Minor = __vMinor;
            this.pluginUI.Build = __vBuild;
            this.pluginUI.Changed += VersionChanged;
		}

		private void CheckAS2VersionFile()
		{
			Encoding __encoding = Encoding.GetEncoding((Int32)PluginCore.PluginBase.Settings.DefaultCodePage);

			if (!File.Exists(__projectPath + "\\" + settingObject.ClassName + ".as"))
			{
				if (File.Exists(__projectPath + "\\" + settingObject.OldClassName + ".as") && settingObject.ClassName != settingObject.OldClassName)
				{
					File.Move(__projectPath + "\\" + settingObject.OldClassName + ".as", __projectPath + "\\" + settingObject.ClassName + ".as");
					string sVersionContent = File.ReadAllText(__projectPath + "\\" + settingObject.ClassName + ".as");

					RegexOptions options = new RegexOptions();
					options |= RegexOptions.Multiline;
					options |= RegexOptions.IgnoreCase;

					sVersionContent = Regex.Replace(sVersionContent, @"class " + settingObject.OldClassName, "class " + settingObject.ClassName, options);
					FileHelper.WriteFile(__projectPath + "\\" + settingObject.ClassName + ".as", sVersionContent, __encoding, PluginCore.PluginBase.Settings.SaveUnicodeWithBOM);

					__vMajor = decimal.Parse(getValue(sVersionContent, @"static public var Major:Number = (\d+);"));
					__vMinor = decimal.Parse(getValue(sVersionContent, @"static public var Minor:Number = (\d+);"));
					__vBuild = decimal.Parse(getValue(sVersionContent, @"static public var Build:Number = (\d+);"));
				}
				else
				{
					string sVersionContent = "class " + settingObject.ClassName + "\r\n" +
							"{" + "\r\n" +
							"	static public var Major:Number = 1;" + "\r\n" +
							"	static public var Minor:Number = 0;" + "\r\n" +
							"	static public var Build:Number = 0;" + "\r\n" +
							"	static public var Revision:Number = 0;" + "\r\n" +
							"	static public var Timestamp:String = \"" + DateTime.Now + "\";" + "\r\n" +
							"	static public var Author:String = \"" + CheckAuthorName() + "\";" + "\r\n" +
							"}";
					FileHelper.WriteFile(__projectPath + "\\" + settingObject.ClassName + ".as", sVersionContent, __encoding, PluginCore.PluginBase.Settings.SaveUnicodeWithBOM);

					__vMajor = 1;
					__vMinor = 0;
					__vBuild = 0;
				}
			}
			else
			{
				String sVersionContent = File.ReadAllText(__projectPath + "\\" + settingObject.ClassName + ".as");

				__vMajor = decimal.Parse(getValue(sVersionContent, @"static public var Major:Number = (\d+);"));
				__vMinor = decimal.Parse(getValue(sVersionContent, @"static public var Minor:Number = (\d+);"));
				__vBuild = decimal.Parse(getValue(sVersionContent, @"static public var Build:Number = (\d+);"));
			}
		}

		private void CheckAS3VersionFile()
		{
			Encoding __encoding = Encoding.GetEncoding((Int32)PluginCore.PluginBase.Settings.DefaultCodePage);

			if (!File.Exists(__projectPath + "\\" + settingObject.ClassName + ".as"))
			{
				if (File.Exists(__projectPath + "\\" + settingObject.OldClassName + ".as") && settingObject.ClassName != settingObject.OldClassName)
				{
					File.Move(__projectPath + "\\" + settingObject.OldClassName + ".as", __projectPath + "\\" + settingObject.ClassName + ".as");
					string sVersionContent = File.ReadAllText(__projectPath + "\\" + settingObject.ClassName + ".as");

					RegexOptions options = new RegexOptions();
					options |= RegexOptions.Multiline;
					options |= RegexOptions.IgnoreCase;

					sVersionContent = Regex.Replace(sVersionContent, @"public final class " + settingObject.OldClassName, "public final class " + settingObject.ClassName, options);
					FileHelper.WriteFile(__projectPath + "\\" + settingObject.ClassName + ".as", sVersionContent, __encoding, PluginCore.PluginBase.Settings.SaveUnicodeWithBOM);

					__vMajor = decimal.Parse(getValue(sVersionContent, @"static public const Major:int = (\d+);"));
					__vMinor = decimal.Parse(getValue(sVersionContent, @"static public const Minor:int = (\d+);"));
					__vBuild = decimal.Parse(getValue(sVersionContent, @"static public const Build:int = (\d+);"));
				}
				else
				{
					string sVersionContent = "package " + __packagePath + "\r\n" +
							"{" + "\r\n" +
							"  public final class " + settingObject.ClassName + "\r\n" +
							"  {" + "\r\n" +
							"      static public const Major:int = 1;" + "\r\n" +
							"      static public const Minor:int = 0;" + "\r\n" +
							"      static public const Build:int = 0;" + "\r\n" +
							"      static public const Revision:int = 0;" + "\r\n" +
							"      static public const Timestamp:String = \"" + DateTime.Now + "\";" + "\r\n" +
							"      static public const Author:String = \"" + CheckAuthorName() + "\";" + "\r\n" +
							"  }" + "\r\n" +
							"}";
					FileHelper.WriteFile(__projectPath + "\\" + settingObject.ClassName + ".as", sVersionContent, __encoding, PluginCore.PluginBase.Settings.SaveUnicodeWithBOM);

					__vMajor = 1;
					__vMinor = 0;
					__vBuild = 0;
				}
			}
			else
			{
				String sVersionContent = File.ReadAllText(__projectPath + "\\" + settingObject.ClassName + ".as");

				__vMajor = decimal.Parse(getValue(sVersionContent, @"static public const Major:int = (\d+);"));
				__vMinor = decimal.Parse(getValue(sVersionContent, @"static public const Minor:int = (\d+);"));
				__vBuild = decimal.Parse(getValue(sVersionContent, @"static public const Build:int = (\d+);"));
			}
		}

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="pattern">The pattern.</param>
        /// <returns></returns>
        private string getValue(String content, String pattern)
        {
            MatchCollection mMatches = Regex.Matches(content, pattern);
            //pluginUI.Debug.Text += "matches: " + mMatches.Count + "\n";
            
            GroupCollection gc = mMatches[0].Groups;
            CaptureCollection cc = gc[1].Captures;
            return cc[0].Value;
        }

        /// <summary>
        /// Increments the version.
        /// </summary>
        private void IncrementVersion()
        {
            __vBuild++;
            pluginUI.Build = __vBuild;
        }

        /// <summary>
        /// Increments the version.
        /// </summary>
        private void DecrementVersion()
        {
            __vBuild--;
            if (__vBuild < 0)
                __vBuild = 0;
            pluginUI.Build = __vBuild;
        }
		
		#endregion

        #region Custom Methods
       
        /// <summary>
        /// Initializes important variables
        /// </summary>
        public void InitBasics()
        {
            String dataPath = Path.Combine(PathHelper.DataDir, "Version");
            if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);
            this.settingFilename = Path.Combine(dataPath, "Settings.fdb");
            this.settingBackup = Path.Combine(dataPath, "Settings.bak");
            this.pluginImage = PluginBase.MainForm.FindImage("100");
        }

        /// <summary>
        /// Adds the required event handlers
        /// </summary> 
        public void AddEventHandlers()
        {
            // Set events you want to listen (combine as flags)
            EventManager.AddEventHandler(this, EventType.Command | EventType.ProcessStart | EventType.ProcessEnd);
        }

        /// <summary>
        /// Initializes the localization of the plugin
        /// </summary>
        public void InitLocalization()
        {
            LocaleVersion locale = PluginBase.MainForm.Settings.LocaleVersion;
            switch (locale)
            {
                default : 
                    // Plugins should default to English...
                    LocaleHelper.Initialize(LocaleVersion.en_US);
                    break;
            }
            this.pluginDesc = LocaleHelper.GetString("Info.Description");
        }

        /// <summary>
        /// Creates a menu item for the plugin and adds a ignored key
        /// </summary>
        public void CreateMenuItem()
        {
            ToolStripMenuItem viewMenu = (ToolStripMenuItem)PluginBase.MainForm.FindMenuItem("ViewMenu");
            viewMenu.DropDownItems.Add(new ToolStripMenuItem(LocaleHelper.GetString("Label.ViewMenuItem"), this.pluginImage, new EventHandler(this.OpenPanel), this.settingObject.VersionShortcut));
            PluginBase.MainForm.IgnoredKeys.Add(this.settingObject.VersionShortcut);
        }

        /// <summary>
        /// Creates a plugin panel for the plugin
        /// </summary>
        public void CreatePluginPanel()
        {
			this.pluginUI = new PluginUI(this);
            this.pluginUI.Text = LocaleHelper.GetString("Title.PluginPanel");
            this.pluginPanel = PluginBase.MainForm.CreateDockablePanel(this.pluginUI, this.pluginGuid, this.pluginImage, DockState.DockRight);

			StringBuilder sb = new StringBuilder();
			byte[] buf = new byte[8192];
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://jeanlouis.persat.free.fr/fd/check.php");
			HttpWebResponse response = (HttpWebResponse) request.GetResponse();
			Stream resStream = response.GetResponseStream();
			string tempString = null;
			int count = 0;

			do
			{
				// fill the buffer with data
				count = resStream.Read(buf, 0, buf.Length);

				// make sure we read some data
				if (count != 0)
				{
					// translate from bytes to ASCII text
					tempString = Encoding.ASCII.GetString(buf, 0, count);

					// continue building the string
					sb.Append(tempString);
				}
			}
			while (count > 0);

			Assembly assem = Assembly.GetExecutingAssembly();
			AssemblyName assemName = assem.GetName();
			int result = string.Compare(assemName.Version.ToString(), sb.ToString());
			if (result < 0)
			{
				this.pluginUI.CheckVersion.Text = "A new build (" + sb.ToString() + ") is available!";
				this.pluginUI.CheckVersion.Enabled = true;
			}
			else
			{
				this.pluginUI.CheckVersion.Text = "You have latest build of this plugin";
				this.pluginUI.CheckVersion.Enabled = false;
			}
			
            this.pluginUI.Changed += VersionChanged;
			this.pluginUI.SVNCheck += checkSVN;
        }

        /// <summary>
        /// Loads the plugin settings
        /// </summary>
        public void LoadSettings()
        {
            this.settingObject = new Settings();
			if (!File.Exists(this.settingFilename))
			{
				this.SaveSettings();
			}
			else
			{
				File.Copy(this.settingFilename, this.settingBackup, true);
				Object obj = ObjectSerializer.Deserialize(this.settingFilename, this.settingObject);
				this.settingObject = (Settings)obj;
			}
            this.settingObject.Changed += SettingObjectChanged;
        }

        /// <summary>
        /// Saves the plugin settings
        /// </summary>
        public void SaveSettings()
        {
			if (this.settingObject != null)
				this.settingObject.Changed -= SettingObjectChanged;
			if (this.pluginUI != null)
				this.pluginUI.Changed -= VersionChanged;
			if (this.pluginUI != null)
				this.pluginUI.SVNCheck -= checkSVN;
			
			ObjectSerializer.Serialize(this.settingFilename, this.settingObject);
        }

        /// <summary>
        /// Opens the plugin panel if closed
        /// </summary>
        public void OpenPanel(Object sender, System.EventArgs e)
        {
            this.pluginPanel.Show();
        }

        /// <summary>
        /// Checks the name of the author.
        /// </summary>
        /// <returns></returns>
        public String CheckAuthorName()
        {
            foreach (Argument arg in PluginBase.Settings.CustomArguments)
            {
                if (arg.Key == "DefaultUser")
                {
                    return arg.Value;
                }
            }
            return "";
        }

        /// <summary>
        /// Update the classpath if an important setting has changed
        /// </summary>
        private void SettingObjectChanged(string setting)
        {
            //
            CheckProject();
        }

        /// <summary>
        /// Update the classpath if an important setting has changed
        /// </summary>
        private void VersionChanged(string type)
        {
            switch (type)
            {
                case "TrackIt":
                    //MessageBox.Show("here");
                    IProject __project = PluginBase.CurrentProject;
                    this.settingObject.Changed -= SettingObjectChanged; 
                    removeIgnoredProject(__project);
                    trackProject(__project, false);
                    this.settingObject.Changed += SettingObjectChanged;
                    break;
                default:
                    pluginUI.Debug.Text += "Updating " + settingObject.ClassName + ".as\n";

                    Encoding __encoding = Encoding.GetEncoding((Int32)PluginCore.PluginBase.Settings.DefaultCodePage);

                    __vMajor = pluginUI.Major;
                    __vMinor = pluginUI.Minor;
                    __vBuild = pluginUI.Build;

                    if (File.Exists(__projectPath + "\\" + settingObject.ClassName + ".as"))
                    {
                        String sVersionContent = File.ReadAllText(__projectPath + "\\" + settingObject.ClassName + ".as");
                        RegexOptions options = new RegexOptions();
                        options |= RegexOptions.Multiline;
                        options |= RegexOptions.IgnoreCase;

						if (__language == Language.AS3)
						{
							sVersionContent = Regex.Replace(sVersionContent, @"static public const Major:int = (\d+);", "static public const Major:int = " + __vMajor + ";", options);
							sVersionContent = Regex.Replace(sVersionContent, @"static public const Minor:int = (\d+);", "static public const Minor:int = " + __vMinor + ";", options);
							sVersionContent = Regex.Replace(sVersionContent, @"static public const Build:int = (\d+);", "static public const Build:int = " + __vBuild + ";", options);
							sVersionContent = Regex.Replace(sVersionContent, @"static public const Revision:int = (\d+);", "static public const Revision:int = " + __vRevision + ";", options);

							String pattern = @"static public const Timestamp:String = ""(.*)"";";
							sVersionContent = Regex.Replace(sVersionContent, pattern, "static public const Timestamp:String = \"" + DateTime.Now + "\";", options);

							pattern = @"static public const Author:String = ""(\.*|\w*)"";";
							sVersionContent = Regex.Replace(sVersionContent, pattern, "static public const Author:String = \"" + __vAuthor + "\";", options);
						}
						if (__language == Language.AS2)
						{
							sVersionContent = Regex.Replace(sVersionContent, @"static public var Major:Number = (\d+);", "static public var Major:Number = " + __vMajor + ";", options);
							sVersionContent = Regex.Replace(sVersionContent, @"static public var Minor:Number = (\d+);", "static public var Minor:Number = " + __vMinor + ";", options);
							sVersionContent = Regex.Replace(sVersionContent, @"static public var Build:Number = (\d+);", "static public var Build:Number = " + __vBuild + ";", options);
							sVersionContent = Regex.Replace(sVersionContent, @"static public var Revision:Number = (\d+);", "static public var Revision:Number = " + __vRevision + ";", options);

							String pattern = @"static public var Timestamp:String = ""(.*)"";";
							sVersionContent = Regex.Replace(sVersionContent, pattern, "static public var Timestamp:String = \"" + DateTime.Now + "\";", options);

							pattern = @"static public var Author:String = ""(\.*|\w*)"";";
							sVersionContent = Regex.Replace(sVersionContent, pattern, "static public var Author:String = \"" + __vAuthor + "\";", options);
						}

                        FileHelper.WriteFile(__projectPath + "\\" + settingObject.ClassName + ".as", sVersionContent, __encoding, PluginCore.PluginBase.Settings.SaveUnicodeWithBOM);
                    }

                    string __projectBaseDir = PluginBase.CurrentProject.ProjectPath;
                    if (__projectBaseDir.IndexOf("\\") > -1)
                    {
                        __projectBaseDir = __projectBaseDir.Substring(0, __projectBaseDir.LastIndexOf("\\") + 1);
                        if (File.Exists(__projectBaseDir + "\\application.xml"))
                        {
                            XmlDocument __xmlDoc = new XmlDocument();
                            __xmlDoc.Load(__projectBaseDir + "\\application.xml");
                            XmlNamespaceManager __NsMgr = new XmlNamespaceManager(__xmlDoc.NameTable);
                            __NsMgr.AddNamespace("air", "http://ns.adobe.com/air/application/1.5");

                            XmlNode __root = __xmlDoc.DocumentElement;
                            XmlNode __nodeVersion = __root.SelectSingleNode("/air:application/air:version", __NsMgr);
                            if (__nodeVersion != null)
                            {
                                __nodeVersion.InnerText = __vMajor + "." + __vMinor + "." + __vBuild;
                            }
                            __xmlDoc.Save(__projectBaseDir + "\\application.xml");
                        }
                    }
                    break;
            }

        }

        /// <summary>
        /// Checks the SVN.
        /// </summary>
        private void checkSVN()
        {
			SvnClient __svnClient = new SvnClient();
            SvnInfoEventArgs __sieaInfo;
            bool __getInfo;
            if (__projectPath != "")
            {
                try
                {
					__getInfo = __svnClient.GetInfo(SvnPathTarget.FromString(__projectPath + "\\" + settingObject.ClassName + ".as"), out __sieaInfo);
                    pluginUI.Revision.Text = __sieaInfo.LastChangeRevision.ToString();
                    __vRevision = (int)__sieaInfo.LastChangeRevision;
                }
                catch (SvnException e)
                {
                    __vRevision = 0;
                    pluginUI.Revision.Text = "";
                }
                /*catch (SvnClientException e)
                {
                    //
                }*/
            }
            else
            {
                __vRevision = 0;
                pluginUI.Revision.Text = "";
            }
        }

        private string GetPath()
        {
            return GetPath(PluginBase.CurrentProject.ProjectPath);
        }

        private string GetPath(string __path)
        {
            int __pos = __path.LastIndexOf("\\");
            string __realpath = __path.Substring(0, __pos + 1);
            return __realpath;
        }

		#endregion

	}
	
}
