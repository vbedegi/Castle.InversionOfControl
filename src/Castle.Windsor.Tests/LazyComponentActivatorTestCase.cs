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

namespace Castle.Windsor.Tests
{
	using System;
	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.ComponentActivator;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Proxy;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.Lifestyle;
	using NUnit.Framework;

	[TestFixture]
	public class LazyComponentActivatorTestCase
	{
		[Test]
		public void Component_created_on_first_invocation()
		{
			var created = false;
			var container = new WindsorContainer();
			container.Register(Component.For<ILazyService>().ImplementedBy<LazyService>().Lazy().OnCreate(delegate { created = true; }));

			var lazy = container.Resolve<ILazyService>();
			Assert.IsFalse(created);
			lazy.WakeUp();
			Assert.IsTrue(created);
		}

		[Test]
		public void Custom_activator_still_works()
		{
			var container = new WindsorContainer();
			container.Register(Component.For<ILazyService>().ImplementedBy<LazyService>().Lazy().Activator<MyCustomLazyServiceActivator>());

			var lazy = container.Resolve<ILazyService>();
			Assert.AreEqual(lazy.Name, "Lazy");
		}

		[Test]
		public void Can_inject_lazily_initialized_service()
		{
			var container = new WindsorContainer();
			container.Register(Component.For<ILazyService>().ImplementedBy<LazyService>().Lazy());
			container.Register(Component.For<LazyDependent>().LifeStyle.Transient);

			var lazyDependent = container.Resolve<LazyDependent>();
			Assert.IsNotNull(lazyDependent.LazyService);
		}

		[Test]
		public void Works_with_singleton_lifestyle()
		{
			var container = new WindsorContainer();
			container.Register(Component.For<ILazyService>().ImplementedBy<LazyService>().Lazy());

			var lazy1 = container.Resolve<ILazyService>();
			var lazy2 = container.Resolve<ILazyService>();

			Assert.AreEqual(lazy1, lazy2);
		}

		[Test]
		public void Works_with_transient_lifestyle()
		{
			var container = new WindsorContainer();
			container.Register(Component.For<ILazyService>().ImplementedBy<LazyService>().Lazy().LifeStyle.Transient);

			var lazy1 = container.Resolve<ILazyService>();
			var lazy2 = container.Resolve<ILazyService>();

			Assert.AreNotEqual(lazy1, lazy2);
		}

		[Test]
		public void When_releasing_the_proxy_the_target_get_released()
		{
			var container = new WindsorContainer();
			container.Register(Component.For<ILazyService>().ImplementedBy<DisposableLazyService>().Lazy().LifeStyle.Transient);

			var lazy = container.Resolve<ILazyService>();
			// make sure, the component gets initialized
			lazy.WakeUp();

			container.Release(lazy);

			// omg
			var innerProxy = ProxyUtil.GetUnproxiedInstance(lazy);
			var targetHost = innerProxy as ILazyTargetHostMixin;
			var target = targetHost.GetTarget() as DisposableLazyService;

			Assert.IsTrue(target.IsDisposed);
		}
	}

	public interface ILazyService
	{
		string Name { get; set; }
		void WakeUp();
	}

	public class LazyService : ILazyService
	{
		public string Name { get; set; }
		public void WakeUp()
		{
		}
	}

	public class DisposableLazyService : DisposableBase, ILazyService
	{
		public string Name { get; set; }
		public void WakeUp()
		{
		}
	}

	public class LazyDependent
	{
		private readonly ILazyService lazyService;

		public LazyDependent(ILazyService lazyService)
		{
			this.lazyService = lazyService;
		}

		public ILazyService LazyService { get { return lazyService; } }
	}

	public class MyCustomLazyServiceActivator : AbstractComponentActivator
	{
		public MyCustomLazyServiceActivator(ComponentModel model, IKernel kernel, ComponentInstanceDelegate onCreation, ComponentInstanceDelegate onDestruction)
			: base(model, kernel, onCreation, onDestruction)
		{
		}

		protected override object InternalCreate(CreationContext context)
		{
			return new LazyService { Name = "Lazy" };
		}

		protected override void InternalDestroy(object instance)
		{
		}
	}
}
