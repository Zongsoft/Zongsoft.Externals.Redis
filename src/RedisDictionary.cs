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
		public RedisDictionary(string name, ServiceStack.Redis.IRedisClient redis) : base(name, redis)
		{
		}

		public RedisDictionary(string name, Zongsoft.Common.IObjectReference<ServiceStack.Redis.IRedisClient> redisReference) : base(name, redisReference)
		{
		}
		#endregion

		public int Count
		{
			get
			{
				return (int)this.Redis.GetHashCount(this.Name);
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
				return this.Redis.GetValueFromHash(this.Name, key);
			}
			set
			{
				this.Redis.SetEntryInHash(this.Name, key, value);
			}
		}

		public ICollection<string> Keys
		{
			get
			{
				return this.Redis.GetHashKeys(this.Name);
			}
		}

		public ICollection<string> Values
		{
			get
			{
				return this.Redis.GetHashValues(this.Name);
			}
		}

		public void SetRange(IEnumerable<KeyValuePair<string, string>> items)
		{
			this.Redis.SetRangeInHash(this.Name, items);
		}

		public bool TryAdd(string key, string value)
		{
			return this.Redis.SetEntryInHashIfNotExists(this.Name, key, value);
		}

		public IEnumerable<string> GetValues(params string[] keys)
		{
			return this.Redis.GetValuesFromHash(this.Name, keys);
		}

		public long Increment(string key, int interval = 1)
		{
			if(interval < 1)
			{
				var text = this.Redis.GetValueFromHash(this.Name, key);
				long result;

				if(long.TryParse(text, out result))
					return result;
				else
					return -1;
			}

			return this.Redis.IncrementValueInHash(this.Name, key, interval);
		}

		public long Decrement(string key, int interval = 1)
		{
			if(interval < 1)
			{
				var text = this.Redis.GetValueFromHash(this.Name, key);
				long result;

				if(long.TryParse(text, out result))
					return result;
				else
					return -1;
			}

			return this.Redis.IncrementValueInHash(this.Name, key, -interval);
		}

		public void Add(string key, string value)
		{
			if(!this.Redis.SetEntryInHashIfNotExists(this.Name, key, value))
				throw new RedisException(string.Format("The '{1}' key of entry is existed in the '{0}' dictionary.", this.Name, key));
		}

		public bool ContainsKey(string key)
		{
			return this.Redis.HashContainsEntry(this.Name, key);
		}

		public bool Remove(string key)
		{
			return this.Redis.RemoveEntryFromHash(this.Name, key);
		}

		public bool TryGetValue(string key, out string value)
		{
			value = this.Redis.GetValueFromHash(this.Name, key);
			return value != null;
		}

		public void Clear()
		{
			this.Redis.Remove(this.Name);
		}

		void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
		{
			this.Add(item.Key, item.Value);
		}

		bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
		{
			if(item.Key == null)
				return false;

			return this.Redis.HashContainsEntry(this.Name, item.Key);
		}

		void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
		{
			if(item.Key == null)
				return false;

			return this.Redis.RemoveEntryFromHash(this.Name, item.Key);
		}

		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
		{
			var entries = this.Redis.GetAllEntriesFromHash(this.Name);

			foreach(var entry in entries)
			{
				yield return entry;
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
