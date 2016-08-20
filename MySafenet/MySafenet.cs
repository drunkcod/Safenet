﻿using System;
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
			Panel_SizeChanged(LoadingPanel, EventArgs.Empty);
			Panel_SizeChanged(DnsPanel, EventArgs.Empty);

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
				new MenuItem("Add Service", (s, args) => {

				}),
			});
			DnsView.NodeMouseClick += (s, args) => {
				if(args.Button != MouseButtons.Right)
					return;
				dnsActions.Tag = args.Node;
				dnsActions.Show((Control)s, args.Location);
			};

			var ui = SynchronizationContext.Current;

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
