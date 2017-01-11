/*
 * Authors:
 *   钟峰(Popeye Zhong) <9555843@qq.com>
 *
 * Copyright (C) 2014-2016 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Linq;

namespace Zongsoft.Externals.Redis.Commands
{
	[Zongsoft.Services.CommandOption("count", Type = typeof(int), DefaultValue = 1)]
	public class QueueDequeueCommand : RedisCommandBase
	{
		#region 构造函数
		public QueueDequeueCommand() : base("Out")
		{
		}

		public QueueDequeueCommand(IRedisService redis) : base(redis, "Out")
		{
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(Services.CommandContext context)
		{
			if(context.Expression.Arguments.Length < 1)
				throw new Services.CommandException("Missing arguments.");

			var count = context.Expression.Options.GetValue<int>("count");

			if(context.Expression.Arguments.Length == 1)
				return this.Redis.GetEntry<IRedisQueue>(context.Expression.Arguments[0]).Dequeue(count);

			var result = new string[context.Expression.Arguments.Length * count];

			for(int i = 0; i < context.Expression.Arguments.Length; i++)
			{
				var queue = this.Redis.GetEntry<IRedisQueue>(context.Expression.Arguments[i]);
				var items = queue.Dequeue(count);

				int j = 0;

				foreach(var item in items)
				{
					result[i * context.Expression.Arguments.Length + j++] = (string)item;
				}
			}

			return result;
		}
		#endregion
	}
}
