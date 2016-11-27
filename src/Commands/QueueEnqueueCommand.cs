/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
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

namespace Zongsoft.Externals.Redis.Commands
{
	public class QueueEnqueueCommand : RedisCommandBase
	{
		#region 构造函数
		public QueueEnqueueCommand() : base("In")
		{
		}

		public QueueEnqueueCommand(IRedisService redis) : base(redis, "In")
		{
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(Services.CommandContext context)
		{
			if(context.Expression.Arguments.Length < 2)
				throw new Services.CommandException("Missing arguments.");

			var queue = this.Redis.GetQueue(context.Expression.Arguments[0]);

			if(context.Expression.Arguments.Length == 2)
			{
				queue.Enqueue(context.Expression.Arguments[1]);
			}
			else
			{
				var items = new string[context.Expression.Arguments.Length - 1];
				Array.Copy(context.Expression.Arguments, 1, items, 0, items.Length);

				queue.Enqueue(items);
			}

			return null;
		}
		#endregion
	}
}
