namespace MySafenet
{
	partial class FileUploadProgressDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileUploadProgressDialog));
			this.Progress = new System.Windows.Forms.ProgressBar();
			this.ActiveFile = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// Progress
			// 
			this.Progress.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.Progress.Location = new System.Drawing.Point(12, 53);
			this.Progress.Name = "Progress";
			this.Progress.Size = new System.Drawing.Size(513, 23);
			this.Progress.TabIndex = 0;
			// 
			// ActiveFile
			// 
			this.ActiveFile.AutoSize = true;
			this.ActiveFile.Location = new System.Drawing.Point(12, 22);
			this.ActiveFile.Name = "ActiveFile";
			this.ActiveFile.Size = new System.Drawing.Size(35, 13);
			this.ActiveFile.TabIndex = 1;
			this.ActiveFile.Text = "label1";
			// 
			// FileUploadProgressDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(537, 85);
			this.Controls.Add(this.ActiveFile);
			this.Controls.Add(this.Progress);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FileUploadProgressDialog";
			this.Text = "Uploading.";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public System.Windows.Forms.ProgressBar Progress;
		public System.Windows.Forms.Label ActiveFile;
	}
}