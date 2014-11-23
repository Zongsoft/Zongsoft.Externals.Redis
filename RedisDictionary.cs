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
	public class RedisDictionary : IRedisDictionary
	{
		#region 成员字段
		private string _name;
		private ServiceStack.Redis.IRedisClient _redis;
		#endregion

		#region 构造函数
		public RedisDictionary(string name, ServiceStack.Redis.IRedisClient redis)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			if(redis == null)
				throw new ArgumentNullException("redis");

			_name = name.Trim();
			_redis = redis;
		}
		#endregion

		public string Name
		{
			get
			{
				return _name;
			}
		}

		public int Count
		{
			get
			{
				return (int)_redis.GetHashCount(_name);
			}
		}

		public bool IsReadOnly
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
				return _redis.GetValueFromHash(_name, key);
			}
			set
			{
				_redis.SetEntryInHash(_name, key, value);
			}
		}

		public ICollection<string> Keys
		{
			get
			{
				return _redis.GetHashKeys(_name);
			}
		}

		public ICollection<string> Values
		{
			get
			{
				return _redis.GetHashValues(_name);
			}
		}

		public void AddRange(IEnumerable<KeyValuePair<string, string>> items)
		{
			_redis.SetRangeInHash(_name, items);
		}

		public bool TryAdd(string key, string value)
		{
			return _redis.SetEntryInHashIfNotExists(_name, key, value);
		}

		public long Increment(string key, int interval = 1)
		{
			if(interval < 1)
			{
				var text = _redis.GetValueFromHash(_name, key);
				long result;

				if(long.TryParse(text, out result))
					return result;
				else
					return -1;
			}

			return _redis.IncrementValueInHash(_name, key, interval);
		}

		public long Decrement(string key, int interval = 1)
		{
			if(interval < 1)
			{
				var text = _redis.GetValueFromHash(_name, key);
				long result;

				if(long.TryParse(text, out result))
					return result;
				else
					return -1;
			}

			return _redis.IncrementValueInHash(_name, key, -interval);
		}

		public void Add(string key, string value)
		{
			_redis.SetEntryInHashIfNotExists(_name, key, value);
		}

		public bool ContainsKey(string key)
		{
			return _redis.HashContainsEntry(_name, key);
		}

		public bool Remove(string key)
		{
			return _redis.RemoveEntryFromHash(_name, key);
		}

		public bool TryGetValue(string key, out string value)
		{
			value = _redis.GetValueFromHash(_name, key);
			return value != null;
		}

		public void Clear()
		{
			_redis.Remove(_name);
		}

		void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
		{
			_redis.SetEntryInHash(_name, item.Key, item.Value);
		}

		bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
		{
			throw new NotImplementedException();
		}

		void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
		{
			var entries = _redis.GetAllEntriesFromHash(_name);

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
