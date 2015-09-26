/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		public static string GetStoreString(object value)
		{
			string text;

			if(Zongsoft.Common.TypeExtension.IsScalarType(value.GetType()))
				text = value.ToString();
			else
				text = Zongsoft.Runtime.Serialization.Serializer.Json.Serialize(value);

			return text;
		}
	}
}
