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

namespace Zongsoft.Externals.Redis
{
	public interface IRedisService : Zongsoft.Common.ISequence, Zongsoft.Collections.IQueueProvider, Zongsoft.Runtime.Caching.ICacheProvider, IDisposable
	{
		#region 公共属性
		/// <summary>
		/// 获取当前Redis数据库的记录总数。
		/// </summary>
		long Count
		{
			get;
		}

		/// <summary>
		/// 获取当前服务对应的Redis数据库编号。
		/// </summary>
		int DatabaseId
		{
			get;
		}

		/// <summary>
		/// 获取当前Redis服务的设置参数。
		/// </summary>
		RedisServiceSettings Settings
		{
			get;
		}

		/// <summary>
		/// 获取当前的Redis订阅器对象。
		/// </summary>
		RedisSubscriber Subscriber
		{
			get;
		}
		#endregion

		/// <summary>
		/// 切换当前数据库。
		/// </summary>
		/// <param name="databaseId">指定要切换的数据库编号。</param>
		/// <returns>如果切换成功则返回真(True)，否则返回假(False)。</returns>
		bool Use(int databaseId);

		/// <summary>
		/// 在当前库中查找满足指定模式的键名集。
		/// </summary>
		/// <param name="pattern">指定要查找的模式。</param>
		/// <returns>返回符合指定模式的键名集。</returns>
		IEnumerable<string> Find(string pattern);

		/// <summary>
		/// 获取指定键对应的字符串值。
		/// </summary>
		/// <param name="key">指定的键。</param>
		/// <returns>获取到的值，如果指定的键不存在则返回空(null)。</returns>
		string GetValue(string key);

		/// <summary>
		/// 获取指定多个键对应的字符串数组。
		/// </summary>
		/// <param name="keys">指定的多个键数组。</param>
		/// <returns>获取到的值数组，如果对应的某个键不存在，则值数组中对应位置的元素值为空(null)。</returns>
		string[] GetValues(params string[] keys);

		/// <summary>
		/// 将给定<paramref name="key"/>的值设为<paramref name="value"/>，并返回<paramref name="key"/>的旧值(old value)。
		/// </summary>
		/// <param name="key">指定的键。</param>
		/// <param name="value">指定要更新的值。</param>
		/// <returns>返回指定键的旧值，如果为空(null)则说明没有旧值，即指定的键在此之前还不存在。</returns>
		string ExchangeValue(string key, string value);

		/// <summary>
		/// 将给定<paramref name="key"/>的值设为<paramref name="value"/>，并返回<paramref name="key"/>的旧值(old value)。
		/// </summary>
		/// <param name="key">指定的键。</param>
		/// <param name="value">指定要更新的值。</param>
		/// <param name="duration">指定要设置的有效期限。</param>
		/// <returns>返回指定键的旧值，如果为空(null)则说明没有旧值，即指定的键在此之前还不存在。</returns>
		string ExchangeValue(string key, string value, TimeSpan duration);

		bool SetValue(string key, string value, bool requiredNotExists = false);
		bool SetValue(string key, string value, TimeSpan duration, bool requiredNotExists = false);

		/// <summary>
		/// 获取指定键的条目。
		/// </summary>
		/// <param name="key">指定要获取的键。</param>
		/// <returns>如果指定的键存在则返回对应的条目对象。</returns>
		object GetEntry(string key);
		T GetEntry<T>(string key, Func<object, T> convert = null);

		RedisEntryType GetEntryType(string key);
		TimeSpan? GetEntryExpiry(string key);
		bool SetEntryExpiry(string key, TimeSpan duration);
		bool SetEntryExpiry(string key, DateTime expires);

		new IRedisQueue GetQueue(string name);
		IRedisHashset GetHashset(string name);
		IRedisDictionary GetDictionary(string name);

		void Clear();
		bool Remove(string key);
		void RemoveMany(params string[] keys);

		bool Contains(string key);
		bool Rename(string oldKey, string newKey);

		/// <summary>
		/// 返回所有给定哈希集之间的交集。
		/// </summary>
		/// <param name="sets">指定的哈希集的名称数组。</param>
		/// <returns>返回的交集。</returns>
		HashSet<string> GetIntersect(params string[] sets);

		/// <summary>
		/// 将所有给定哈希集之间的交集保存到指定名称的哈希集中。
		/// </summary>
		/// <param name="destination">指定的目的哈希集名称，如果<paramref name="destination"/>哈希集已经存在则将其覆盖，可以指定为当前哈希集。</param>
		/// <param name="sets">指定的哈希集的名称数组。</param>
		long SetIntersect(string destination, params string[] sets);

		/// <summary>
		/// 返回所有给定哈希集之间的并集。
		/// </summary>
		/// <param name="sets">指定的哈希集的名称数组。</param>
		/// <returns>返回的并集。</returns>
		HashSet<string> GetUnion(params string[] sets);

		/// <summary>
		/// 将所有给定哈希集之间的并集保存到指定名称的哈希集中。
		/// </summary>
		/// <param name="destination">指定的目的哈希集名称，如果<paramref name="destination"/>哈希集已经存在则将其覆盖，可以指定为当前哈希集。</param>
		/// <param name="sets">指定的哈希集的名称数组。</param>
		long SetUnion(string destination, params string[] sets);

		/// <summary>
		/// 发送一条消息到指定的通道。
		/// </summary>
		/// <param name="channel">指定的消息通道。</param>
		/// <param name="message">要发送的消息。</param>
		/// <returns>返回接收到信息的订阅者数量。</returns>
		long Publish(string channel, string message);
	}
}
