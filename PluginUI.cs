using System;
using System.Collections;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI;
using PluginCore;
using PluginCore.Helpers;
using PluginCore.Localization;
using Version.Resources;
using System.Text.RegularExpressions;

namespace Version
{
    public delegate void VersionChangeHandler(string setting);
    public delegate void SVNCheckHandler();
    
    public class PluginUI : UserControl
    {
        public event VersionChangeHandler Changed;
        public event SVNCheckHandler SVNCheck;
        private GroupBox groupBox1;
        private PluginMain pluginMain;
        private decimal __Major;
        private decimal __Minor;
        private decimal __Build;
        //private decimal __Revision;
        private Label label4;
        private TextBox vRevision;
        private Label label3;
        private NumericUpDown vMajor;
        private Label label2;
        private NumericUpDown vMinor;
        private Label label1;
        private NumericUpDown vBuild;
        private Button buttonSVNCheck;
        private Label NotTracked;
        private RichTextBox tbDebug;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginUI"/> class.
        /// </summary>
        /// <param name="pluginMain">The plugin main.</param>
		public PluginUI(PluginMain pluginMain)
		{
			this.InitializeComponent();
			this.pluginMain = pluginMain;
            


            LocaleVersion locale = PluginCore.PluginBase.MainForm.Settings.LocaleVersion;
            switch (locale)
            {
                /*
                case LocaleVersion.fi_FI : 
                    // We have Finnish available... or not. :)
                    LocaleHelper.Initialize(LocaleVersion.fi_FI);
                    break;
                */
                default:
                    // Plugins should default to English...
                    LocaleHelper.Initialize(LocaleVersion.en_US);
                    break;
            }

            this.buttonSVNCheck.Text = LocaleHelper.GetString("Title.SVNCheckButton");
            this.NotTracked.Text = LocaleHelper.GetString("Title.NotTracked");
		}

        public void enableVersion()
        {
            this.NotTracked.Visible = false;
            this.buttonSVNCheck.Visible = true;
            this.groupBox1.Visible = true;
        }

        public void disableVersion()
        {
            this.NotTracked.Visible = true;
            this.buttonSVNCheck.Visible = false;
            this.groupBox1.Visible = false;
        }

