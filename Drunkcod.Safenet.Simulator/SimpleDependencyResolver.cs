using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;

namespace Drunkcod.Safenet.Simulator
{
	public class SimpleDependencyResolver : IDependencyResolver
	{
		class SimpleDependencyScope : IDependencyScope
		{
			readonly SimpleDependencyResolver parent;
			readonly IDependencyScope inner;

			public SimpleDependencyScope(SimpleDependencyResolver parent, IDependencyScope inner) {
				this.parent = parent;
				this.inner = inner;
			}

			public void Dispose() => inner.Dispose();
 
			public object GetService(Type serviceType) {
				object found;
				return parent.TryResolveService(serviceType, out found) 
					? found 
					: inner.GetService(serviceType);
			}

			public IEnumerable<object> GetServices(Type serviceType) => inner.GetServices(serviceType);
		}

		readonly IDependencyResolver inner;
		readonly Dictionary<Type, Func<object>> knownTypes = new Dictionary<Type, Func<object>>();

		public SimpleDependencyResolver(IDependencyResolver inner) {
			this.inner = inner;
		}

		public void Register<T>(Func<object> makeObject) => knownTypes.Add(typeof(T), makeObject); 

		bool TryResolveService(Type type, out object instance) {
			Func<object> found;
			if (knownTypes.TryGetValue(type, out found)) {
				instance = found();
				return true;
			}
			instance = null;
			return false;
		}

		public void Dispose() => inner.Dispose();

		public object GetService(Type serviceType) {
			object found;
			if (TryResolveService(serviceType, out found))
				return found;
			return inner.GetService(serviceType);
		}

		public IEnumerable<object> GetServices(Type serviceType) => inner.GetServices(serviceType);

		public IDependencyScope BeginScope()
		{
			return new SimpleDependencyScope(this, inner.BeginScope());
		}
	}
}