﻿// Copyright 2004-2007 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.Framework.Tests.Controllers
{
	using System;
	using NUnit.Framework;
	using Test;

	[TestFixture]
	public class ControllerWithAccessibleThroughTestCase
	{
		private MockEngineContext engineContext;
		private ViewEngineManagerStub viewEngStub;
		private MockServices services;
		private MockRequest request;
		private MockResponse response;

		[SetUp]
		public void Init()
		{
			request = new MockRequest();
			response = new MockResponse();
			services = new MockServices();
			viewEngStub = new ViewEngineManagerStub();
			services.ViewEngineManager = viewEngStub;
			engineContext = new MockEngineContext(request, response, services, null);
		}

		[Test]
		public void ActionWithGetOnlyAttributeCanBeInvokedWithGET()
		{
			AccThrController controller = new AccThrController();

			request.HttpMethod = "GET";

			IControllerContext context = services.ControllerContextFactory.
				Create("", "home", "GetOnly", services.ControllerDescriptorProvider.BuildDescriptor(controller));

			controller.Process(engineContext, context);

			Assert.AreEqual("home\\GetOnly", viewEngStub.TemplateRendered);
		}

		[Test,
		 ExpectedException(typeof(MonoRailException),
		 	"Access to the action [GetOnly] on controller [home] is not allowed to the http verb [POST].")]
		public void ActionWithGetOnlyAttributeCannotBeInvokedWithPOST()
		{
			AccThrController controller = new AccThrController();

			request.HttpMethod = "POST";

			IControllerContext context = services.ControllerContextFactory.
				Create("", "home", "GetOnly", services.ControllerDescriptorProvider.BuildDescriptor(controller));

			try
			{
				controller.Process(engineContext, context);
			}
			catch(Exception)
			{
				Assert.AreEqual(403, response.StatusCode);
				Assert.AreEqual("Forbidden", response.StatusDescription);

				throw;
			}
		}

		[Test,
		 ExpectedException(typeof(MonoRailException),
		 	"Access to the action [PostOnly] on controller [home] is not allowed to the http verb [GET].")]
		public void ActionWithPostOnlyAttributeCannotBeInvokedWithGET()
		{
			AccThrController controller = new AccThrController();

			request.HttpMethod = "GET";

			IControllerContext context = services.ControllerContextFactory.
				Create("", "home", "PostOnly", services.ControllerDescriptorProvider.BuildDescriptor(controller));

			try
			{
				controller.Process(engineContext, context);
			}
			catch(Exception)
			{
				Assert.AreEqual(403, response.StatusCode);
				Assert.AreEqual("Forbidden", response.StatusDescription);

				throw;
			}
		}

		[Test]
		public void ActionWithGetOrPostOnlyAttributeCanBeInvokedWithGETAndPOST()
		{
			AccThrController controller = new AccThrController();

			IControllerContext context = services.ControllerContextFactory.
				Create("", "home", "GetAndPost", services.ControllerDescriptorProvider.BuildDescriptor(controller));

			request.HttpMethod = "GET";
			controller.Process(engineContext, context);
			Assert.AreEqual("home\\GetAndPost", viewEngStub.TemplateRendered);

			request.HttpMethod = "POST";
			controller.Process(engineContext, context);
			Assert.AreEqual("home\\GetAndPost", viewEngStub.TemplateRendered);
		}

		#region Controllers

		private class AccThrController : Controller
		{
			[AccessibleThrough(Verb.Get)]
			public void GetOnly()
			{
			}

			[AccessibleThrough(Verb.Post)]
			public void PostOnly()
			{
			}

			[AccessibleThrough(Verb.Get | Verb.Post)]
			public void GetAndPost()
			{
			}
		}

		#endregion
	}
}