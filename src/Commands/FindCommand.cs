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

namespace Zongsoft.Externals.Redis.Commands
{
	public class FindCommand : RedisCommandBase
	{
		#region 构造函数
		public FindCommand() : base("Find")
		{
		}

		public FindCommand(IRedisService redis) : base(redis, "Find")
		{
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(Services.CommandContext context)
		{
			if(context.Expression.Arguments.Length == 0)
				throw new Zongsoft.Services.CommandException("Missing arguments.");

			var result = new IEnumerable<string>[context.Expression.Arguments.Length];

			for(int i = 0; i < context.Expression.Arguments.Length; i++)
			{
				//查找指定模式的键名集
				result[i] = this.Redis.Find(context.Expression.Arguments[i]);

				//打印模式字符串
				context.Output.WriteLine(Services.CommandOutletColor.Magenta, context.Expression.Arguments[i]);

				//定义遍历序号
				var index = 1;

				foreach(var key in result[i])
				{
					context.Output.Write(Services.CommandOutletColor.DarkGray, $"[{index++}] ");
					context.Output.WriteLine(Services.CommandOutletColor.Green, key);
				}

				context.Output.WriteLine();
			}

			return result;
		}
		#endregion
	}
}
