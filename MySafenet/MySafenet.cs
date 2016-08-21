using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Drunkcod.Safenet;

namespace MySafenet
{
	public partial class MySafenet : Form
	{
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

			ThreadPool.QueueUserWorkItem(state => { 
				var steps = new [] {
					Step("Requesting authorization...", async () => {
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
					}), 
					Step("Loading DNS info...", async () => {
						var getDns = await safe.DnsGetAsync();
						if(getDns.StatusCode != HttpStatusCode.OK)
							throw new Exception("Failed to get Public ID's");
						ui.Post(x => {
							var dnsEntries = (string[])x;
							ProgressLabel.Text = "My Public IDs";
							DnsPanel.Visible = true;
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
					})
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
			});
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
