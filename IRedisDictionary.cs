﻿/*
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
	public interface IRedisDictionary : IDictionary<string, string>
	{
		/// <summary>
		/// 获取当前<seealso cref="IRedisDictionary"/>字典的名称。
		/// </summary>
		string Name
		{
			get;
		}

		/// <summary>
		/// 批量新增指定的键值对集合到字典中。
		/// </summary>
		/// <param name="items"></param>
		void AddRange(IEnumerable<KeyValuePair<string, string>> items);

		/// <summary>
		/// 尝试新增一个指定的键值对，如果指定的键已存在则不执行任何操作并返回假(false)。
		/// </summary>
		/// <param name="key">要新增的键。</param>
		/// <param name="value">要新增的值。</param>
		/// <returns>如果新增成功则返回真(true)，否则返回假(false)。</returns>
		bool TryAdd(string key, string value);

		long Increment(string key, int interval = 1);
		long Decrement(string key, int interval = 1);
	}
}
