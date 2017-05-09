/*
 * Authors:
 *   钟峰(Popeye Zhong) <9555843@qq.com>
 *
 * Copyright (C) 2014-2017 Zongsoft Corporation <http://www.zongsoft.com>
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

using StackExchange.Redis;

namespace Zongsoft.Externals.Redis
{
	internal static class RedisExtensions
	{
		public static string[] ToStringArray(this RedisValue[] values)
		{
			return ExtensionMethods.ToStringArray(values);
		}

		public static RedisKey[] ToRedisKeys(this string[] keys)
		{
			if(keys == null)
				throw new ArgumentNullException(nameof(keys));

			var result = new RedisKey[keys.Length];

			for(int i = 0; i < keys.Length; i++)
			{
				result[i] = keys[i];
			}

			return result;
		}

		public static RedisValue[] ToRedisValues(this string[] values)
		{
			if(values == null)
				throw new ArgumentNullException(nameof(values));

			var result = new RedisValue[values.Length];

			for(int i = 0; i < values.Length; i++)
			{
				result[i] = values[i];
			}

			return result;
		}
	}
}
