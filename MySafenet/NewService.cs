using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Drunkcod.Safenet;

namespace MySafenet
{
	public partial class NewService : Form
	{
		readonly SafenetClient safe;
		readonly SynchronizationContext ui;

		public NewService(SafenetClient safe) {
			InitializeComponent();
			this.safe = safe;
			this.ui = SynchronizationContext.Current;
		}

		public string ServiceName => ServiceNameInput.Text; 
		public string ServiceRoot => RootPath.SelectedNode.Tag.ToString();

		void NewService_Load(object sender, EventArgs e) {
			LoadDirectory(null, "");
			Ok.Click += (_, args) => {
				if(string.IsNullOrEmpty(ServiceName) || RootPath.SelectedNode == null) {
					MessageBox.Show("Must enter service name and select root path", "Error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				DialogResult = DialogResult.OK;
				Close();
			};
			Cancel.Click += (_, args) => Close();
		}

		void LoadDirectory(TreeNode parent, string path) {
			ThreadPool.QueueUserWorkItem(state => {
				var getDirectory = safe.NfsGetDirectoryAsync("app", path);
				var node = new TreeNode(getDirectory.Result.Response.Info.Name) {
					Tag = path,
				};
				if(parent == null)
					ui.Post(obj => RootPath.Nodes.Add((TreeNode)obj), node);
				else ui.Post(obj => parent.Nodes.Add((TreeNode)obj), node);
				Array.ForEach(getDirectory.Result.Response.SubDirectories, x => LoadDirectory(node, path + "/" + x.Name));
			}, null);
		}
	}
}
