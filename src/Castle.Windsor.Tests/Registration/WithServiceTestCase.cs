// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

namespace Castle.MicroKernel.Tests.Registration
{
	using System.Linq;

	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.Windsor.Tests;
	using Castle.Windsor.Tests.ClassComponents;

	using NUnit.Framework;

	[TestFixture]
	public class WithServiceTestCase : RegistrationTestCaseBase
	{
		[Test]
		public void AllInterfaces_uses_all_implemented_interfaces()
		{
			Kernel.Register(AllTypes.FromThisAssembly().Where(t => t == typeof(TwoInterfacesImpl)).WithService.AllInterfaces());
			var handlers = Kernel.GetAssignableHandlers(typeof(object));
			Assert.AreEqual(2, handlers.Length);
			Assert.True(handlers.Any(h => h.Service == typeof(ICommon)));
			Assert.True(handlers.Any(h => h.Service == typeof(ICommon2)));
		}

		[Test]
		public void AllInterfaces_uses_single_interface()
		{
			Kernel.Register(AllTypes.FromThisAssembly().Where(t => t == typeof(CommonImpl1)).WithService.AllInterfaces());
			var handler = Kernel.GetAssignableHandlers(typeof(object)).Single();
			Assert.AreEqual(typeof(ICommon), handler.Service);
		}

		[Test]
		public void AllTypes_not_specified_uses_service_itself()
		{
			Kernel.Register(AllTypes.FromThisAssembly().Where(t => t == typeof(CommonImpl1)));
			var handler = Kernel.GetAssignableHandlers(typeof(object)).Single();
			Assert.AreEqual(typeof(CommonImpl1), handler.Service);
		}

		[Test]
		public void Base_uses_both_types_from_BasedOn_if_implemented_generically_twice()
		{
			// NOTE: This scenario should be tested for not just Base, but also other methods as well?
			Kernel.Register(AllTypes.FromThisAssembly().BasedOn(typeof(ISimpleGeneric<>)).WithService.Base());

			var allHandlers = Kernel.GetAssignableHandlers(typeof(object));
			var aHandlers = Kernel.GetAssignableHandlers(typeof(ISimpleGeneric<A>));
			var bHandlers = Kernel.GetAssignableHandlers(typeof(ISimpleGeneric<B>));

			Assert.IsNotEmpty(allHandlers);
			Assert.True(allHandlers.All(h => h.ComponentModel.Implementation == typeof(ClosedSimpleGenericOverAAndB)));
			Assert.IsNotEmpty(aHandlers);
			Assert.IsNotEmpty(bHandlers);
		}

		[Test]
		public void Base_uses_type_from_BasedOn()
		{
			Kernel.Register(AllTypes.FromThisAssembly().BasedOn<ICommon>().WithService.Base());

			var handlers = Kernel.GetAssignableHandlers(typeof(object));

			Assert.IsNotEmpty(handlers);
			Assert.True(handlers.All(h => h.Service == typeof(ICommon)));
		}

		[Test]
		public void Can_cumulate_services()
		{
			Kernel.Register(AllTypes.FromThisAssembly().Where(t => t == typeof(TwoInterfacesImpl))
			                	.WithService.AllInterfaces()
			                	.WithService.Self());
			var handlers = Kernel.GetAssignableHandlers(typeof(object));
			Assert.AreEqual(3, handlers.Length);
			Assert.True(handlers.Any(h => h.Service == typeof(ICommon)));
			Assert.True(handlers.Any(h => h.Service == typeof(ICommon2)));
			Assert.True(handlers.Any(h => h.Service == typeof(TwoInterfacesImpl)));
		}

		[Test]
		public void Can_cumulate_services_without_duplication()
		{
			Kernel.Register(AllTypes.FromThisAssembly().Where(t => t == typeof(TwoInterfacesImpl))
			                	.WithService.AllInterfaces()
			                	.WithService.FirstInterface());
			var handlers = Kernel.GetAssignableHandlers(typeof(object));
			Assert.AreEqual(2, handlers.Length);
			Assert.True(handlers.Any(h => h.Service == typeof(ICommon)));
			Assert.True(handlers.Any(h => h.Service == typeof(ICommon2)));
		}

		[Test]
		public void Component_not_specified_uses_service_itself()
		{
			Kernel.Register(Component.For<CommonImpl1>());
			var handler = Kernel.GetAssignableHandlers(typeof(object)).Single();
			Assert.AreEqual(typeof(CommonImpl1), handler.Service);
		}

		[Test]
		public void DefaultInterface_can_match_multiple_types()
		{
			Kernel.Register(AllTypes.FromThisAssembly().Where(t => t == typeof(CommonSub1Impl)).WithService.DefaultInterface());
			var one = Kernel.Resolve<ICommon>();
			var two = Kernel.Resolve<ICommonSub1>();
			Assert.AreSame(one, two);
		}

		[Test]
		public void DefaultInterface_ignores_not_matched_interfaces()
		{
			Kernel.Register(AllTypes.FromThisAssembly().Where(t => t == typeof(CommonSub2Impl)).WithService.DefaultInterface());

			var one = Kernel.Resolve<ICommon>();
			var two = Kernel.Resolve<ICommonSub2>();

			Assert.AreSame(one, two);
			Assert.Throws<ComponentNotFoundException>(() => Kernel.Resolve<ICommonSub1>());
		}

		[Test]
		public void DefaultInterface_ignores_on_no_interface_matched()
		{
			Kernel.Register(AllTypes.FromThisAssembly().Where(t => t == typeof(TwoInterfacesImpl)).WithService.DefaultInterface());
			var handler = Kernel.GetAssignableHandlers(typeof(object)).Single();
			Assert.AreEqual(typeof(TwoInterfacesImpl), handler.Service);
		}

		[Test]
		public void DefaultInterface_matches_by_type_name()
		{
			Kernel.Register(AllTypes.FromThisAssembly().Where(t => t == typeof(CommonImpl1)).WithService.DefaultInterface());
			var handler = Kernel.GetAssignableHandlers(typeof(object)).Single();
			Assert.AreEqual(typeof(ICommon), handler.Service);
		}

		[Test]
		public void FirstInterface_uses_single_interface()
		{
			Kernel.Register(AllTypes.FromThisAssembly().Where(t => t == typeof(CommonImpl1)).WithService.FirstInterface());
			var handler = Kernel.GetAssignableHandlers(typeof(object)).Single();
			Assert.AreEqual(typeof(ICommon), handler.Service);
		}

		[Test]
		public void FromInterface_uses_subtypes_of_type_from_BasedOn_but_not_the_type_itself()
		{
			Kernel.Register(AllTypes.FromThisAssembly().BasedOn<ICommon>().WithService.FromInterface());

			var handlers = Kernel.GetAssignableHandlers(typeof(object));

			Assert.IsNotEmpty(handlers);
			Assert.True(handlers.All(h => typeof(ICommon).IsAssignableFrom(h.Service)));
			Assert.True(handlers.Any(h => typeof(ICommon) != h.Service));
		}

		[Test]
		public void Self_uses_service_itself()
		{
			Kernel.Register(AllTypes.FromThisAssembly().Where(t => t == typeof(CommonImpl1)).WithService.Self());
			var handler = Kernel.GetAssignableHandlers(typeof(object)).Single();
			Assert.AreEqual(typeof(CommonImpl1), handler.Service);
		}
	}
}