        /// <summary>
        /// Accessor to the RichTextBox
        /// </summary>
        /// <value>The major.</value>
        public Decimal Major
        {
            get { return __Major; }
            set {
                __Major = value;
                vMajor.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the minor.
        /// </summary>
        /// <value>The minor.</value>
        public Decimal Minor
        {
            get { return __Minor; }
            set
            {
                __Minor = value;
                vMinor.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the build.
        /// </summary>
        /// <value>The build.</value>
        public Decimal Build
        {
            get { return __Build; }
            set
            {
                __Build = value;
                vBuild.Value = value++;
            }
        }

        /// <summary>
        /// Gets or sets the revision.
        /// </summary>
        /// <value>The revision.</value>
        public TextBox Revision
        {
            get { return this.vRevision; }
            set
            {
                vRevision = value;
                this.vRevision = value;
            }
        }

        /// <summary>
        /// Gets or sets the frame.
        /// </summary>
        /// <value>The frame.</value>
        public GroupBox Frame
        {
            get { return this.groupBox1; }
            set
            {
                this.groupBox1 = value;
            }
        }


        /// <summary>
        /// Gets the debug.
        /// </summary>
        /// <value>The debug.</value>
        public RichTextBox Debug
        {
            get { return this.tbDebug; }
        }
		
		#region Windows Forms Designer Generated Code

		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent() 
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.NotTracked = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.vRevision = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.vMajor = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.vMinor = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.vBuild = new System.Windows.Forms.NumericUpDown();
            this.tbDebug = new System.Windows.Forms.RichTextBox();
            this.buttonSVNCheck = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.vMajor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.vMinor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.vBuild)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.vRevision);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.vMajor);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.vMinor);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.vBuild);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(243, 60);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Version of ...";
            // 
            // NotTracked
            // 
            this.NotTracked.BackColor = System.Drawing.SystemColors.Control;
            this.NotTracked.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NotTracked.Location = new System.Drawing.Point(0, 0);
            this.NotTracked.Name = "NotTracked";
            this.NotTracked.Size = new System.Drawing.Size(267, 120);
            this.NotTracked.TabIndex = 10;
            this.NotTracked.Text = "Not tracked";
            this.NotTracked.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.Gray;
            this.label4.Location = new System.Drawing.Point(165, 14);
            this.label4.Margin = new System.Windows.Forms.Padding(0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 13);
            this.label4.TabIndex = 20;
            this.label4.Text = "Revision";
            // 
            // vRevision
            // 
            this.vRevision.Location = new System.Drawing.Point(168, 30);
            this.vRevision.Name = "vRevision";
            this.vRevision.ReadOnly = true;
            this.vRevision.Size = new System.Drawing.Size(66, 20);
            this.vRevision.TabIndex = 16;
            this.vRevision.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.Gray;
            this.label3.Location = new System.Drawing.Point(99, 15);
            this.label3.Margin = new System.Windows.Forms.Padding(0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 13);
            this.label3.TabIndex = 19;
            this.label3.Text = "Build";
            // 
            // vMajor
            // 
            this.vMajor.BackColor = System.Drawing.Color.White;
            this.vMajor.ForeColor = System.Drawing.SystemColors.WindowText;
            this.vMajor.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.vMajor.Location = new System.Drawing.Point(14, 31);
            this.vMajor.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.vMajor.Name = "vMajor";
            this.vMajor.ReadOnly = true;
            this.vMajor.Size = new System.Drawing.Size(38, 20);
            this.vMajor.TabIndex = 15;
            this.vMajor.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.vMajor.ValueChanged += new System.EventHandler(this.vMajor_ValueChanged);
            this.vMajor.Click += new System.EventHandler(this.vMajor_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Gray;
            this.label2.Location = new System.Drawing.Point(55, 15);
            this.label2.Margin = new System.Windows.Forms.Padding(0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 13);
            this.label2.TabIndex = 18;
            this.label2.Text = "Minor";
            // 
            // vMinor
            // 
            this.vMinor.BackColor = System.Drawing.Color.White;
            this.vMinor.Location = new System.Drawing.Point(58, 31);
            this.vMinor.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.vMinor.Name = "vMinor";
            this.vMinor.ReadOnly = true;
            this.vMinor.Size = new System.Drawing.Size(38, 20);
            this.vMinor.TabIndex = 14;
            this.vMinor.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.vMinor.ValueChanged += new System.EventHandler(this.vMinor_ValueChanged);
            this.vMinor.Click += new System.EventHandler(this.vMinor_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Gray;
            this.label1.Location = new System.Drawing.Point(11, 15);
            this.label1.Margin = new System.Windows.Forms.Padding(0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 17;
            this.label1.Text = "Major";
            // 
            // vBuild
            // 
            this.vBuild.BackColor = System.Drawing.Color.White;
            this.vBuild.Location = new System.Drawing.Point(102, 31);
            this.vBuild.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.vBuild.Name = "vBuild";
            this.vBuild.ReadOnly = true;
            this.vBuild.Size = new System.Drawing.Size(60, 20);
            this.vBuild.TabIndex = 13;
            this.vBuild.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.vBuild.ValueChanged += new System.EventHandler(this.vBuild_ValueChanged);
            this.vBuild.Click += new System.EventHandler(this.vBuild_Click);
            // 
            // tbDebug
            // 
            this.tbDebug.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tbDebug.Location = new System.Drawing.Point(12, 118);
            this.tbDebug.Name = "tbDebug";
            this.tbDebug.Size = new System.Drawing.Size(241, 68);
            this.tbDebug.TabIndex = 8;
            this.tbDebug.Text = "";
            this.tbDebug.Visible = false;
            // 
            // buttonSVNCheck
            // 
            this.buttonSVNCheck.Location = new System.Drawing.Point(12, 78);
            this.buttonSVNCheck.Name = "buttonSVNCheck";
            this.buttonSVNCheck.Size = new System.Drawing.Size(243, 27);
            this.buttonSVNCheck.TabIndex = 9;
            this.buttonSVNCheck.Text = "Read SVN revision (no update)";
            this.buttonSVNCheck.UseVisualStyleBackColor = true;
            this.buttonSVNCheck.Click += new System.EventHandler(this.buttonSVNCheck_Click);
            // 
            // PluginUI
            // 
            this.Controls.Add(this.buttonSVNCheck);
            this.Controls.Add(this.tbDebug);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.NotTracked);
            this.Name = "PluginUI";
            this.Size = new System.Drawing.Size(267, 120);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.vMajor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.vMinor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.vBuild)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

        /// <summary>
        /// Handles the Click event of the button1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void button1_Click(object sender, EventArgs e)
        {
            //@"static public const Timestamp:String = (""\d{2}/\d{2}/\d{2}\s\d{2}:\d{2}:\d{2}"");",
            //Boolean test = Regex.IsMatch("static public const Timestamp:String = \"25/02/2010 16:47:27\";", tbDebug.Text);
            //MessageBox.Show(test.ToString());
        }

        /// <summary>
        /// Handles the ValueChanged event of the vMajor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void vMajor_ValueChanged(object sender, EventArgs e)
        {
            //tbDebug.Text += "changed!";
            __Major = vMajor.Value;
            FireChanged("vMajor");
        }

        /// <summary>
        /// Handles the ValueChanged event of the vMinor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void vMinor_ValueChanged(object sender, EventArgs e)
        {
            //tbDebug.Text += "changed!";
            __Minor = vMinor.Value;
            FireChanged("vMinor");
        }

        /// <summary>
        /// Handles the ValueChanged event of the vBuild control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void vBuild_ValueChanged(object sender, EventArgs e)
        {
            __Build = vBuild.Value;
            FireChanged("vBuild");
        }

        /// <summary>
        /// Handles the Click event of the vBuild control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void vBuild_Click(object sender, EventArgs e)
        {
            __Build = vBuild.Value;
            if (__Build >= 1)
                __Build--;

            //FireChanged("vBuild");
        }

        /// <summary>
        /// Handles the Click event of the vMinor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void vMinor_Click(object sender, EventArgs e)
        {
            //FireChanged("vMinor");
        }

        /// <summary>
        /// Handles the Click event of the vMajor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void vMajor_Click(object sender, EventArgs e)
        {
            //FireChanged("vMajor");
        }

        /// <summary>
        /// Fires the changed.
        /// </summary>
        /// <param name="type">The type.</param>
        private void FireChanged(string type)
        {
            if (Changed != null)
                Changed(type);
        }

        /// <summary>
        /// Handles the Click event of the buttonSVNCheck control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void buttonSVNCheck_Click(object sender, EventArgs e)
        {
            if (SVNCheck != null)
                SVNCheck();
        }
				
 	}

}
