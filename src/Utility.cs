/*
 * Authors:
 *   钟峰(Popeye Zhong) <9555843@qq.com>
 *
 * Copyright (C) 2014-2015 Zongsoft Corporation <http://www.zongsoft.com>
 *
 * This file is part of Zongsoft.Externals.Redis.
 *
 * Zongsoft.Externals.Redis is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * Zongsoft.Externals.Redis is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 * Lesser General Public License for more details.
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Zongsoft.Externals.Redis; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

using System;

using StackExchange.Redis;

namespace Zongsoft.Externals.Redis
{
	internal static class Utility
	{
		public static T ConvertValue<T>(object value)
		{
			if(value == null)
				return default(T);

			if(typeof(T) == typeof(string) || Zongsoft.Common.TypeExtension.IsScalarType(typeof(T)))
				return Zongsoft.Common.Convert.ConvertValue<T>(value);

			if(value is string)
				return Zongsoft.Runtime.Serialization.Serializer.Json.Deserialize<T>((string)value);

			//强制转换，可能会导致无效转换异常
			return (T)value;
		}

		public static StackExchange.Redis.RedisValue GetStoredValue(object value)
		{
			if(value == null)
				return RedisValue.Null;

			var type = value.GetType();

			if(type == typeof(string))
				return (string)value;

			if(type.IsPrimitive || type.IsEnum ||
			   type == typeof(decimal) || type == typeof(DateTime) || type == typeof(DateTimeOffset) ||
			   type == typeof(TimeSpan) || type == typeof(Guid) || value is System.Text.StringBuilder)
				return value.ToString();

			var serializable = value as Zongsoft.Runtime.Serialization.ISerializable;

			if(serializable != null)
			{
				using(var stream = new System.IO.MemoryStream())
				{
					serializable.Serialize(stream);
					return stream.ToArray();
				}
			}

			return Zongsoft.Common.Convert.ConvertValue<string>(value, () =>
			{
				return Zongsoft.Runtime.Serialization.Serializer.Json.Serialize(value);
			});
		}
	}
}
