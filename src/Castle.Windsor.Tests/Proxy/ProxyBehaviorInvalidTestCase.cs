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

#if !SILVERLIGHT // we do not support xml config on SL

namespace Castle.Windsor.Tests.Proxy
{
	using Castle.DynamicProxy;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.SubSystems.Conversion;
	using Castle.Windsor.Tests.Components;
	using NUnit.Framework;

	[TestFixture]
	public class ProxyBehaviorInvalidTestCase
	{
		[Test]
		public void InvalidProxyBehaviorFromConfiguration()
		{
			Assert.Throws<ConverterException>(() =>
				new WindsorContainer(
					ConfigHelper.ResolveConfigPath("Proxy/proxyBehaviorInvalidConfig.xml")));
		}

		[Test]
		public void RequestSingleInterfaceProxyWithoutServiceInterface()
		{
			var container = new WindsorContainer();
			container.Register(Component.For<StandardInterceptor>());

			Assert.Throws<ComponentRegistrationException>(() =>
			                                              container.Register(
			                                              	Component.For<CalculatorServiceWithSingleProxyBehavior>()));
		}
	}
}

#endif
