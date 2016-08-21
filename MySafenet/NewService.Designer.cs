namespace MySafenet
{
	partial class NewService
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
			this.Ok = new System.Windows.Forms.Button();
			this.Cancel = new System.Windows.Forms.Button();
			this.ServiceNameInput = new System.Windows.Forms.TextBox();
			this.RootPath = new System.Windows.Forms.TreeView();
			this.ServiceNameLabel = new System.Windows.Forms.Label();
			this.RootPathLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// Ok
			// 
			this.Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Ok.Location = new System.Drawing.Point(346, 246);
			this.Ok.Name = "Ok";
			this.Ok.Size = new System.Drawing.Size(75, 23);
			this.Ok.TabIndex = 0;
			this.Ok.Text = "Ok";
			this.Ok.UseVisualStyleBackColor = true;
			// 
			// Cancel
			// 
			this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel.Location = new System.Drawing.Point(265, 246);
			this.Cancel.Name = "Cancel";
			this.Cancel.Size = new System.Drawing.Size(75, 23);
			this.Cancel.TabIndex = 1;
			this.Cancel.Text = "Cancel";
			this.Cancel.UseVisualStyleBackColor = true;
			// 
			// ServiceNameInput
			// 
			this.ServiceNameInput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ServiceNameInput.Location = new System.Drawing.Point(121, 12);
			this.ServiceNameInput.Name = "ServiceNameInput";
			this.ServiceNameInput.Size = new System.Drawing.Size(300, 22);
			this.ServiceNameInput.TabIndex = 2;
			// 
			// RootPath
			// 
			this.RootPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.RootPath.HideSelection = false;
			this.RootPath.Location = new System.Drawing.Point(121, 38);
			this.RootPath.Name = "RootPath";
			this.RootPath.Size = new System.Drawing.Size(300, 202);
			this.RootPath.TabIndex = 3;
			// 
			// ServiceNameLabel
			// 
			this.ServiceNameLabel.AutoSize = true;
			this.ServiceNameLabel.Location = new System.Drawing.Point(19, 15);
			this.ServiceNameLabel.Name = "ServiceNameLabel";
			this.ServiceNameLabel.Size = new System.Drawing.Size(96, 17);
			this.ServiceNameLabel.TabIndex = 4;
			this.ServiceNameLabel.Text = "Service Name";
			// 
			// RootPathLabel
			// 
			this.RootPathLabel.AutoSize = true;
			this.RootPathLabel.Location = new System.Drawing.Point(44, 38);
			this.RootPathLabel.Name = "RootPathLabel";
			this.RootPathLabel.Size = new System.Drawing.Size(71, 17);
			this.RootPathLabel.TabIndex = 5;
			this.RootPathLabel.Text = "Root Path";
			// 
			// NewService
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(433, 281);
			this.Controls.Add(this.RootPathLabel);
			this.Controls.Add(this.ServiceNameLabel);
			this.Controls.Add(this.RootPath);
			this.Controls.Add(this.ServiceNameInput);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.Ok);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "NewService";
			this.ShowInTaskbar = false;
			this.Text = "NewService";
			this.Load += new System.EventHandler(this.NewService_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button Ok;
		private System.Windows.Forms.Button Cancel;
		private System.Windows.Forms.Label ServiceNameLabel;
		private System.Windows.Forms.Label RootPathLabel;
		private System.Windows.Forms.TextBox ServiceNameInput;
		private System.Windows.Forms.TreeView RootPath;
	}
}