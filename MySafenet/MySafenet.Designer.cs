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
			this.WelcomeText = new System.Windows.Forms.Label();
			this.WelcomeLabel = new System.Windows.Forms.Label();
			this.ExplorerView = new System.Windows.Forms.ListView();
			this.ExplorerPath = new System.Windows.Forms.Label();
			this.StatusStrip = new System.Windows.Forms.StatusStrip();
			this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.StatusProgress = new System.Windows.Forms.ToolStripProgressBar();
			this.DnsPanel.SuspendLayout();
			this.panel1.SuspendLayout();
			this.StatusStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// DnsPanel
			// 
			this.DnsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DnsPanel.Controls.Add(this.DnsView);
			this.DnsPanel.Controls.Add(this.panel1);
			this.DnsPanel.Location = new System.Drawing.Point(18, 64);
			this.DnsPanel.Name = "DnsPanel";
			this.DnsPanel.Size = new System.Drawing.Size(379, 382);
			this.DnsPanel.TabIndex = 8;
			this.DnsPanel.SizeChanged += new System.EventHandler(this.Panel_SizeChanged);
			// 
			// DnsView
			// 
			this.DnsView.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.DnsView.HideSelection = false;
			this.DnsView.Location = new System.Drawing.Point(27, 22);
			this.DnsView.Margin = new System.Windows.Forms.Padding(0);
			this.DnsView.Name = "DnsView";
			this.DnsView.PathSeparator = ".";
			this.DnsView.Size = new System.Drawing.Size(320, 307);
			this.DnsView.TabIndex = 8;
			// 
			// panel1
			// 
			this.panel1.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.panel1.Controls.Add(this.DnsAdd);
			this.panel1.Controls.Add(this.NewDnsName);
			this.panel1.Location = new System.Drawing.Point(27, 338);
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
			this.NewDnsName.Size = new System.Drawing.Size(222, 27);
			this.NewDnsName.TabIndex = 11;
			// 
			// WelcomeText
			// 
			this.WelcomeText.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.WelcomeText.AutoSize = true;
			this.WelcomeText.Location = new System.Drawing.Point(20, 34);
			this.WelcomeText.Name = "WelcomeText";
			this.WelcomeText.Size = new System.Drawing.Size(628, 20);
			this.WelcomeText.TabIndex = 4;
			this.WelcomeText.Text = "Make Sure the SAFE Network Launcher is available and grant access to MySafenet wh" +
    "en asked";
			// 
			// WelcomeLabel
			// 
			this.WelcomeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.WelcomeLabel.Location = new System.Drawing.Point(18, 1);
			this.WelcomeLabel.Name = "WelcomeLabel";
			this.WelcomeLabel.Size = new System.Drawing.Size(752, 40);
			this.WelcomeLabel.TabIndex = 10;
			this.WelcomeLabel.Text = "Welcome to the SAFE Network";
			this.WelcomeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ExplorerView
			// 
			this.ExplorerView.Location = new System.Drawing.Point(403, 86);
			this.ExplorerView.Name = "ExplorerView";
			this.ExplorerView.Size = new System.Drawing.Size(367, 360);
			this.ExplorerView.TabIndex = 11;
			this.ExplorerView.UseCompatibleStateImageBehavior = false;
			// 
			// ExplorerPath
			// 
			this.ExplorerPath.AutoSize = true;
			this.ExplorerPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ExplorerPath.Location = new System.Drawing.Point(403, 64);
			this.ExplorerPath.Name = "ExplorerPath";
			this.ExplorerPath.Size = new System.Drawing.Size(14, 20);
			this.ExplorerPath.TabIndex = 12;
			this.ExplorerPath.Text = "/";
			// 
			// StatusStrip
			// 
			this.StatusStrip.BackColor = System.Drawing.Color.Transparent;
			this.StatusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusProgress,
            this.StatusLabel});
			this.StatusStrip.Location = new System.Drawing.Point(0, 446);
			this.StatusStrip.Name = "StatusStrip";
			this.StatusStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
			this.StatusStrip.Size = new System.Drawing.Size(782, 25);
			this.StatusStrip.SizingGrip = false;
			this.StatusStrip.TabIndex = 13;
			// 
			// StatusLabel
			// 
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = new System.Drawing.Size(187, 20);
			this.StatusLabel.Text = "Welcome to SAFE Network";
			// 
			// StatusProgress
			// 
			this.StatusProgress.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.StatusProgress.AutoSize = false;
			this.StatusProgress.Name = "StatusProgress";
			this.StatusProgress.Size = new System.Drawing.Size(150, 19);
			// 
			// MySafenet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.ClientSize = new System.Drawing.Size(782, 471);
			this.Controls.Add(this.WelcomeText);
			this.Controls.Add(this.StatusStrip);
			this.Controls.Add(this.ExplorerPath);
			this.Controls.Add(this.ExplorerView);
			this.Controls.Add(this.WelcomeLabel);
			this.Controls.Add(this.DnsPanel);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "MySafenet";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "MySafenet";
			this.Load += new System.EventHandler(this.MySafenet_Load);
			this.DnsPanel.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.StatusStrip.ResumeLayout(false);
			this.StatusStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Panel DnsPanel;
		private System.Windows.Forms.TreeView DnsView;
		private System.Windows.Forms.Label WelcomeText;
		private System.Windows.Forms.Label WelcomeLabel;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button DnsAdd;
		private System.Windows.Forms.TextBox NewDnsName;
		private System.Windows.Forms.ListView ExplorerView;
		private System.Windows.Forms.Label ExplorerPath;
		private System.Windows.Forms.StatusStrip StatusStrip;
		private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
		private System.Windows.Forms.ToolStripProgressBar StatusProgress;
	}
}

