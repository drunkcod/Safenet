using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
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
			public string RootPath;
			public string Path;
			public Func<Task> Refresh = () => Task.FromResult(0);
			public Stack<Func<Task>> Back = new Stack<Func<Task>>();
		}

		class ExplorerViewItem
		{
			public Func<Task> Enter;
			public Func<Task> Delete;
			public Func<string, IEnumerable<KeyValuePair<string, Task<SafenetResponse>>>> Download; 
		}

		readonly SafenetClient safe = new SafenetClient();
		readonly SynchronizationContext ui;
		readonly ThreadPoolWorker worker;

		public MySafenet() {
			InitializeComponent();
			this.ui = SynchronizationContext.Current;
			this.worker = new ThreadPoolWorker();
		}

		void DnsActions_Delete(object sender, EventArgs e) {
			var m = (MenuItem)sender;
			var node = (TreeNode)m.Parent.Tag;
			if(MessageBox.Show($"You sure you want to unregister '{node.Text}'?", "Delete?", MessageBoxButtons.YesNo) != DialogResult.Yes)
				return;
			var deletePublicId = safe.DnsDeleteAsync(node.Text).AwaitResult();
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
				worker.Post(AddService, input, (TreeNode)m.Parent.Tag);
			}
		}

		void AddService(NewService input, TreeNode node) {
			var registerService = safe.DnsPutAsync(new SafenetDnsRegisterServiceRequest {
				LongName = node.Text,
				ServiceName = input.ServiceName,
				RootPath = "app",
				ServiceHomeDirPath = input.ServiceRoot,
			});
			if(registerService.AwaitResult().StatusCode != HttpStatusCode.OK)
				MessageBox.Show("Failed to register service", "Error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
			else ui.Post(obj => node.Nodes.Add(obj.ToString()), input.ServiceName);
		}

		void ServiceActions_Delete(object sender, EventArgs e) {
			var m = (MenuItem)sender;
			var node = (TreeNode)m.Parent.Tag;
			if(MessageBox.Show($"You sure you want to delete '{node.Text}'?", "Delete?", MessageBoxButtons.YesNo) != DialogResult.Yes)
				return;
			var deleteService = safe.DnsDeleteAsync(node.Text, node.Parent.Text).AwaitResult();
			if(deleteService.StatusCode != HttpStatusCode.OK) {
				MessageBox.Show($"Failed to delete service '{node.Text}'", "Error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			DnsView.Nodes.Remove(node);
		}

		private void MySafenet_Load(object sender, EventArgs e) {
			Text = Text + " v" + Version;
			Font = SystemFonts.DefaultFont;
			WelcomeLabel.Font = new Font(SystemFonts.DefaultFont.FontFamily, 16, FontStyle.Bold);
			Center(WelcomeLabel);
			StatusLabel.Font = SystemFonts.StatusFont;
			WelcomeText.Font = new Font(SystemFonts.DefaultFont.FontFamily, 9, FontStyle.Italic);

			DnsAdd.Font = SystemFonts.DefaultFont;
			NewDnsName.Font = SystemFonts.DefaultFont;
			DnsLabel.Font = new Font(SystemFonts.DefaultFont.FontFamily, 12, FontStyle.Bold);
			NewDnsName.Height = DnsAdd.Height;
			DnsAdd.TextAlign = ContentAlignment.MiddleCenter;
			CenterChildControls(DnsPanel, EventArgs.Empty);

			Center(WelcomeText);

			ConfigureDnsView();
			ConfigureExplorerView();
			worker.Post(RunSteps, new [] {
				Step("Requesting authorization...", RequestAuthorization), 
				Step("Loading DNS info...", LoadDnsInfo),
				Step("Preparing Storage Explorer...", LoadStorageInfo)
			});
		}

		void RunSteps(KeyValuePair<string,Func<Task>>[] steps) {
			ui.Post(_ => {
				StatusProgress.Step = 1;
				StatusProgress.Maximum = steps.Length;
			}, null);

			try { 
				foreach(var item in steps) {
					ui.Post(_ => StatusLabel.Text = item.Key, null);
						
					item.Value().Wait();
					ui.Post(_ => StatusProgress.PerformStep(), null);
				}
			} catch(AggregateException ex) {
				MessageBox.Show(ex.InnerException.Message, "Error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Application.Exit();
			}
			ui.Post(_ => StatusLabel.Text = "Ready.", null);
		} 

		void ConfigureDnsView() {
			var dnsActions = new ContextMenu(new[] {
				new MenuItem("Delete", DnsActions_Delete),
				new MenuItem("Add Service", DnsActions_AddService)
			});

			var serviceActions = new ContextMenu(new[] {
				new MenuItem("Delete", ServiceActions_Delete),
			});

			DnsView.NodeMouseClick += (s, args) => {
				if (args.Button != MouseButtons.Right)
					return;
				if (args.Node.Level == 0) {
					dnsActions.Tag = args.Node;
					dnsActions.Show((Control) s, args.Location);
				} else {
					serviceActions.Tag = args.Node;
					serviceActions.Show((Control) s, args.Location);
				}
			};
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

			ExplorerView.AllowDrop = true;
			ExplorerView.DragEnter += (sender, args) => args.Effect = DragDropEffects.Copy;
			ExplorerView.DragDrop += ExplorerView_DragDrop; 

			ExplorerView.MouseDoubleClick += ExplorerView_MouseDoubleClick;
			ExplorerView.KeyDown += ExplorerView_KeyDown;

			var explorerActions = new ContextMenu(new [] {
				new MenuItem("Download", ExplorerView_Download), 
			});

			ExplorerView.MouseClick += (s, args) => {
				var view = (ListView)s;
				if(args.Button != MouseButtons.Right || view.SelectedItems.Count == 0)
					return;
				explorerActions.Tag = view.SelectedItems.Cast<ListViewItem>().Select(x => x.Tag).Cast<ExplorerViewItem>();
				explorerActions.Show(view, args.Location);
			};
		}

		void ExplorerView_DragDrop(object sender, DragEventArgs e) { 
			var progress = new FileUploadProgressDialog();
			progress.ActiveFile.Font = this.Font;
			worker.Post(UploadDroppedFiles,
				progress,
				(ExplorerViewContext)((ListView)sender).Tag,
				(string[])e.Data.GetData(DataFormats.FileDrop));
			progress.ShowDialog();
		}

		void UploadDroppedFiles(FileUploadProgressDialog progress, ExplorerViewContext ctx, string[] paths) {
			var uploads = safe.UploadPathsAsync(paths, ctx.RootPath, ctx.Path, (_, e) => {
				ui.Post(obj => {
					var p = (FileUploadProgressDialog)obj;
					p.Progress.Maximum = e.TotalFiles;
					p.Progress.Value = e.UploadedFiles;
					p.ActiveFile.Text = $"{e.UploadedFiles}/{e.TotalFiles}: {e.ActiveFile}";
				}, progress);
			}).GetResults();
			foreach(var item in uploads.Where(x => x.Value.StatusCode != HttpStatusCode.OK))
				MessageBox.Show($"Failed to upload '{item.Key}' reason: {item.Value.Error.Value.Description}", "Error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
			ctx.Refresh().Wait();
			ui.Post(_ => progress.Close(), null);
		}

		void ExplorerView_MouseDoubleClick(object sender, MouseEventArgs e) {
			if(e.Button != MouseButtons.Left)
				return;
			ActivateSelected((ListView)sender);
		}

		void ExplorerView_KeyDown(object sender, KeyEventArgs e) {
			var view = (ListView)sender;
			switch(e.KeyCode) {
				case Keys.Return:
					e.Handled = ActivateSelected(view);
					break;
				case Keys.Back:
					var ctx = (ExplorerViewContext)view.Tag;
					if(ctx.Back.Count == 0)
						break;
					worker.Post(x => x().AwaitResult(), ctx.Back.Pop());
					e.Handled = true;
					break;
				case Keys.Delete: 
					e.Handled = DeleteSelected(view);
					break;
			}
		}

		void ExplorerView_Download(object sender, EventArgs e) {
			var destination = new FolderBrowserDialog();
			if(destination.ShowDialog() != DialogResult.OK)
				return;
			var m = (MenuItem)sender;
			var progress = new FileUploadProgressDialog();
			progress.Text = "Downloading.";
			progress.ActiveFile.Text = "Preparing download.";
			worker.Post(downloads => {
				var items = new List<Task<SafenetResponse>>();
				foreach(var item in downloads.SelectMany(x => x)) { 
					items.Add(item.Value.ContinueWith(x => {
						ui.Post(_ => {
							progress.Progress.PerformStep();
							progress.ActiveFile.Text = $"{progress.Progress.Value}/{progress.Progress.Maximum} {item.Key}";
						}, null);
						return x.Result;
					}));
					ui.Post(max => {
						progress.Progress.Maximum = (int)max;
						progress.ActiveFile.Text = $"{progress.Progress.Value}/{progress.Progress.Maximum} {item.Key}";
					}, items.Count);
				}

				items.ForEach(x => x.AwaitResult());
				ui.Post(_ => progress.Close(), null);

			}, (m.Parent.Tag as IEnumerable<ExplorerViewItem>).Select(x => x.Download(destination.SelectedPath)).ToArray());
			progress.ShowDialog();
		}

		bool ActivateSelected(ListView sender) {
			if(sender.SelectedItems.Count != 1)
				return false;
			var item = sender.SelectedItems[0];
			var itemTag = (ExplorerViewItem) item.Tag;
			if(itemTag?.Enter == null)
				return false;

			var ctx = (ExplorerViewContext)sender.Tag;
			ctx.Back.Push(ctx.Refresh);
			worker.Post(x => x().AwaitResult(), itemTag.Enter);
			return true;
		}

		bool DeleteSelected(ListView sender) {
			foreach(ListViewItem item in sender.SelectedItems) { 
				var itemTag = (ExplorerViewItem) item.Tag;
				if(itemTag?.Delete == null)
					continue;

				worker.Post(x => {
					x().AwaitResult();
					ui.Post(_ => sender.Items.Remove(item), null);
				}, itemTag.Delete);
			}
			return true;
		}

		async Task RequestAuthorization() {
			var getToken = await safe.AuthPostAsync(new SafenetAuthRequest {
				App = new SafenetAppInfo {
					Id = "drunckod.mysafenet",
					Name = "MySafenet",
					Vendor = "drunkcod",
					Version = Version,
				},
			});
			if(getToken.StatusCode != HttpStatusCode.OK)
				throw new Exception("Failed to get authorization.");
			safe.SetToken(getToken.Response.Token);
		}

		string Version => GetType().Assembly.GetName().Version.ToString(4); 

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
					worker.Post(target => {
						var getServices = safe.DnsGetAsync(target.Text).AwaitResult();
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
				ctx.RootPath = string.Empty;
				ctx.Path = string.Empty;
				ExplorerPath.Text = string.Empty;
				ExplorerView.Items.Clear();
				ExplorerView.Items.Add(MakeViewItem(
					getDirectory.Response.Info, 0, 
					new ExplorerViewItem { Enter = () => LoadDirectoryInfo("app", "") }));
			}, null);
		}

		async Task LoadDirectoryInfo(string root, string path) {
			var getDirectory = await safe.NfsGetDirectoryAsync(root, path);
			var dirs = Array.ConvertAll(getDirectory.Response.SubDirectories, x =>
				MakeViewItem(x, 1, new ExplorerViewItem {
					Enter = () => LoadDirectoryInfo(root, path + "/" + x.Name),
					Delete = () => safe.NfsDeleteDirectoryAsync(root, path + "/" + x.Name),
					Download = downloadPath => safe.DownloadDirectoryAsync(root, UrlPath.Combine(path, x.Name), downloadPath),
				}));
			var files = Array.ConvertAll(getDirectory.Response.Files, x => 
				MakeViewItem(x, new ExplorerViewItem {
					Delete = () => safe.NfsDeleteFileAsync(root, UrlPath.Combine(path, x.Name)),
					Download = downloadPath => {
						var sourcePath = UrlPath.Combine(path, x.Name);
						return new [] { KeyValuePair.From(
							sourcePath, 
							safe.DownloadFileAsyn(root, sourcePath, Path.Combine(downloadPath, x.Name)))
						};
					}
				}));
			ui.Post(obj => {
				var ctx = (ExplorerViewContext)ExplorerView.Tag;
				ctx.Refresh = async () => await LoadDirectoryInfo(root, path);
				ctx.RootPath = root;
				ctx.Path = path;
				ExplorerPath.Text = ctx.Path + "/";
				ExplorerView.Items.Clear();
				ExplorerView.Items.AddRange((ListViewItem[])obj);
			}, dirs.Concat(files).ToArray());
		}

		static ListViewItem MakeViewItem(SafenetDirectoryInfo x, int img, ExplorerViewItem tag) {
			var item = new ListViewItem(x.Name, img);
			item.SubItems.Add(x.ModifiedOn.ToString());
			if (x.IsPrivate)
				item.SubItems.Add("✓");
			item.Tag = tag;
			return item;
		}

		static ListViewItem MakeViewItem(SafenetFileInfo x, ExplorerViewItem tag) {
			var item = new ListViewItem(x.Name, 2);
			item.SubItems.Add(x.ModifiedOn.ToString());
			item.Tag = tag;
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

		void CenterChildControls(object sender, EventArgs e) {
			var c = (Control)sender;
			foreach(Control item in c.Controls)
				Center(item);
		}

		static void Center(Control item) =>
			item.Left = (item.Parent.ClientSize.Width - item.Width) / 2;

		static T GetAttribute<T>() where T : Attribute =>
			(T)typeof(Program).Assembly.GetCustomAttribute(typeof(T));

	}
}
