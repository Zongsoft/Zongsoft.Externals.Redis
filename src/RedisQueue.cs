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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Zongsoft.Externals.Redis
{
	public class RedisQueue : RedisObjectBase, IRedisQueue
	{
		#region 事件定义
		public event EventHandler<Zongsoft.Collections.DequeuedEventArgs> Dequeued;
		public event EventHandler<Zongsoft.Collections.EnqueuedEventArgs> Enqueued;
		#endregion

		#region 构造函数
		public RedisQueue(string name, Zongsoft.Collections.ObjectPool<ServiceStack.Redis.IRedisClient> redisPool) : base(name, redisPool)
		{
		}
		#endregion

		#region 公共属性
		public int Capacity
		{
			get
			{
				return 0;
			}
		}

		public int Count
		{
			get
			{
				var redis = this.Redis;

				try
				{
					return (int)redis.GetListCount(this.Name);
				}
				finally
				{
					this.RedisPool.Release(redis);
				}
			}
		}

		bool ICollection.IsSynchronized
		{
			get
			{
				return false;
			}
		}

		private readonly object _syncRoot = new object();
		object ICollection.SyncRoot
		{
			get
			{
				return _syncRoot;
			}
		}
		#endregion

		#region 公共方法
		public void Clear()
		{
			var redis = this.Redis;

			try
			{
				redis.RemoveAllFromList(this.Name);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public object Dequeue()
		{
			var redis = this.Redis;
			string result = null;

			try
			{
				result = redis.RemoveStartFromList(this.Name);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}

			//激发“Dequeued”事件
			if(result != null)
				this.OnDequeued(new Zongsoft.Collections.DequeuedEventArgs(result, false, Collections.CollectionRemovedReason.Remove));

			return result;
		}

		public IEnumerable Dequeue(int length)
		{
			if(length < 1)
				throw new ArgumentOutOfRangeException("length");

			var count = Math.Min(length, this.Count);
			var redis = this.Redis;

			try
			{
				for(int i = 0; i < count; i++)
				{
					var result = redis.RemoveStartFromList(this.Name);

					//如果Redis队列返回值为空则表示队列已空
					if(result == null)
						break;

					//激发“Dequeued”事件
					this.OnDequeued(new Zongsoft.Collections.DequeuedEventArgs(result, false, Collections.CollectionRemovedReason.Remove));

					yield return result;
				}
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public void Enqueue(string value)
		{
			if(value == null)
				throw new ArgumentNullException("value");

			var redis = this.Redis;

			try
			{
				redis.AddItemToList(this.Name, value);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}

			//激发“Enqueued”事件
			this.OnEnqueued(new Zongsoft.Collections.EnqueuedEventArgs(value, false));
		}

		public void Enqueue(object value)
		{
			if(value == null)
				throw new ArgumentNullException("value");

			var redis = this.Redis;

			try
			{
				redis.AddItemToList(this.Name, this.ConvertValue(value));
			}
			finally
			{
				this.RedisPool.Release(redis);
			}

			//激发“Enqueued”事件
			this.OnEnqueued(new Zongsoft.Collections.EnqueuedEventArgs(value, false));
		}

		public void Enqueue<T>(IEnumerable<T> values)
		{
			if(values == null)
				throw new ArgumentNullException("values");

			List<string> list = values as List<string>;

			if(list == null)
			{
				if(typeof(T) == typeof(string))
				{
					list = new List<string>((IEnumerable<string>)values);
				}
				else
				{
					foreach(var value in values)
					{
						if(value == null)
							continue;

						list.Add(this.ConvertValue(value));
					}
				}
			}

			var redis = this.Redis;

			try
			{
				redis.AddRangeToList(this.Name, list);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}

			//激发“Enqueued”事件
			this.OnEnqueued(new Zongsoft.Collections.EnqueuedEventArgs(list, true));
		}

		public IEnumerable Peek(int length)
		{
			if(length < 1)
				throw new ArgumentOutOfRangeException("length");

			var redis = this.Redis;

			try
			{
				return redis.GetRangeFromList(this.Name, 0, length - 1);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public object Peek()
		{
			var redis = this.Redis;

			try
			{
				return redis.GetItemFromList(this.Name, 0);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public IEnumerable Take(int index, int length)
		{
			var redis = this.Redis;

			try
			{
				if(length > 0)
					return redis.GetRangeFromList(this.Name, index, index + length);
				else
					return redis.GetRangeFromList(this.Name, index, -1);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public object Take(int index)
		{
			var redis = this.Redis;

			try
			{
				return redis.GetItemFromList(this.Name, index);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}

		public void CopyTo(Array array, int index)
		{
			if(index < 0)
				throw new ArgumentOutOfRangeException("index");

			var redis = this.Redis;
			List<string> items = null;

			try
			{
				items = redis.GetRangeFromList(this.Name, index, index + array.Length);
			}
			finally
			{
				this.RedisPool.Release(redis);
			}

			Array.Copy(items.ToArray(), array, array.Length);
		}
		#endregion

		#region 激发事件
		protected virtual void OnDequeued(Zongsoft.Collections.DequeuedEventArgs args)
		{
			var dequeued = this.Dequeued;

			if(dequeued != null)
				dequeued(this, args);
		}

		protected virtual void OnEnqueued(Zongsoft.Collections.EnqueuedEventArgs args)
		{
			var enqueued = this.Enqueued;

			if(enqueued != null)
				enqueued(this, args);
		}
		#endregion

		#region 私有方法
		private string ConvertValue(object value)
		{
			if(value == null)
				return null;

			if(value.GetType() == typeof(string))
				return (string)value;

			if(value.GetType().IsPrimitive || value.GetType().IsEnum || value is StringBuilder)
				return value.ToString();

			var serializable = value as Zongsoft.Runtime.Serialization.ISerializable;

			if(serializable != null)
			{
				using(var stream = new MemoryStream())
				{
					serializable.Serialize(stream);
					return Encoding.UTF8.GetString(stream.ToArray());
				}
			}

			return Zongsoft.Common.Convert.ConvertValue<string>(value, () =>
			{
				using(var stream = new MemoryStream())
				{
					Zongsoft.Runtime.Serialization.Serializer.Json.Serialize(stream, value);
					return Encoding.UTF8.GetString(stream.ToArray());
				}
			});
		}
		#endregion

		#region 遍历枚举
		public System.Collections.IEnumerator GetEnumerator()
		{
			var count = this.Count;
			var redis = this.Redis;

			try
			{
				for(int i = 0; i < count; i++)
				{
					var result = redis.GetItemFromList(this.Name, i);

					if(result != null)
						yield return result;
				}
			}
			finally
			{
				this.RedisPool.Release(redis);
			}
		}
		#endregion
	}
}
