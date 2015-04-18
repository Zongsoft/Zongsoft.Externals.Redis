/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2015 Zongsoft Corporation <http://www.zongsoft.com>
 *
 * This file is part of Zongsoft.CoreLibrary.
 *
 * Zongsoft.CoreLibrary is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * Zongsoft.CoreLibrary is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 * Lesser General Public License for more details.
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Zongsoft.CoreLibrary; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

using System;
using System.Net;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Zongsoft.Externals.Redis
{
	internal static class RedisPoolManager
	{
		private static ConcurrentDictionary<string, Zongsoft.Collections.ObjectPool<ServiceStack.Redis.IRedisClient>> _cache;

		static RedisPoolManager()
		{
			_cache = new ConcurrentDictionary<string, Collections.ObjectPool<ServiceStack.Redis.IRedisClient>>();
		}

		public static Zongsoft.Collections.ObjectPool<ServiceStack.Redis.IRedisClient> GetRedisPool(IPEndPoint address, Func<ServiceStack.Redis.IRedisClient> creator)
		{
			return _cache.GetOrAdd(address.ToString(), new Collections.ObjectPool<ServiceStack.Redis.IRedisClient>(creator, p => p.Dispose(), 16));
		}
	}
}
