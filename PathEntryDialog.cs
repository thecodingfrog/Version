using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using PluginCore.Localization;

namespace Version.Helpers
{
	/// <summary>
	/// A simple form where a user can enter a text string.
	/// </summary>
	public class PathEntryDialog : System.Windows.Forms.Form
	{
		private string __RelativeProjectPath = string.Empty;
		private string __ProjectPath;
		private string __SourcePath;
		private string __PackagePath;
		private string __PathTitle;

		#region Form Designer Components

		private System.Windows.Forms.TextBox pathTextBox;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
        private Label packageLabel;
		private TextBox packageTextBox;
		private System.Windows.Forms.Label pathLabel;

		#endregion

		/// <summary>
        /// Gets the line entered by the user.
        /// </summary>
        public string PackagePath
        {
            get { return __PackagePath; }
        }

        /// <summary>
        /// Gets the line entered by the user.
        /// </summary>
        public string RelativePath
        {
			get { return __RelativeProjectPath; }
        }

        public PathEntryDialog(string captionText, string pathTitle, string relativeTitle, string projectPath, string sourcePath, string packageTitle)
		{
			InitializeComponent();
            InititalizeLocalization();
            this.Font = PluginCore.PluginBase.Settings.DefaultFont;

			__PathTitle = pathTitle;
			__ProjectPath = projectPath;
			__SourcePath = sourcePath;

			__RelativeProjectPath = __SourcePath.Substring(__ProjectPath.Length);

			this.Text = captionText;
			pathLabel.Text = pathTitle + __SourcePath;
			
            pathTextBox.Text = string.Empty;
			pathTextBox.SelectAll();
			pathTextBox.Focus();

            packageLabel.Text = packageTitle;
			//packageTextBox.Text = FormatPackage(__RelativeProjectPath);
		}

		#region Dispose

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#endregion

		#region Windows Form Designer Generated Code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.pathLabel = new System.Windows.Forms.Label();
			this.pathTextBox = new System.Windows.Forms.TextBox();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.packageLabel = new System.Windows.Forms.Label();
			this.packageTextBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// pathLabel
			// 
			this.pathLabel.Location = new System.Drawing.Point(8, 9);
			this.pathLabel.Name = "pathLabel";
			this.pathLabel.Size = new System.Drawing.Size(405, 15);
			this.pathLabel.TabIndex = 3;
			this.pathLabel.Text = "Enter text:";
			// 
			// pathTextBox
			// 
			this.pathTextBox.Location = new System.Drawing.Point(10, 24);
			this.pathTextBox.Name = "pathTextBox";
			this.pathTextBox.Size = new System.Drawing.Size(403, 20);
			this.pathTextBox.TabIndex = 0;
			this.pathTextBox.TextChanged += new System.EventHandler(this.pathTextBox_TextChanged);
			// 
			// btnOK
			// 
			this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnOK.Location = new System.Drawing.Point(263, 100);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(72, 21);
			this.btnOK.TabIndex = 1;
			this.btnOK.Text = "OK";
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnCancel.Location = new System.Drawing.Point(341, 100);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(72, 21);
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// packageLabel
			// 
			this.packageLabel.Location = new System.Drawing.Point(12, 57);
			this.packageLabel.Name = "packageLabel";
			this.packageLabel.Size = new System.Drawing.Size(403, 14);
			this.packageLabel.TabIndex = 4;
			this.packageLabel.Text = "Enter text:";
			// 
			// packageTextBox
			// 
			this.packageTextBox.Location = new System.Drawing.Point(12, 74);
			this.packageTextBox.Name = "packageTextBox";
			this.packageTextBox.Size = new System.Drawing.Size(403, 20);
			this.packageTextBox.TabIndex = 5;
			// 
			// PathEntryDialog
			// 
			this.AcceptButton = this.btnOK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(424, 133);
			this.Controls.Add(this.packageTextBox);
			this.Controls.Add(this.packageLabel);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.pathTextBox);
			this.Controls.Add(this.pathLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PathEntryDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Enter Text";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

        private void InititalizeLocalization()
        {
            this.btnOK.Text = "OK";
            this.btnCancel.Text = "Cancel";
            this.pathLabel.Text = "EnterText";
            this.Text = " " + "EnterText";
        }

		private void btnOK_Click(object sender, System.EventArgs e)
		{
            this.__ProjectPath = pathTextBox.Text.Replace("/", "\\");
            this.__PackagePath = packageTextBox.Text;
            CancelEventArgs cancelArgs = new CancelEventArgs(false);
			OnValidating(cancelArgs);
			if (!cancelArgs.Cancel)
			{
				this.DialogResult = DialogResult.OK;
				this.Close();
			}
		}

		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

        public void SelectRange(int start, int length)
        {
            pathTextBox.Select(start, length);
        }

		private String FormatPackage(string __path)
        {
			string __packagePath = __path;
			
			//MessageBox.Show(__newpath);
			if (__packagePath.Length > 0)
            {
				if (__packagePath.StartsWith("\\"))
					__packagePath = __packagePath.Substring(1);

				if (__packagePath.EndsWith("\\"))
					__packagePath = __packagePath.Substring(0, __packagePath.Length - 1);

				__packagePath = __packagePath.Replace("\\", ".");
				__packagePath = __packagePath.Replace("/", ".");
            }
            else
            {
				__packagePath = String.Empty;
            }


			return __packagePath;
        }

        private void pathTextBox_TextChanged(object sender, EventArgs e)
        {
			string __path = pathTextBox.Text.Replace("/", "\\");

			packageTextBox.Text = FormatPackage(__path);

			if (__path.Length > 0 && !__path.StartsWith("\\"))
				__path = "\\" + __path;
			if (!__path.EndsWith("\\"))
				__path += "\\";

			pathLabel.Text = __PathTitle + __SourcePath + __path;
			__RelativeProjectPath = __path.Replace("\\", "/");
			
        }

    }

}
