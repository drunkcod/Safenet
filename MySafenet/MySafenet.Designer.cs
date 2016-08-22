namespace MySafenet
{
	partial class MySafenet
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if(disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.DnsPanel = new System.Windows.Forms.Panel();
			this.DnsView = new System.Windows.Forms.TreeView();
			this.panel1 = new System.Windows.Forms.Panel();
			this.DnsAdd = new System.Windows.Forms.Button();
			this.NewDnsName = new System.Windows.Forms.TextBox();
			this.LoadingPanel = new System.Windows.Forms.Panel();
			this.ProgressLabel = new System.Windows.Forms.Label();
			this.ConnectionProgres = new System.Windows.Forms.ProgressBar();
			this.WelcomeText = new System.Windows.Forms.Label();
			this.WelcomeLabel = new System.Windows.Forms.Label();
			this.ExplorerView = new System.Windows.Forms.ListView();
			this.DnsPanel.SuspendLayout();
			this.panel1.SuspendLayout();
			this.LoadingPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// DnsPanel
			// 
			this.DnsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DnsPanel.Controls.Add(this.DnsView);
			this.DnsPanel.Controls.Add(this.panel1);
			this.DnsPanel.Location = new System.Drawing.Point(18, 165);
			this.DnsPanel.Name = "DnsPanel";
			this.DnsPanel.Size = new System.Drawing.Size(379, 294);
			this.DnsPanel.TabIndex = 8;
			this.DnsPanel.SizeChanged += new System.EventHandler(this.Panel_SizeChanged);
			// 
			// DnsView
			// 
			this.DnsView.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.DnsView.HideSelection = false;
			this.DnsView.Location = new System.Drawing.Point(25, 4);
			this.DnsView.Margin = new System.Windows.Forms.Padding(0);
			this.DnsView.Name = "DnsView";
			this.DnsView.PathSeparator = ".";
			this.DnsView.Size = new System.Drawing.Size(320, 250);
			this.DnsView.TabIndex = 8;
			// 
			// panel1
			// 
			this.panel1.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.panel1.Controls.Add(this.DnsAdd);
			this.panel1.Controls.Add(this.NewDnsName);
			this.panel1.Location = new System.Drawing.Point(25, 259);
			this.panel1.Margin = new System.Windows.Forms.Padding(0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(320, 35);
			this.panel1.TabIndex = 11;
			// 
			// DnsAdd
			// 
			this.DnsAdd.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.DnsAdd.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DnsAdd.Location = new System.Drawing.Point(232, 4);
			this.DnsAdd.Margin = new System.Windows.Forms.Padding(0);
			this.DnsAdd.Name = "DnsAdd";
			this.DnsAdd.Size = new System.Drawing.Size(88, 26);
			this.DnsAdd.TabIndex = 1;
			this.DnsAdd.Text = "Register";
			this.DnsAdd.UseVisualStyleBackColor = true;
			this.DnsAdd.Click += new System.EventHandler(this.DnsAdd_Click);
			// 
			// NewDnsName
			// 
			this.NewDnsName.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.NewDnsName.Location = new System.Drawing.Point(2, 4);
			this.NewDnsName.Name = "NewDnsName";
			this.NewDnsName.Size = new System.Drawing.Size(222, 22);
			this.NewDnsName.TabIndex = 11;
			// 
			// LoadingPanel
			// 
			this.LoadingPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.LoadingPanel.AutoSize = true;
			this.LoadingPanel.Controls.Add(this.ProgressLabel);
			this.LoadingPanel.Controls.Add(this.ConnectionProgres);
			this.LoadingPanel.Controls.Add(this.WelcomeText);
			this.LoadingPanel.Location = new System.Drawing.Point(18, 46);
			this.LoadingPanel.Name = "LoadingPanel";
			this.LoadingPanel.Size = new System.Drawing.Size(752, 114);
			this.LoadingPanel.TabIndex = 9;
			this.LoadingPanel.SizeChanged += new System.EventHandler(this.Panel_SizeChanged);
			// 
			// ProgressLabel
			// 
			this.ProgressLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.ProgressLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ProgressLabel.Location = new System.Drawing.Point(97, 73);
			this.ProgressLabel.Name = "ProgressLabel";
			this.ProgressLabel.Size = new System.Drawing.Size(584, 23);
			this.ProgressLabel.TabIndex = 6;
			this.ProgressLabel.Text = "Connecting to SAFE Network";
			this.ProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ConnectionProgres
			// 
			this.ConnectionProgres.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.ConnectionProgres.Location = new System.Drawing.Point(83, 32);
			this.ConnectionProgres.Name = "ConnectionProgres";
			this.ConnectionProgres.Size = new System.Drawing.Size(580, 23);
			this.ConnectionProgres.TabIndex = 5;
			// 
			// WelcomeText
			// 
			this.WelcomeText.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.WelcomeText.AutoSize = true;
			this.WelcomeText.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.WelcomeText.Location = new System.Drawing.Point(23, 0);
			this.WelcomeText.Name = "WelcomeText";
			this.WelcomeText.Size = new System.Drawing.Size(724, 20);
			this.WelcomeText.TabIndex = 4;
			this.WelcomeText.Text = "Make Sure the SAFE Network Launcher is available and grant access to MySafenet wh" +
    "en asked";
			// 
			// WelcomeLabel
			// 
			this.WelcomeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.WelcomeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.WelcomeLabel.Location = new System.Drawing.Point(18, 1);
			this.WelcomeLabel.Name = "WelcomeLabel";
			this.WelcomeLabel.Size = new System.Drawing.Size(752, 40);
			this.WelcomeLabel.TabIndex = 10;
			this.WelcomeLabel.Text = "Welcome to the SAFE Network";
			this.WelcomeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ExplorerView
			// 
			this.ExplorerView.Location = new System.Drawing.Point(403, 169);
			this.ExplorerView.Name = "ExplorerView";
			this.ExplorerView.Size = new System.Drawing.Size(367, 285);
			this.ExplorerView.TabIndex = 11;
			this.ExplorerView.UseCompatibleStateImageBehavior = false;
			// 
			// MySafenet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(782, 461);
			this.Controls.Add(this.ExplorerView);
			this.Controls.Add(this.WelcomeLabel);
			this.Controls.Add(this.LoadingPanel);
			this.Controls.Add(this.DnsPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "MySafenet";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "MySafenet";
			this.Load += new System.EventHandler(this.MySafenet_Load);
			this.DnsPanel.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.LoadingPanel.ResumeLayout(false);
			this.LoadingPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Panel DnsPanel;
		private System.Windows.Forms.TreeView DnsView;
		private System.Windows.Forms.Panel LoadingPanel;
		private System.Windows.Forms.Label ProgressLabel;
		private System.Windows.Forms.ProgressBar ConnectionProgres;
		private System.Windows.Forms.Label WelcomeText;
		private System.Windows.Forms.Label WelcomeLabel;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button DnsAdd;
		private System.Windows.Forms.TextBox NewDnsName;
		private System.Windows.Forms.ListView ExplorerView;
	}
}

