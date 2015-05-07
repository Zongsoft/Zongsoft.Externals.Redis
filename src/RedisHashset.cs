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

namespace Zongsoft.Externals.Redis
{
	public class RedisHashset : RedisObjectBase, IRedisHashset
	{
		#region 构造函数
		public RedisHashset(string name, Zongsoft.Collections.ObjectPool<ServiceStack.Redis.IRedisClient> redisPool) : base(name, redisPool)
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
					return (int)redis.GetSetCount(this.Name);
				}
				finally
				{
					this.RedisPool.Release(redis);
				}
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}
		#endregion

		#region 公共方法
		public HashSet<string> GetExcept(params string[] other)
		{
			var redis = this.Redis;

			try
			{
				return redis.GetDifferencesFromSet(this.Name, other);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public void SetExcept(string destination, params string[] other)
		{
			var redis = this.Redis;

			try
			{
				redis.StoreDifferencesFromSet(destination, this.Name, other);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public HashSet<string> GetIntersect(params string[] other)
		{
			var sets = new string[other.Length + 1];
			sets[0] = this.Name;
			Array.Copy(other, 0, sets, 1, other.Length);

			var redis = this.Redis;

			try
			{
				return redis.GetIntersectFromSets(sets);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public void SetIntersect(string destination, params string[] other)
		{
			var sets = new string[other.Length + 1];
			sets[0] = this.Name;
			Array.Copy(other, 0, sets, 1, other.Length);

			var redis = this.Redis;

			try
			{
				redis.StoreIntersectFromSets(destination, sets);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public HashSet<string> GetUnion(params string[] other)
		{
			var sets = new string[other.Length + 1];
			sets[0] = this.Name;
			Array.Copy(other, 0, sets, 1, other.Length);

			var redis = this.Redis;

			try
			{
				return redis.GetUnionFromSets(sets);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public void SetUnion(string destination, params string[] other)
		{
			var sets = new string[other.Length + 1];
			sets[0] = this.Name;
			Array.Copy(other, 0, sets, 1, other.Length);

			var redis = this.Redis;

			try
			{
				redis.StoreUnionFromSets(destination, sets);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public HashSet<string> GetRandomValues(int count)
		{
			var result = new HashSet<string>();
			var redis = this.Redis;

			try
			{
				for(int i = 0; i < Math.Max(1, count); i++)
				{
					result.Add(redis.GetRandomItemFromSet(this.Name));
				}
			}
			finally
			{
				this.RedisPool.Release(redis);
			}

			return result;
		}

		public bool Move(string destination, string item)
		{
			var redis = this.Redis;

			try
			{
				redis.MoveBetweenSets(this.Name, destination, item);
				return true;
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public void RemoveRange(params string[] items)
		{
			var redis = this.Redis;
			var transaction = redis.CreateTransaction();

			try
			{
				foreach(var item in items)
				{
					transaction.QueueCommand(proxy => proxy.RemoveEntryFromHash(this.Name, item));
				}

				transaction.Commit();
			}
			finally
			{
				if(transaction != null)
					transaction.Dispose();

				this.RedisPool.Release(redis);
			}
		}

		public bool Remove(string item)
		{
			var redis = this.Redis;

			try
			{
				redis.RemoveItemFromSet(this.Name, item);
				return true;
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public void Add(string item)
		{
			var redis = this.Redis;

			try
			{
				redis.AddItemToSet(this.Name, item);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public void AddRange(params string[] items)
		{
			var redis = this.Redis;

			try
			{
				redis.AddRangeToSet(this.Name, System.Linq.Enumerable.ToList(items));
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

		public bool Contains(string item)
		{
			var redis = this.Redis;

			try
			{
				return redis.SetContainsItem(this.Name, item);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}
		#endregion

		#region 显式实现
		void ICollection<string>.CopyTo(string[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region 遍历枚举
		public IEnumerator<string> GetEnumerator()
		{
			HashSet<string> items;
			var redis = this.Redis;

			try
			{
				items = redis.GetAllItemsFromSet(this.Name);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}

			foreach(var item in items)
			{
				yield return item;
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		#endregion
	}
}
