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

namespace Castle.Facilities.EventWiring
{
	using System.Collections.Generic;
	using System.Reflection;

	using Castle.Core;
	using Castle.MicroKernel.LifecycleConcerns;

	public class UnwireEventPublisher : ILifecycleConcern
	{
		private readonly EventWiringFacility facility;

		public UnwireEventPublisher(EventWiringFacility facility)
		{
			this.facility = facility;
		}

		public void Apply(ComponentModel model, object component)
		{
			var events = model.ExtendedProperties["publishedEvents"] as IDictionary<string, EventInfo>;
			foreach (var @event in events)
			{
				facility.UnwireEvent(@event.Key, @event.Value, component);
			}
		}
	}
}