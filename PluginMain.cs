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
        private String pluginDesc = "Version number plugin for FlashDevelop 4";
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
        private int __vRevision = 0;
        private String __vAuthor;
        private String __versionFilePath;
		private String __packagePath;
		private bool __isVersionned = false;
        private CompilationModes __lastAction;
		private Language __language = Language.AS3;

	    #region Required Properties

		/// <summary>
		/// Api level of the plugin
		/// </summary>
		public Int32 Api
		{
			get { return 1; }
		}
		
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
                            __lastAction = CompilationModes.BuildProject;
                            break;

                        case "ProjectManager.TestingProject":
                            __lastAction = CompilationModes.TestMovie;
                            break;
                        
                        case "ProjectManager.BuildFailed":
                            //MessageBox.Show("ProjectManager.BuildFailed");
                            break;
                    }
                    
                    break;
                case EventType.ProcessStart:
                    //MessageBox.Show("EventType.ProcessStart");
					if (settingObject.AutoIncrement
							&& __isVersionned
                            && (settingObject.CompilationMode == CompilationModes.Both
                                || (settingObject.CompilationMode == CompilationModes.BuildProject && __lastAction == CompilationModes.BuildProject)
                                || (settingObject.CompilationMode == CompilationModes.TestMovie && __lastAction == CompilationModes.TestMovie)
                            )
                        )
						IncrementVersion();
                    break;
                case EventType.ProcessEnd:
                    //MessageBox.Show("EventType.ProcessEnd");
                    string res = (e as TextEvent).Value;
                    //MessageBox.Show(hasNoCompilationError(res).ToString());
                    //MessageBox.Show(__lastAction.ToString());
                    if (!hasNoCompilationError(res)
							&& __isVersionned
                            && settingObject.AutoIncrement
                            && (settingObject.CompilationMode == CompilationModes.Both
                                || (settingObject.CompilationMode == CompilationModes.BuildProject && __lastAction == CompilationModes.BuildProject)
                                || (settingObject.CompilationMode == CompilationModes.TestMovie && __lastAction == CompilationModes.TestMovie)
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
        private bool hasNoCompilationError(string __value)
        {
            RegexOptions options = new RegexOptions();
            options |= RegexOptions.Multiline;
            options |= RegexOptions.IgnoreCase;

            return Regex.IsMatch(__value, @"Done\s*\(0\)", options);
        }

        /// <summary>
        /// Find the version file if exists.
        /// </summary>
        private string FindVersionFile(string __path)
        {
            string[] __filePaths;
            string __foundPath = string.Empty;
            
            __filePaths = Directory.GetFiles(__path, settingObject.ClassName + ".as", SearchOption.AllDirectories);
            foreach (string __item in __filePaths)
            {
				__foundPath = __item;
            }
            return __foundPath;
        }

        /// <summary>
        /// Checks the project.
        /// </summary>
        private void CheckProject()
        {
            IProject __project = PluginBase.CurrentProject;
			
			switch (__project.Language.ToLower())
			{
				case "as2":
					__language = Language.AS2;
					break;
				case "as3":
				default:
					__language = Language.AS3;
					break;
			}

			__vAuthor = CheckAuthorName();
			
            pluginUI.Frame.Text = String.Format(LocaleHelper.GetString("Info.CurrentVersion"), __project.Name);
            
            char[] splitchar = { ';' };

            checkConsistency();

            foreach (string __ignoredProjectName in this.settingObject.IgnoredProjects)
            {
                //pluginUI.Debug.Text += "project: " + __ignoredProjectName.Split(splitchar)[0] + "\n";
                if (__ignoredProjectName.Split(splitchar)[0] == __project.Name)
                {
                    pluginUI.disableVersion();
					__isVersionned = false;
                    return;
                }
            }

			if (File.Exists(GetPath() + "\\obj\\Version.xml"))
			{
				pluginUI.enableVersion();
				__isVersionned = true;
			}
			else
			{
				string __foundVersionFilePath = FindVersionFile(GetPath());
				//MessageBox.Show(__foundVersionFilePath);
				if (__foundVersionFilePath != string.Empty)
				{
					pluginUI.enableVersion();
					__isVersionned = true;
					ReadVersionFile(__foundVersionFilePath);
					__versionFilePath = __foundVersionFilePath.Substring(GetSourcePath().Length);
					if (__versionFilePath.IndexOf(settingObject.ClassName + ".as") > -1)
					{
						__versionFilePath = __versionFilePath.Substring(0, __versionFilePath.Length - (settingObject.ClassName + ".as").Length);
						__versionFilePath = __versionFilePath.Replace("\\", "/");
					}
					SaveVersionXml();
					ReadVersionXml();
				}
				else
				{
					__isVersionned = false;
				}
			}
			if (!__isVersionned)
			{
				if (MessageBox.Show(String.Format(LocaleHelper.GetString("Info.TrackProject"), __project.Name), LocaleHelper.GetString("Title.UnTrackedProject"), MessageBoxButtons.YesNo) == DialogResult.Yes)
				{
					trackProject(__isVersionned);
				}
				else
				{
					ignoreProject(__project);
				}
			}
			else
			{
				CheckVersionFile();
				//trackProject(__isVersionned);
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
        }

        /// <summary>
        /// Ignores the project.
        /// </summary>
        /// <param name="project">The project.</param>
        private void ignoreProject(IProject __project)
        {
            pluginUI.disableVersion();
			__isVersionned = false;
            string[] tempIgnoredProjects = new string[this.settingObject.IgnoredProjects.Length + 1];
            this.settingObject.IgnoredProjects.CopyTo(tempIgnoredProjects, 0);
			tempIgnoredProjects.SetValue(__project.Name, this.settingObject.IgnoredProjects.Length);
            this.settingObject.IgnoredProjects = tempIgnoredProjects;
        }

        private void trackProject(bool __versionned)
        {
			string __sourcePath;

            pluginUI.enableVersion();

			if (!__versionned)
			{
				PathEntryDialog prompt;

				__sourcePath = GetSourcePath();
				prompt = new PathEntryDialog(String.Format(LocaleHelper.GetString("Title.ProjectPath"), settingObject.ClassName + ".as"), LocaleHelper.GetString("Info.Path"), LocaleHelper.GetString("Info.Relative"), GetPath(), __sourcePath, LocaleHelper.GetString("Info.Package"));

				if (prompt.ShowDialog() == DialogResult.OK)
				{
					__versionFilePath = prompt.RelativePath;
					__versionFilePath = __versionFilePath.Replace("\\", "/");

					__packagePath = prompt.PackagePath;

					if (!Directory.Exists(__sourcePath + __versionFilePath))
					{
						Directory.CreateDirectory(__sourcePath + __versionFilePath);
					}

					__isVersionned = true;
					CheckVersionFile();
					CreateVersionFile(__vMajor, __vMinor, __vBuild, __vRevision, DateTime.Now, __vAuthor);
				}
				else
				{
					IProject __project = PluginBase.CurrentProject;
					ignoreProject(__project);
				}
			}
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
			//pluginUI.Debug.Text += "PluginBase.CurrentProject.Name: " + PluginBase.CurrentProject.Name + "\n";
            //pluginUI.Debug.Text += "PluginBase.CurrentProject.SourcePaths: " + PluginBase.CurrentProject.SourcePaths + "\n";
            //pluginUI.Debug.Text += "__projectPath: " + __projectPath + "\n";
            //pluginUI.Debug.Text += "PluginBase.CurrentProject.OutputPathAbsolute: " + PluginBase.CurrentProject.OutputPathAbsolute + "\n";
            ReadVersionXml();
			checkSVN();
		}

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="pattern">The pattern.</param>
        /// <returns></returns>
		private string getValue(String __content, String __pattern)
		{
			return getValue(__content, __pattern, 0);
		}

        private string getValue(String __content, String __pattern, int __idx)
        {
			//MessageBox.Show(__pattern);
			
			MatchCollection __matches = Regex.Matches(__content, __pattern);
			//MessageBox.Show(__matches.Count.ToString());
            //pluginUI.Debug.Text += "matches: " + mMatches.Count + "\n";
			if (__matches.Count > 0)
			{
				GroupCollection __gc = __matches[0].Groups;
				//MessageBox.Show(__gc[_idx + 1].ToString());
				CaptureCollection __cc = __gc[__idx + 1].Captures;
				//MessageBox.Show(__cc.Count.ToString());
				//MessageBox.Show(__cc.Count.ToString() + ":" + __cc[__idx].Value);
				return __cc[0].Value;
			}
			else
			{
				return string.Empty;
			}
        }

        /// <summary>
        /// Increments the version.
        /// </summary>
        private void IncrementVersion()
        {
			__vBuild++;
			checkSVN();
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

			try
			{
				StringBuilder sb = new StringBuilder();
				byte[] buf = new byte[8192];
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://jeanlouis.persat.free.fr/fd/check.php");
				HttpWebResponse response = (HttpWebResponse)request.GetResponse();
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
				string __currentVersion = assemName.Version.Major.ToString() + assemName.Version.Minor.ToString() + assemName.Version.Build.ToString();
				__currentVersion = __currentVersion.Replace(".", "");

				string __availableVersion = sb.ToString().Replace(".", "");

				int result = decimal.Compare(Convert.ToDecimal(__currentVersion), Convert.ToDecimal(__availableVersion));
				if (result < 0)
				{
					this.pluginUI.CheckVersion.Text = String.Format(LocaleHelper.GetString("Info.New"), sb.ToString());
					this.pluginUI.CheckVersion.Enabled = true;
				}
				else
				{
					this.pluginUI.CheckVersion.Text = LocaleHelper.GetString("Info.Latest");
					this.pluginUI.CheckVersion.Enabled = false;
				}
			}
			catch (Exception ex)
			{
				this.pluginUI.CheckVersion.Text = LocaleHelper.GetString("Info.Inaccessible");
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
            foreach (Argument arg in PluginBase.MainForm.CustomArguments)
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
        private void VersionChanged(string __type)
        {
			switch (__type)
            {
                case "TrackIt":
                    IProject __project = PluginBase.CurrentProject;
                    this.settingObject.Changed -= SettingObjectChanged; 
                    removeIgnoredProject(__project);
                    trackProject(false);
                    this.settingObject.Changed += SettingObjectChanged;
                    break;
                default:
                    pluginUI.Debug.Text += "Updating " + settingObject.ClassName + ".as\n";

                    Encoding __encoding = Encoding.GetEncoding((Int32)PluginCore.PluginBase.Settings.DefaultCodePage);

                    __vMajor = pluginUI.Major;
                    __vMinor = pluginUI.Minor;
                    __vBuild = pluginUI.Build;

					SaveVersionXml();
					CreateVersionFile(__vMajor, __vMinor, __vBuild, __vRevision, DateTime.Now, __vAuthor);
                    break;
            }
			
        }

		private string GetNamespaceVersion(XmlDocument __xmlDoc)
		{
			string __result;
			try
			{
				string __ns = __xmlDoc.DocumentElement.NamespaceURI.ToLower();
				__result = __ns.Replace("http://ns.adobe.com/air/application/", "");
			}
			catch (Exception ex)
			{
				__result = "1.5";
			}
			return __result;
		}

		private void ReadVersionXml()
		{
			if (!File.Exists(GetPath() + "\\obj\\Version.xml"))
			{
				__vMajor = 1;
				__vMinor = 0;
				__vBuild = 0;
				SaveVersionXml();
			}
			else
			{
				__vAuthor = CheckAuthorName();
				
				XmlDocument __xmlDoc = new XmlDocument();
				__xmlDoc.Load(GetPath() + "\\obj\\Version.xml");

				XmlNode __root = __xmlDoc.DocumentElement;
				
				XmlNode __nodeversion = __root.SelectSingleNode("//version");
				
				__versionFilePath = __nodeversion.Attributes["path"].Value;
				__packagePath = __nodeversion.Attributes["package"].Value;
				//MessageBox.Show(__versionFilePath);

				XmlNode __nodeMajor = __root.SelectSingleNode("//major");
				__vMajor = Convert.ToDecimal(__nodeMajor.InnerText);

				XmlNode __nodeMinor = __root.SelectSingleNode("//minor");
				__vMinor = Convert.ToDecimal(__nodeMinor.InnerText);

				XmlNode __nodeBuild = __root.SelectSingleNode("//build");
				__vBuild = Convert.ToDecimal(__nodeBuild.InnerText);

				checkSVN();
			}

			this.pluginUI.Changed -= VersionChanged;
			this.pluginUI.Major = __vMajor;
			this.pluginUI.Minor = __vMinor;
			this.pluginUI.Build = __vBuild;
			this.pluginUI.Changed += VersionChanged;

		}

		private void SaveVersionXml()
		{
			decimal __tempvBuild = __vBuild;

			if (!Directory.Exists(GetPath() + "\\obj"))
				Directory.CreateDirectory(GetPath() + "\\obj");

			XmlTextWriter __xmlTW = new XmlTextWriter(GetPath() + "\\obj\\Version.xml", null);
			__xmlTW.Formatting = Formatting.Indented;
			__xmlTW.WriteStartDocument(false);
			__xmlTW.WriteStartElement("version");
			__xmlTW.WriteAttributeString("path", __versionFilePath);
			__xmlTW.WriteAttributeString("package", __packagePath);
			__xmlTW.WriteElementString("major", __vMajor.ToString());
			__xmlTW.WriteElementString("minor", __vMinor.ToString());
			__xmlTW.WriteElementString("build", __vBuild.ToString());
			__xmlTW.WriteElementString("revision", __vRevision.ToString());
			__xmlTW.WriteElementString("author", __vAuthor);
			__xmlTW.WriteElementString("timestamp", DateTime.Now.ToString());
			__xmlTW.WriteEndElement();
			__xmlTW.Flush();
			__xmlTW.Close();
			if (__xmlTW != null)
				__xmlTW.Close();

			string __projectBaseDir = PluginBase.CurrentProject.ProjectPath;
			if (__projectBaseDir.IndexOf("\\") > -1)
			{
				__projectBaseDir = GetPath();
				if (File.Exists(__projectBaseDir + "\\application.xml"))
				{
					XmlDocument __xmlDoc = new XmlDocument();
					__xmlDoc.Load(__projectBaseDir + "\\application.xml");
					string __xmlNV = GetNamespaceVersion(__xmlDoc);

					XmlNamespaceManager __NsMgr = new XmlNamespaceManager(__xmlDoc.NameTable);
					__NsMgr.AddNamespace("air", "http://ns.adobe.com/air/application/" + __xmlNV);

					XmlNode __root = __xmlDoc.DocumentElement;
					XmlNode __nodeVersion = __root.SelectSingleNode("/air:application/air:version", __NsMgr);
					if (__vBuild > 999)
						__tempvBuild = 999;
					if (__nodeVersion != null)
					{
						__nodeVersion.InnerText = __vMajor + "." + __vMinor + "." + __tempvBuild;
					}
					__nodeVersion = __root.SelectSingleNode("/air:application/air:versionNumber", __NsMgr);
					if (__nodeVersion != null)
					{
						__nodeVersion.InnerText = __vMajor + "." + __vMinor + "." + __tempvBuild;
					}
					__xmlDoc.Save(__projectBaseDir + "\\application.xml");
				}
			}
		}

		private void CreateVersionFile(decimal major, decimal minor, decimal build, int revision, DateTime timestamp, string author)
		{
			Encoding __encoding = Encoding.GetEncoding((Int32)PluginCore.PluginBase.Settings.DefaultCodePage);
			string sVersionContent = string.Empty;
			
			switch (__language)
			{
				case Language.AS2:
					sVersionContent = "class " + BuildAS2PackagePath(__packagePath) + settingObject.ClassName + "\r\n" +
						"{" + "\r\n" +
						"	public static var Major:Number = " + major + ";\r\n" +
						"	public static var Minor:Number = " + minor + ";\r\n" +
						"	public static var Build:Number = " + build + ";\r\n" +
						"	public static var Revision:Number = " + revision + ";\r\n" +
						"	public static var Timestamp:String = \"" + timestamp.ToString() + "\";\r\n" +
						"	public static var Author:String = \"" + author + "\";\r\n" +
						"}";
					break;
				case Language.AS3:
				default:
					sVersionContent = "package " + __packagePath + "\r\n" +
						 "{" + "\r\n" +
						 "  public final class " + settingObject.ClassName + "\r\n" +
						 "  {" + "\r\n" +
						 "      public static const Major:int = " + major + ";\r\n" +
						 "      public static const Minor:int = " + minor + ";\r\n" +
						 "      public static const Build:int = " + build + ";\r\n" +
						 "      public static const Revision:int = " + revision + ";\r\n" +
						 "      public static const Timestamp:String = \"" + timestamp + "\";\r\n" +
						 "      public static const Author:String = \"" + author + "\";\r\n" +
						 "  }" + "\r\n" +
						 "}";
					break;
			}


			if (!Directory.Exists(GetSourcePath() + __versionFilePath))
			{
				Directory.CreateDirectory(GetSourcePath() + __versionFilePath);
			}

			FileHelper.WriteFile(GetSourcePath() + __versionFilePath + "\\" + settingObject.ClassName + ".as", sVersionContent, __encoding, PluginCore.PluginBase.Settings.SaveUnicodeWithBOM);
		}

		private void ReadVersionFile(string __path)
		{
			string __fileContent = string.Empty;

			__fileContent = File.ReadAllText(__path);
			
			switch (__language)
			{
				case Language.AS2:
					__packagePath = getValue(__fileContent, @"class ([\w.]*)" + settingObject.ClassName);
					__vMajor = decimal.Parse(getValue(__fileContent, @"(static public|public static) var Major:Number = (\d+);", 1));
					__vMinor = decimal.Parse(getValue(__fileContent, @"(static public|public static) var Minor:Number = (\d+);", 1));
					__vBuild = decimal.Parse(getValue(__fileContent, @"(static public|public static) var Build:Number = (\d+);", 1));
					break;
				case Language.AS3:
				default:
					__packagePath = getValue(__fileContent, @"package ([\w.]*)");
					__vMajor = decimal.Parse(getValue(__fileContent, @"(static public|public static) const Major:int = (\d+);", 1));
					__vMinor = decimal.Parse(getValue(__fileContent, @"(static public|public static) const Minor:int = (\d+);", 1));
					__vBuild = decimal.Parse(getValue(__fileContent, @"(static public|public static) const Build:int = (\d+);", 1));
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
			
			if (__versionFilePath != "")
            {
				try
				{
					__getInfo = __svnClient.GetInfo(SvnPathTarget.FromString(GetSourcePath() + __versionFilePath + "\\" + settingObject.ClassName + ".as"), out __sieaInfo);
					pluginUI.Revision.Text = __sieaInfo.LastChangeRevision.ToString();
					__vRevision = (int)__sieaInfo.LastChangeRevision;
				}
				catch (SvnException e)
				{
					try
					{
						__getInfo = __svnClient.GetInfo(SvnPathTarget.FromString(GetSourcePath() + __versionFilePath), out __sieaInfo);
						pluginUI.Revision.Text = __sieaInfo.LastChangeRevision.ToString();
						__vRevision = (int)__sieaInfo.LastChangeRevision;
					}
					catch (SvnException ex)
					{
						__vRevision = 0;
						pluginUI.Revision.Text = "";
					}
				}
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

		private string GetSourcePath()
		{
			string __sourcePath = string.Empty;

			if (PluginBase.CurrentProject.SourcePaths.Length > 0)
			{
				__sourcePath = PluginBase.CurrentProject.GetAbsolutePath(PluginBase.CurrentProject.SourcePaths[0]);
			}
			else
			{
				__sourcePath = GetPath();
			}

			return __sourcePath;
		}

		private string BuildAS2PackagePath(string __path)
		{
			string __newpath = string.Empty;
			if (__path.Length > 0)
				__newpath = __path + ".";
			return __newpath;

		}

		#endregion

	}
	
}
