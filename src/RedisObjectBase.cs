/*
 * Authors:
 *   钟峰(Popeye Zhong) <9555843@qq.com>
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
	public abstract class RedisObjectBase : MarshalByRefObject
	{
		#region 成员字段
		private string _name;
		private StackExchange.Redis.IDatabase _database;
		#endregion

		#region 构造函数
		protected RedisObjectBase(string name, StackExchange.Redis.IDatabase database)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));

			if(database == null)
				throw new ArgumentNullException(nameof(database));

			_name = name.Trim();
			_database = database;
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取或设置Redis对象的名称。
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
		#endregion

		#region 保护属性
		/// <summary>
		/// 获取或设置当前Redis对象所依附的Redis数据库。
		/// </summary>
		protected StackExchange.Redis.IDatabase Database
		{
			get
			{
				return _database;
			}
		}
		#endregion

		#region 虚拟方法
		protected virtual StackExchange.Redis.RedisValue GetStoredValue(object value)
		{
			return Utility.GetStoredValue(value);
		}
		#endregion
	}
}
