// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.MicroKernel.ComponentActivator
{
	using System;
	using Castle.Core;
	using Castle.DynamicProxy;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Proxy;

	public class LazyComponentActivator : IComponentActivator
	{
		private readonly ComponentModel model;
		private readonly IComponentActivator innerActivator;
		private static readonly ProxyGenerator proxyGenerator = new ProxyGenerator();

		public LazyComponentActivator(ComponentModel model, IComponentActivator innerActivator)
		{
			this.innerActivator = innerActivator;
			this.model = model;
		}

		public object Create(CreationContext context)
		{
			if (model.Service.IsInterface)
			{
				var interceptor = new LazyComponentInterceptor(innerActivator, context);
				var mixin = new LazyTargetHostMixin(interceptor);
				var proxyOptions = new ProxyGenerationOptions();
				proxyOptions.AddMixinInstance(mixin);

				var targetInterface = proxyGenerator.CreateInterfaceProxyWithoutTarget(model.Service, Type.EmptyTypes,
																																						   proxyOptions);
				var proxy = proxyGenerator.CreateInterfaceProxyWithTargetInterface(model.Service, Type.EmptyTypes, targetInterface,
																																				   ProxyGenerationOptions.Default, interceptor);
				return proxy;
			}
			// now what?

			throw new NotImplementedException("Service type must be an interface");
		}

		public void Destroy(object instance)
		{
			var innerProxy = ProxyUtil.GetUnproxiedInstance(instance);
			var targetHost = innerProxy as ILazyTargetHostMixin;

			if (targetHost == null) return;

			var target = targetHost.GetTarget();
			if (target != null)
			{
				innerActivator.Destroy(target);
			}
		}
	}

#if SILVERLIGHT
	public class LazyComponentInterceptor : IInterceptor
#else
	[Serializable]
	public class LazyComponentInterceptor : MarshalByRefObject, IInterceptor
#endif
	{
		private readonly IComponentActivator activator;
		private readonly CreationContext context;
		private object target;

		public LazyComponentInterceptor(IComponentActivator activator, CreationContext context)
		{
			this.activator = activator;
			this.context = context;
		}

		public void Intercept(IInvocation invocation)
		{
			if (target == null)
			{
				target = activator.Create(context);
			}

			var changeProxyTarget = invocation as IChangeProxyTarget;
			if (changeProxyTarget != null)
			{
				changeProxyTarget.ChangeInvocationTarget(target);
			}

			invocation.Proceed();
		}

		public object GetActualTarget()
		{
			return target;
		}
	}

	public interface ILazyTargetHostMixin
	{
		object GetTarget();
	}

	public class LazyTargetHostMixin : ILazyTargetHostMixin
	{
		private readonly LazyComponentInterceptor interceptor;

		public LazyTargetHostMixin(LazyComponentInterceptor interceptor)
		{
			this.interceptor = interceptor;
		}

		public object GetTarget()
		{
			return interceptor.GetActualTarget();
		}
	}
}