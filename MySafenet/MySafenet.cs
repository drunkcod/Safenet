using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Drunkcod.Safenet;
using MySafenet.Properties;

namespace MySafenet
{
	public partial class MySafenet : Form
	{
		class ExplorerViewContext
		{
			public Func<Task> Refresh = () => Task.FromResult(0);
			public Stack<Func<Task>> Back = new Stack<Func<Task>>();
		}

		SafenetClient safe = new SafenetClient();
		readonly SynchronizationContext ui;

		ContextMenu dnsActions;
		ContextMenu serviceActions;

		public MySafenet() {
			InitializeComponent();
			this.ui = SynchronizationContext.Current;
		}

		void DnsActions_Delete(object sender, EventArgs e) {
			var m = (MenuItem)sender;
			var node = (TreeNode)m.Parent.Tag;
			if(MessageBox.Show($"You sure you want to unregister '{node.Text}'?", "Delete?", MessageBoxButtons.YesNo) != DialogResult.Yes)
				return;
			var deletePublicId = safe.DnsDeleteAsync(node.Text).Result;
			if(deletePublicId.StatusCode != HttpStatusCode.OK) {
				MessageBox.Show($"Failed to delete public id '{node.Text}'", "Error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			DnsView.Nodes.Remove(node);
		}

		void DnsActions_AddService(object sender, EventArgs e) {
			using(var input = new NewService(safe)) { 
				if(input.ShowDialog() != DialogResult.OK)
					return;
				var m = (MenuItem)sender;
				var node = (TreeNode)m.Parent.Tag;
				ThreadPool.QueueUserWorkItem(_ => {
					var registerService = safe.DnsPutAsync(new SafenetDnsRegisterServiceRequest
					{
						LongName = node.Text,
						ServiceName = input.ServiceName,
						RootPath = "app",
						ServiceHomeDirPath = input.ServiceRoot,
					});
					if(registerService.Result.StatusCode != HttpStatusCode.OK)
						MessageBox.Show("Failed to register service", "Error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
					else ui.Post(obj => node.Nodes.Add(obj.ToString()), input.ServiceName);
				});
			}
		}

		void ServiceActions_Delete(object sender, EventArgs e) {
			var m = (MenuItem)sender;
			var node = (TreeNode)m.Parent.Tag;
			if(MessageBox.Show($"You sure you want to delete '{node.Text}'?", "Delete?", MessageBoxButtons.YesNo) != DialogResult.Yes)
				return;
			var deleteService = safe.DnsDeleteAsync(node.Text, node.Parent.Text).Result;
			if(deleteService.StatusCode != HttpStatusCode.OK) {
				MessageBox.Show($"Failed to delete service '{node.Text}'", "Error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			DnsView.Nodes.Remove(node);
		}

		private void MySafenet_Load(object sender, EventArgs e) {

			Panel_SizeChanged(LoadingPanel, EventArgs.Empty);
			Panel_SizeChanged(DnsPanel, EventArgs.Empty);

			dnsActions = new ContextMenu(new [] {
				new MenuItem("Delete", DnsActions_Delete),
				new MenuItem("Add Service", DnsActions_AddService)
			});

			serviceActions = new ContextMenu(new [] {
				new MenuItem("Delete", ServiceActions_Delete),
			});

			DnsView.NodeMouseClick += (s, args) => {
				if(args.Button != MouseButtons.Right)
					return;
				if(args.Node.Level == 0) { 
					dnsActions.Tag = args.Node;
					dnsActions.Show((Control)s, args.Location);
				} else {
					serviceActions.Tag = args.Node;
					serviceActions.Show((Control)s, args.Location);
				}
			};

			ConfigureExplorerView();

			ThreadPool.QueueUserWorkItem(state => { 
				var steps = new [] {
					Step("Requesting authorization...", RequestAuthorization), 
					Step("Loading DNS info...", LoadDnsInfo),
					Step("Preparing Storage Explorer...", LoadStorageInfo)
				};

				ui.Post(_ => {
					ConnectionProgres.Step = 1;
					ConnectionProgres.Maximum = steps.Length;
				}, null);

				try { 
					foreach(var item in steps) {
						ui.Post(_ => ProgressLabel.Text = item.Key, null);
						item.Value().Wait();
						ui.Post(_ => ConnectionProgres.PerformStep(), null);
					}
				} catch(AggregateException ex) {
					MessageBox.Show(ex.InnerException.Message, "Error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
					Application.Exit();
				}
				ui.Post(_ => ProgressLabel.Text = "Ready.", null);
			});
		}

		void ConfigureExplorerView() {
			var explorerImages = new ImageList();
			explorerImages.ColorDepth = ColorDepth.Depth32Bit;
			explorerImages.Images.Add(Resources.Box);
			explorerImages.Images.Add(Resources.Folder);
			explorerImages.Images.Add(Resources.Page);
			explorerImages.Images.Add(Resources.Lock);
			ExplorerView.SmallImageList = explorerImages;

			ExplorerView.View = View.Details;
			ExplorerView.Columns.Add("Name", 130);
			ExplorerView.Columns.Add("Date Modified", 130);
			var isPrivate = ExplorerView.Columns.Add("", 28);
			isPrivate.DisplayIndex = 0;
			isPrivate.ImageIndex = 3;

			ExplorerView.Tag = new ExplorerViewContext();

			ExplorerView.MouseDoubleClick += ExplorerView_MouseDoubleClick;
			ExplorerView.KeyPress += ExplorerView_KeyPress;
		}

		void ExplorerView_MouseDoubleClick(object sender, MouseEventArgs e) {
			if(e.Button != MouseButtons.Left)
				return;
			ActivateSelected((ListView)sender);
		}

		void ExplorerView_KeyPress(object sender, KeyPressEventArgs e) {
			var view = (ListView)sender;
			switch(e.KeyChar) {
				case (char)Keys.Return:
					e.Handled = ActivateSelected(view);
					break;
				case (char)Keys.Back:
					var ctx = (ExplorerViewContext)view.Tag;
					if(ctx.Back.Count == 0)
						break;
					ThreadPool.QueueUserWorkItem(x => ((Func<Task>)x)().Wait(), ctx.Back.Pop());
					e.Handled = true;
					break;
			}
		}

		bool ActivateSelected(ListView sender) {
			if(sender.SelectedItems.Count != 1)
				return false;
			var item = sender.SelectedItems[0];
			if(item.Tag == null)
				return false;
			var ctx = (ExplorerViewContext)sender.Tag;
			ctx.Back.Push(ctx.Refresh);
			ThreadPool.QueueUserWorkItem(x => ((Func<Task>)x)().Wait(), sender.SelectedItems[0].Tag);
			return true;
		}

		async Task RequestAuthorization() {
			var getToken = await safe.AuthPostAsync(new SafenetAuthRequest {
				App = new SafenetAppInfo {
					Id = "drunckod.mysafenet",
					Name = "MySafenet",
					Vendor = "drunkcod",
					Version = "0.0.1"
				},
			});
			if(getToken.StatusCode != HttpStatusCode.OK)
				throw new Exception("Failed to get authorization.");
			safe.SetToken(getToken.Response.Token);
		}

		async Task LoadDnsInfo() {
			var getDns = await safe.DnsGetAsync();
			if(getDns.StatusCode != HttpStatusCode.OK)
				throw new Exception("Failed to get Public ID's");
			ui.Post(x => {
				var dnsEntries = (string[])x;
				var entries = dnsEntries.Length;
				if(entries > 0)
					DnsView.Enabled = false;
				foreach(var item in dnsEntries) {
					var node = DnsView.Nodes.Add(item);
					ThreadPool.QueueUserWorkItem(n => {
						var target = (TreeNode)n;
						var getServices = safe.DnsGetAsync(target.Text).Result;
						if(getServices.StatusCode == HttpStatusCode.OK) {
							ui.Post(_ => {
								foreach(var service in getServices.Response)
									target.Nodes.Add(service);
							}, null);
						}
						else {
							MessageBox.Show($"Failed to get services connected to {target.Text}", "Error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
						}
						if(--entries == 0)
							ui.Post(_ => DnsView.Enabled = true, null);
					}, node);
				}
			},getDns.Response);
		}

		async Task LoadStorageInfo() {
			var getDirectory = await safe.NfsGetDirectoryAsync("app", "");
			ui.Post(_ => {
				var ctx = (ExplorerViewContext)ExplorerView.Tag;
				ctx.Refresh = LoadStorageInfo;
				ExplorerView.Items.Clear();
				ExplorerView.Items.Add(MakeViewItem(
					getDirectory.Response.Info, 0, 
					() => LoadDirectoryInfo("app", "")));
			}, null);
		}

		async Task LoadDirectoryInfo(string root, string path) {
			var getDirectory = await safe.NfsGetDirectoryAsync(root, path);
			var dirs = Array.ConvertAll(getDirectory.Response.SubDirectories, x =>
				MakeViewItem(x, 1, () => LoadDirectoryInfo("app", path + "/" + x.Name)));
			var files = Array.ConvertAll(getDirectory.Response.Files, x => {
				var item = new ListViewItem(x.Name, 2);
				item.SubItems.Add(x.ModifiedOn.ToString());
				return item;
			});
			ui.Post(obj => {
				var ctx = (ExplorerViewContext)ExplorerView.Tag;
				ctx.Refresh = async () => await LoadDirectoryInfo(root, path);
				ExplorerView.Items.Clear();
				ExplorerView.Items.AddRange((ListViewItem[])obj);
			}, dirs.Concat(files).ToArray());
		}

		ListViewItem MakeViewItem(SafenetDirectoryInfo x, int img, Func<Task> refresh) {
			var item = new ListViewItem(x.Name, img);
			item.SubItems.Add(x.ModifiedOn.ToString());
			if (x.IsPrivate)
				item.SubItems.Add("✓");
			item.Tag = refresh;
			return item;
		}

		static KeyValuePair<string, Func<Task>> Step(string name, Func<Task> func) => new KeyValuePair<string, Func<Task>>(name, func); 

		void DnsAdd_Click(object sender, EventArgs e) {
			
			var createService = safe.DnsPostAsync(NewDnsName.Text).Result;
			if(createService.StatusCode != HttpStatusCode.OK) { 
				MessageBox.Show("Failed to register service", "Error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			DnsView.Nodes.Add(NewDnsName.Text);
		}

		private void Panel_SizeChanged(object sender, EventArgs e) {
			var c = (Control)sender;
			foreach(Control item in c.Controls)
				item.Left = (item.Parent.ClientSize.Width - item.Width) / 2;
		}
	}
}
