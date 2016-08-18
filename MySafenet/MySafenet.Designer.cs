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
			this.ConnectionProgres = new System.Windows.Forms.ProgressBar();
			this.WelcomeLabel = new System.Windows.Forms.Label();
			this.ProgressLabel = new System.Windows.Forms.Label();
			this.WelcomeText = new System.Windows.Forms.Label();
			this.DnsPanel = new System.Windows.Forms.Panel();
			this.DnsViewLabel = new System.Windows.Forms.Label();
			this.DnsAdd = new System.Windows.Forms.Button();
			this.NewDnsName = new System.Windows.Forms.TextBox();
			this.DnsView = new System.Windows.Forms.TreeView();
			this.DnsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// ConnectionProgres
			// 
			this.ConnectionProgres.Location = new System.Drawing.Point(238, 135);
			this.ConnectionProgres.Name = "ConnectionProgres";
			this.ConnectionProgres.Size = new System.Drawing.Size(580, 23);
			this.ConnectionProgres.TabIndex = 0;
			// 
			// WelcomeLabel
			// 
			this.WelcomeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.WelcomeLabel.Location = new System.Drawing.Point(12, 9);
			this.WelcomeLabel.Name = "WelcomeLabel";
			this.WelcomeLabel.Size = new System.Drawing.Size(982, 40);
			this.WelcomeLabel.TabIndex = 1;
			this.WelcomeLabel.Text = "Welcome to the SAFE Network";
			this.WelcomeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ProgressLabel
			// 
			this.ProgressLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ProgressLabel.Location = new System.Drawing.Point(234, 161);
			this.ProgressLabel.Name = "ProgressLabel";
			this.ProgressLabel.Size = new System.Drawing.Size(584, 23);
			this.ProgressLabel.TabIndex = 2;
			this.ProgressLabel.Text = "Connecting to SAFE Network";
			this.ProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// WelcomeText
			// 
			this.WelcomeText.AutoSize = true;
			this.WelcomeText.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.WelcomeText.Location = new System.Drawing.Point(163, 84);
			this.WelcomeText.Name = "WelcomeText";
			this.WelcomeText.Size = new System.Drawing.Size(724, 20);
			this.WelcomeText.TabIndex = 3;
			this.WelcomeText.Text = "Make Sure the SAFE Network Launcher is available and grant access to MySafenet wh" +
    "en asked";
			// 
			// DnsPanel
			// 
			this.DnsPanel.Controls.Add(this.DnsViewLabel);
			this.DnsPanel.Controls.Add(this.DnsAdd);
			this.DnsPanel.Controls.Add(this.NewDnsName);
			this.DnsPanel.Controls.Add(this.DnsView);
			this.DnsPanel.Location = new System.Drawing.Point(238, 187);
			this.DnsPanel.Name = "DnsPanel";
			this.DnsPanel.Size = new System.Drawing.Size(580, 372);
			this.DnsPanel.TabIndex = 8;
			this.DnsPanel.Visible = false;
			// 
			// DnsViewLabel
			// 
			this.DnsViewLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DnsViewLabel.Location = new System.Drawing.Point(133, 30);
			this.DnsViewLabel.Name = "DnsViewLabel";
			this.DnsViewLabel.Size = new System.Drawing.Size(314, 32);
			this.DnsViewLabel.TabIndex = 11;
			this.DnsViewLabel.Text = "My Public ID\'s";
			this.DnsViewLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// DnsAdd
			// 
			this.DnsAdd.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DnsAdd.Location = new System.Drawing.Point(360, 317);
			this.DnsAdd.Name = "DnsAdd";
			this.DnsAdd.Size = new System.Drawing.Size(88, 26);
			this.DnsAdd.TabIndex = 10;
			this.DnsAdd.Text = "Register";
			this.DnsAdd.UseVisualStyleBackColor = true;
			this.DnsAdd.Click += new System.EventHandler(this.DnsAdd_Click);
			// 
			// NewDnsName
			// 
			this.NewDnsName.Location = new System.Drawing.Point(132, 319);
			this.NewDnsName.Name = "NewDnsName";
			this.NewDnsName.Size = new System.Drawing.Size(222, 22);
			this.NewDnsName.TabIndex = 9;
			// 
			// DnsView
			// 
			this.DnsView.Location = new System.Drawing.Point(132, 62);
			this.DnsView.Name = "DnsView";
			this.DnsView.PathSeparator = ".";
			this.DnsView.Size = new System.Drawing.Size(316, 249);
			this.DnsView.TabIndex = 8;
			// 
			// MySafenet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1006, 593);
			this.Controls.Add(this.DnsPanel);
			this.Controls.Add(this.WelcomeText);
			this.Controls.Add(this.ProgressLabel);
			this.Controls.Add(this.WelcomeLabel);
			this.Controls.Add(this.ConnectionProgres);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "MySafenet";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "MySafenet";
			this.Load += new System.EventHandler(this.MySafenet_Load);
			this.DnsPanel.ResumeLayout(false);
			this.DnsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ProgressBar ConnectionProgres;
		private System.Windows.Forms.Label WelcomeLabel;
		private System.Windows.Forms.Label ProgressLabel;
		private System.Windows.Forms.Label WelcomeText;
		private System.Windows.Forms.Panel DnsPanel;
		private System.Windows.Forms.Label DnsViewLabel;
		private System.Windows.Forms.Button DnsAdd;
		private System.Windows.Forms.TextBox NewDnsName;
		private System.Windows.Forms.TreeView DnsView;
	}
}

