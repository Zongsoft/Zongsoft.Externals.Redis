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
	public interface IRedisHashset : ICollection<string>
	{
		/// <summary>
		/// 获取当前<seealso cref="IRedisHashset"/>哈希集的名称。
		/// </summary>
		string Name
		{
			get;
		}

		/// <summary>
		/// 刷新当前哈希集，当与远程Redis服务器连接中断后可使用该方法手动连接。
		/// </summary>
		/// <param name="timeout">刷新的超时，如果为零则表示无超时限制。</param>
		void Refresh(TimeSpan timeout);

		/// <summary>
		/// 返回当前哈希集与所有给定哈希集之间的差集。
		/// </summary>
		/// <param name="other">指定的其他哈希集的名称数组。</param>
		/// <returns>返回的差集。</returns>
		HashSet<string> GetExcept(params string[] other);

		/// <summary>
		/// 保存当前哈希集与所有给定哈希集之间的差集到指定名称的哈希集中。
		/// </summary>
		/// <param name="destination">指定的目的哈希集名称，如果<paramref name="destination"/>哈希集已经存在则将其覆盖，可以指定为当前哈希集。</param>
		/// <param name="other">指定的其他哈希集的名称数组。</param>
		void SetExcept(string destination, params string[] other);

		/// <summary>
		/// 返回当前哈希集与所有给定哈希集之间的交集。
		/// </summary>
		/// <param name="other">指定的其他哈希集的名称数组。</param>
		/// <returns>返回的交集。</returns>
		HashSet<string> GetIntersect(params string[] other);

		/// <summary>
		/// 保存当前哈希集与所有给定哈希集之间的交集到指定名称的哈希集中。
		/// </summary>
		/// <param name="destination">指定的目的哈希集名称，如果<paramref name="destination"/>哈希集已经存在则将其覆盖，可以指定为当前哈希集。</param>
		/// <param name="other">指定的其他哈希集的名称数组。</param>
		void SetIntersect(string destination, params string[] other);

		/// <summary>
		/// 返回当前哈希集与所有给定哈希集之间的并集。
		/// </summary>
		/// <param name="other">指定的其他哈希集的名称数组。</param>
		/// <returns>返回的并集。</returns>
		HashSet<string> GetUnion(params string[] other);

		/// <summary>
		/// 保存当前哈希集与所有给定哈希集之间的并集到指定名称的哈希集中。
		/// </summary>
		/// <param name="destination">指定的目的哈希集名称，如果<paramref name="destination"/>哈希集已经存在则将其覆盖，可以指定为当前哈希集。</param>
		/// <param name="other">指定的其他哈希集的名称数组。</param>
		void SetUnion(string destination, params string[] other);

		/// <summary>
		/// 返回指定个数的随机成员集，结果集是随机且不重复的。
		/// </summary>
		/// <param name="count">指定要获取的成员数量。</param>
		/// <returns>返回的随机结果集。</returns>
		HashSet<string> GetRandomValues(int count);

		/// <summary>
		/// 将当前哈希集中指定的成员移动到目的哈希集中，如果目标哈希集已经存在指定的成员则将当前哈希集中的该成员删除。
		/// </summary>
		/// <param name="destination">指定的目的哈希集名称。</param>
		/// <param name="item">指定的当前哈希集的成员。</param>
		/// <returns>如果移动成功返回真(true)，否则返回假(false)。</returns>
		bool Move(string destination, string item);

		/// <summary>
		/// 批量添加多个元素到哈希集中。
		/// </summary>
		/// <param name="items">指定要批量添加的元素。</param>
		void AddRange(params string[] items);

		/// <summary>
		/// 删除指定的多个元素。
		/// </summary>
		/// <param name="items">指定的多个元素。</param>
		void RemoveRange(params string[] items);
	}
}
