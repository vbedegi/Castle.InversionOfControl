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


namespace Castle.MicroKernel.SubSystems.Conversion
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;

	using Castle.Core.Configuration;

#if (!SILVERLIGHT)
	[Serializable]
#endif
	public class DictionaryConverter : AbstractTypeConverter
	{
		public override bool CanHandleType(Type type)
        {
#if (SILVERLIGHT)
			return (type == typeof(IDictionary));
#else
            return (type == typeof(IDictionary) || type == typeof(Hashtable));
#endif
		}

		public override object PerformConversion(String value, Type targetType)
		{
			throw new NotImplementedException();
		}

		public override object PerformConversion(IConfiguration configuration, Type targetType)
		{
			Debug.Assert(CanHandleType(targetType), "CanHandleType(targetType)");

			var dict = new Dictionary<object, object>();

			String keyTypeName = configuration.Attributes["keyType"];
			Type defaultKeyType = typeof(String);
			
			String valueTypeName = configuration.Attributes["valueType"];
			Type defaultValueType = typeof(String);

			if (keyTypeName != null)
			{
				defaultKeyType = Context.Composition.PerformConversion<Type>( keyTypeName);
			}
			if (valueTypeName != null)
			{
				defaultValueType = Context.Composition.PerformConversion<Type>(valueTypeName);
			}

			foreach(IConfiguration itemConfig in configuration.Children)
			{
				// Preparing the key

				String keyValue = itemConfig.Attributes["key"];

				if (keyValue == null)
				{
					throw new ConverterException("You must provide a key for the dictionary entry");
				}

				Type convertKeyTo = defaultKeyType;

				if (itemConfig.Attributes["keyType"] != null)
				{
					convertKeyTo = Context.Composition.PerformConversion<Type>(itemConfig.Attributes["keyType"]);
				}

				object key = Context.Composition.PerformConversion(keyValue, convertKeyTo);

				// Preparing the value

				Type convertValueTo = defaultValueType;

				if (itemConfig.Attributes["valueType"] != null)
				{
					convertValueTo = Context.Composition.PerformConversion<Type>(itemConfig.Attributes["valueType"]);
				}

				object value;

				if (itemConfig.Children.Count == 0)
				{
					value = Context.Composition.PerformConversion(
						itemConfig, convertValueTo);
				}
				else
				{
					value = Context.Composition.PerformConversion(
						itemConfig.Children[0], convertValueTo);
				}

				dict.Add( key, value );
			}

			return dict;
		}
	}
}
