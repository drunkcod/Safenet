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
		ContextMenu dnsActions;

		public MySafenet() {
			InitializeComponent();
		}

		private void MySafenet_Load(object sender, EventArgs e) {
			dnsActions = new ContextMenu(new [] {
				new MenuItem("Delete", (s, args) => {
					var m = (MenuItem)s;
					var node = (TreeNode)m.Parent.Tag;
					if(MessageBox.Show($"You sure you want to unregister '{node.Text}'?", "Delete?", MessageBoxButtons.YesNo) != DialogResult.Yes)
						return;
					var deletePublicId = safe.DnsDeleteAsync(node.Text).Result;
					if(deletePublicId.StatusCode != HttpStatusCode.OK) {
						MessageBox.Show($"Failed to delete public id '{node.Text}'", "Error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					}
					DnsView.Nodes.Remove(node);
				}),
			});
			DnsView.NodeMouseClick += (s, args) => {
				if(args.Button != MouseButtons.Right)
					return;
				dnsActions.Tag = args.Node;
				dnsActions.Show((Control)s, args.Location);
			};

			ThreadPool.QueueUserWorkItem(_ => { 
				var steps = new [] {
					new KeyValuePair<string, Func<Task>>("Authorize", async () => {
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
					new KeyValuePair<string, Func<Task>>("Load Application Data", async () => Thread.Sleep(100)), 
					new KeyValuePair<string, Func<Task>>("Preparing UI", async () => {
						var getDns = await safe.DnsGetAsync();
						if(getDns.StatusCode != HttpStatusCode.OK)
							throw new Exception("Failed to get Public ID's");
						Invoke(new Action(() => {
							DnsPanel.Visible = true;
							foreach(var item in getDns.Response) {
								var node = DnsView.Nodes.Add(item);
								var getServices = safe.DnsGetAsync(item).Result;
								foreach(var service in getServices.Response)
									node.Nodes.Add(service);
							}
						}));
					})
				};

				Invoke(new Action(() => {
					ConnectionProgres.Step = 1;
					ConnectionProgres.Maximum = steps.Length;
				}));
				try { 
					foreach(var item in steps) {
						Invoke(new Action(() => ProgressLabel.Text = item.Key));
						item.Value().Wait();
						Invoke(new Action(() => ConnectionProgres.PerformStep()));
					}
				} catch(AggregateException ex) {
					MessageBox.Show(ex.InnerException.Message, "Error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
					Application.Exit();
				}
			});
		}

		private void DnsAdd_Click(object sender, EventArgs e) {
			var createService = safe.DnsPostAsync(NewDnsName.Text).Result;
			if(createService.StatusCode != HttpStatusCode.OK) { 
				MessageBox.Show("Failed to register service", "Error.", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			DnsView.Nodes.Add(NewDnsName.Text);
		}
	}
}
