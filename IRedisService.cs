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
using System.Net;
using System.Collections.Generic;

namespace Zongsoft.Externals.Redis
{
	public interface IRedisService : IDisposable
	{
		#region 公共属性
		/// <summary>
		/// 获取或设置当前Redis服务器的地址。
		/// </summary>
		IPEndPoint Address
		{
			get;
			set;
		}

		/// <summary>
		/// 获取或设置当前服务操作的Redis数据库编号。
		/// </summary>
		int DatabaseId
		{
			get;
			set;
		}
		#endregion

		#region 获取集合
		IRedisHashset GetHashset(string name);
		IRedisDictionary GetDictionary(string name);
		IRedisQueue GetQueue(string name);
		#endregion

		/// <summary>
		/// 刷新当前Redis服务，当与远程Redis服务器连接中断后可使用该方法手动连接。
		/// </summary>
		/// <param name="timeout">刷新的超时，如果为零则表示无超时限制。</param>
		void Refresh(TimeSpan timeout);

		string GetValue(string key);
		IEnumerable<string> GetValues(params string[] keys);

		string ExchangeValue(string key, string value);
		string ExchangeValue(string key, string value, DateTime expires);
		string ExchangeValue(string key, string value, TimeSpan duration);

		bool SetValue(string key, string value);
		bool SetValue(string key, string value, DateTime expires, bool requiredNotExists = false);
		bool SetValue(string key, string value, TimeSpan duration, bool requiredNotExists = false);

		RedisEntryType GetEntryType(string key);
		TimeSpan GetEntryExpire(string key);
		bool SetEntryExpire(string key, DateTime expires);
		bool SetEntryExpire(string key, TimeSpan duration);

		void Clear();
		bool Remove(string key);
		void RemoveRange(params string[] keys);

		bool Contains(string key);
		void Rename(string oldKey, string newKey);

		long Increment(string key, int interval = 1);
		long Decrement(string key, int interval = 1);

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
		void SetIntersect(string destination, params string[] sets);

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
		void SetUnion(string destination, params string[] sets);
	}
}
