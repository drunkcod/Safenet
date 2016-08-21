using System.Collections.Generic;

namespace Drunkcod.Safenet.Simulator
{
	public class SafenetInMemoryDns
	{
		readonly Dictionary<string, List<string>> services = new Dictionary<string, List<string>>();

		public void Register(string longName) {
			services.Add(longName, new List<string>());
		}

		public void Unregister(string longName) {
			services.Remove(longName);
		}

		public IEnumerable<KeyValuePair<string, List<string>>> GetServices() => services;
		public string[] GetServices(string longName) => services[longName].ToArray();

		public void Clear() =>
			services.Clear();

		public void BindService(string longName, string serviceName) {
			services[longName].Add(serviceName);
		}

		public void RemoveService(string serviceName, string longName) {
			services[longName].Remove(serviceName);
		}
	}
}