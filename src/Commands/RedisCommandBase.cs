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

namespace Zongsoft.Externals.Redis.Commands
{
	public abstract class RedisCommandBase : Zongsoft.Services.CommandBase<Zongsoft.Services.CommandContext>
	{
		#region 成员字段
		private IRedisService _redis;
		#endregion

		#region 构造函数
		protected RedisCommandBase(string name) : base(name)
		{
		}

		protected RedisCommandBase(IRedisService redis, string name) : base(name)
		{
			_redis = redis;
		}
		#endregion

		#region 公共属性
		public IRedisService Redis
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
		#endregion

		#region 重写方法
		public override bool CanExecute(Services.CommandContext parameter)
		{
			return _redis != null && base.CanExecute(parameter);
		}
		#endregion
	}
}
