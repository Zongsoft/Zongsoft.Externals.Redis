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
	public class RedisHashset : IRedisHashset
	{
		#region 成员字段
		private string _name;
		private ServiceStack.Redis.IRedisClient _redis;
		#endregion

		#region 构造函数
		public RedisHashset(string name, ServiceStack.Redis.IRedisClient redis)
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
				return (int)_redis.GetSetCount(_name);
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public HashSet<string> GetExcept(params string[] other)
		{
			return _redis.GetDifferencesFromSet(_name, other);
		}

		public void SetExcept(string destination, params string[] other)
		{
			_redis.StoreDifferencesFromSet(destination, _name, other);
		}

		public HashSet<string> GetIntersect(params string[] other)
		{
			var sets = new string[other.Length + 1];
			sets[0] = _name;
			Array.Copy(other, 0, sets, 1, other.Length);

			return _redis.GetIntersectFromSets(sets);
		}

		public void SetIntersect(string destination, params string[] other)
		{
			var sets = new string[other.Length + 1];
			sets[0] = _name;
			Array.Copy(other, 0, sets, 1, other.Length);

			_redis.StoreIntersectFromSets(destination, sets);
		}

		public HashSet<string> GetUnion(params string[] other)
		{
			var sets = new string[other.Length + 1];
			sets[0] = _name;
			Array.Copy(other, 0, sets, 1, other.Length);

			return _redis.GetUnionFromSets(sets);
		}

		public void SetUnion(string destination, params string[] other)
		{
			var sets = new string[other.Length + 1];
			sets[0] = _name;
			Array.Copy(other, 0, sets, 1, other.Length);

			_redis.StoreUnionFromSets(destination, sets);
		}

		public HashSet<string> GetRandomValues(int count)
		{
			var result = new HashSet<string>();

			for(int i = 0; i < Math.Max(1, count); i++)
			{
				result.Add(_redis.GetRandomItemFromSet(_name));
			}

			return result;
		}

		public bool Move(string destination, string item)
		{
			_redis.MoveBetweenSets(_name, destination, item);
			return true;
		}

		public void RemoveRange(params string[] items)
		{
			using(var transaction = _redis.CreateTransaction())
			{
				foreach(var item in items)
				{
					transaction.QueueCommand(proxy => proxy.RemoveEntryFromHash(_name, item));
				}

				transaction.Commit();
			}
		}

		public bool Remove(string item)
		{
			_redis.RemoveItemFromSet(_name, item);
			return true;
		}

		public void Add(string item)
		{
			_redis.AddItemToSet(_name, item);
		}

		public void AddRange(params string[] items)
		{
			_redis.AddRangeToSet(_name, System.Linq.Enumerable.ToList(items));
		}

		public void Clear()
		{
			_redis.Remove(_name);
		}

		public bool Contains(string item)
		{
			return _redis.SetContainsItem(_name, item);
		}

		void ICollection<string>.CopyTo(string[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<string> GetEnumerator()
		{
			var items = _redis.GetAllItemsFromSet(_name);

			foreach(var item in items)
			{
				yield return item;
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
