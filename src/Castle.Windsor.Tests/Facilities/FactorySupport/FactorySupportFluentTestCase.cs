﻿// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor.Tests.Facilities.FactorySupport
{
	using Castle.Facilities.FactorySupport;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;

	using NUnit.Framework;

	public class User
	{
		public FiscalStability FiscalStability { get; set; }
	}

	public enum FiscalStability
	{
		DirtFarmer,
		MrMoneyBags
	} ;

	public interface ICarProvider
	{
	}

	public class FerrariProvider : ICarProvider
	{
	}

	public class HondaProvider : ICarProvider
	{
	}

	public class AbstractCarProviderFactory
	{
		public ICarProvider Create(User currentUser)
		{
			if (currentUser.FiscalStability == FiscalStability.MrMoneyBags)
			{
				return new FerrariProvider();
			}
			else
			{
				return new HondaProvider();
			}
		}
	}

	[TestFixture]
	public class FactorySupportFluentTestCase
	{
		[SetUp]
		public void SetUp()
		{
			kernel = new DefaultKernel();
			kernel.AddFacility<FactorySupportFacility>();
		}

		private IKernel kernel;

		private void RegisterComponentsImplemtedByFerrari(User user)
		{
			kernel.Register(
				Component.For<User>().Named("currentUser").Instance(user),
				Component.For<AbstractCarProviderFactory>().Named("AbstractCarProviderFactory"),
				Component.For<ICarProvider>()
					.ImplementedBy<FerrariProvider>()
					.Attribute("factoryId").Eq("AbstractCarProviderFactory")
					.Attribute("factoryCreate").Eq("Create")
				);
		}

		[Test]
		public void can_register_without_providing_an_implementation()
		{
			var user = new User { FiscalStability = FiscalStability.DirtFarmer };
			kernel.Register(
				Component.For<User>().Named("currentUser").Instance(user),
				Component.For<AbstractCarProviderFactory>().Named("AbstractCarProviderFactory"),
				Component.For<ICarProvider>()
					.Attribute("factoryId").Eq("AbstractCarProviderFactory")
					.Attribute("factoryCreate").Eq("Create")
				);
			Assert.IsInstanceOf(typeof(HondaProvider), kernel.Resolve<ICarProvider>());
		}

		[Test]
		public void register_ferrari_implementation_get_ferrari_instance()
		{
			RegisterComponentsImplemtedByFerrari(new User { FiscalStability = FiscalStability.MrMoneyBags });
			Assert.IsInstanceOf(typeof(FerrariProvider), kernel.Resolve<ICarProvider>());
		}

		[Test]
		public void register_ferrari_implementation_get_honda_instance()
		{
			RegisterComponentsImplemtedByFerrari(new User { FiscalStability = FiscalStability.DirtFarmer });
			Assert.IsInstanceOf(typeof(HondaProvider), kernel.Resolve<ICarProvider>());
		}
	}
}