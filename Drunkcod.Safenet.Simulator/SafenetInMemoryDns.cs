using System.Collections.Generic;

namespace Drunkcod.Safenet.Simulator
{
	public class SafenetInMemoryDns
	{
		readonly List<string> services = new List<string>();

		public void Register(string longName) {
			services.Add(longName);
		}

		public void Unregister(string longName) {
			services.Remove(longName);
		}

		public IEnumerable<string> GetServices() => services;

		public void Clear() =>
			services.Clear();
	}
}