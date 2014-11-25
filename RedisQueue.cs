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
	public class RedisQueue : IRedisQueue, Zongsoft.Collections.IQueue
	{
		#region 事件定义
		public event EventHandler<Zongsoft.Collections.DequeuedEventArgs> Dequeued;
		public event EventHandler<Zongsoft.Collections.EnqueuedEventArgs> Enqueued;
		#endregion

		#region 成员字段
		private string _name;
		private ServiceStack.Redis.IRedisClient _redis;
		#endregion

		#region 构造函数
		public RedisQueue(string name, ServiceStack.Redis.IRedisClient redis)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			if(redis == null)
				throw new ArgumentNullException("redis");

			_name = name.Trim();
			_redis = redis;
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取或设置队列的名称。
		/// </summary>
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if(string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException();

				_name = value.Trim();
			}
		}

		/// <summary>
		/// 获取或设置本队列所依附的<seealso cref="ServiceStack.Redis.IRedisClient"/>对象。
		/// </summary>
		public ServiceStack.Redis.IRedisClient Redis
		{
			get
			{
				return _redis;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_redis = value;
			}
		}

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
				return (int)_redis.GetListCount(_name);
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
			_redis.RemoveAllFromList(_name);
		}

		public object Dequeue()
		{
			var result = _redis.RemoveStartFromList(_name);

			//激发“Dequeued”事件
			this.OnDequeued(new Zongsoft.Collections.DequeuedEventArgs(result, false, Collections.CollectionRemovedReason.Remove));

			return result;
		}

		public IEnumerable Dequeue(int length)
		{
			if(length < 1)
				throw new ArgumentOutOfRangeException("length");

			for(int i = 0; i < length; i++)
			{
				var result = _redis.RemoveStartFromList(_name);

				//激发“Dequeued”事件
				this.OnDequeued(new Zongsoft.Collections.DequeuedEventArgs(result, false, Collections.CollectionRemovedReason.Remove));

				yield return result;
			}
		}

		public void Enqueue(string value)
		{
			if(value == null)
				throw new ArgumentNullException("value");

			_redis.AddItemToList(_name, value);

			//激发“Enqueued”事件
			this.OnEnqueued(new Zongsoft.Collections.EnqueuedEventArgs(value, false));
		}

		public void Enqueue(IEnumerable<string> values)
		{
			if(values == null)
				throw new ArgumentNullException("values");

			_redis.AddRangeToList(_name, System.Linq.Enumerable.ToList(values));

			//激发“Enqueued”事件
			this.OnEnqueued(new Zongsoft.Collections.EnqueuedEventArgs(values, true));
		}

		public void Enqueue(object value)
		{
			if(value == null)
				throw new ArgumentNullException("value");

			_redis.AddItemToList(_name, this.ConvertValue(value));

			//激发“Enqueued”事件
			this.OnEnqueued(new Zongsoft.Collections.EnqueuedEventArgs(value, false));
		}

		public void Enqueue(IEnumerable values)
		{
			if(values == null)
				throw new ArgumentNullException("values");

			var list = new List<string>();

			foreach(var value in values)
			{
				if(value == null)
					continue;

				list.Add(this.ConvertValue(value));
			}

			_redis.AddRangeToList(_name, list);

			//激发“Enqueued”事件
			this.OnEnqueued(new Zongsoft.Collections.EnqueuedEventArgs(list, true));
		}

		public IEnumerable Peek(int length)
		{
			if(length < 1)
				throw new ArgumentOutOfRangeException("length");

			return _redis.GetRangeFromList(_name, 0, length - 1);
		}

		public object Peek()
		{
			return _redis.GetItemFromList(_name, 0);
		}

		public IEnumerable Take(int index, int length)
		{
			if(length > 0)
				return _redis.GetRangeFromList(_name, index, index + length);
			else
				return _redis.GetRangeFromList(_name, index, -1);
		}

		public object Take(int index)
		{
			return _redis.GetItemFromList(_name, index);
		}

		public void CopyTo(Array array, int index)
		{
			if(index < 0)
				throw new ArgumentOutOfRangeException("index");

			var items = _redis.GetRangeFromList(_name, index, index + array.Length);
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

			if(value == typeof(string))
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

			for(int i = 0; i < count; i++)
			{
				var result = _redis.GetItemFromList(_name, i);

				if(result != null)
					yield return result;
			}
		}
		#endregion
	}
}
