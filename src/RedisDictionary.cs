/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2014 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Collections.Generic;

namespace Zongsoft.Externals.Redis
{
	public class RedisDictionary : RedisObjectBase, IRedisDictionary
	{
		#region 构造函数
		public RedisDictionary(string name, Zongsoft.Collections.ObjectPool<ServiceStack.Redis.IRedisClient> redisPool) : base(name, redisPool)
		{
		}
		#endregion

		#region 公共属性
		public int Count
		{
			get
			{
				var redis = this.Redis;

				try
				{
					return (int)redis.GetHashCount(this.Name);
				}
				finally
				{
					this.RedisPool.Release(redis);
				}
			}
		}

		bool ICollection<KeyValuePair<string, string>>.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public string this[string key]
		{
			get
			{
				var redis = this.Redis;

				try
				{
					return redis.GetValueFromHash(this.Name, key);
				}
				finally
				{
					this.RedisPool.Release(redis);
				}
			}
			set
			{
				var redis = this.Redis;

				try
				{
					redis.SetEntryInHash(this.Name, key, value);
				}
				finally
				{
					this.RedisPool.Release(redis);
				}
			}
		}

		public ICollection<string> Keys
		{
			get
			{
				var redis = this.Redis;

				try
				{
					return redis.GetHashKeys(this.Name);
				}
				finally
				{
					this.RedisPool.Release(redis);
				}
			}
		}

		public ICollection<string> Values
		{
			get
			{
				var redis = this.Redis;

				try
				{
					return redis.GetHashValues(this.Name);
				}
				finally
				{
					this.RedisPool.Release(redis);
				}
			}
		}
		#endregion

		#region 公共方法
		public void SetRange(IEnumerable<KeyValuePair<string, string>> items)
		{
			var redis = this.Redis;

			try
			{
				redis.SetRangeInHash(this.Name, items);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public bool TryAdd(string key, string value)
		{
			var redis = this.Redis;

			try
			{
				return redis.SetEntryInHashIfNotExists(this.Name, key, value);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public IReadOnlyList<string> GetValues(params string[] keys)
		{
			var redis = this.Redis;

			try
			{
				return redis.GetValuesFromHash(this.Name, keys);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public IReadOnlyDictionary<string, string> GetAllEntries()
		{
			var redis = this.Redis;

			try
			{
				return redis.GetAllEntriesFromHash(this.Name);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public long Increment(string key, int interval = 1)
		{
			var redis = this.Redis;

			try
			{
				return redis.IncrementValueInHash(this.Name, key, interval);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public long Decrement(string key, int interval = 1)
		{
			var redis = this.Redis;

			try
			{
				return redis.IncrementValueInHash(this.Name, key, -interval);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public void Add(string key, string value)
		{
			var redis = this.Redis;

			try
			{
				if(!redis.SetEntryInHashIfNotExists(this.Name, key, value))
					throw new RedisException(string.Format("The '{1}' key of entry is existed in the '{0}' dictionary.", this.Name, key));
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public bool ContainsKey(string key)
		{
			var redis = this.Redis;

			try
			{
				return redis.HashContainsEntry(this.Name, key);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public bool Remove(string key)
		{
			var redis = this.Redis;

			try
			{
				return redis.RemoveEntryFromHash(this.Name, key);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public bool TryGetValue(string key, out string value)
		{
			var redis = this.Redis;

			try
			{
				value = redis.GetValueFromHash(this.Name, key);
				return value != null;
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public void Clear()
		{
			var redis = this.Redis;

			try
			{
				redis.Remove(this.Name);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}
		#endregion

		#region 显式实现
		void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
		{
			this.Add(item.Key, item.Value);
		}

		bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
		{
			if(item.Key == null)
				return false;

			return this.ContainsKey(item.Key);
		}

		void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
		{
			if(item.Key == null)
				return false;

			return this.Remove(item.Key);
		}
		#endregion

		#region 遍历枚举
		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
		{
			Dictionary<string, string> entries;
			var redis = this.Redis;

			try
			{
				entries = redis.GetAllEntriesFromHash(this.Name);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}

			foreach(var entry in entries)
			{
				yield return entry;
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		#endregion
	}
}
