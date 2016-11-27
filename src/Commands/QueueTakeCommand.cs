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
	[Zongsoft.Services.CommandOption("index", Type = typeof(int), Description = "${Text.ListCommand.Index}")]
	[Zongsoft.Services.CommandOption("count", Type = typeof(int), DefaultValue = 1, Description = "${Text.ListCommand.Count}")]
	public class QueueTakeCommand : RedisCommandBase
	{
		#region 构造函数
		public QueueTakeCommand() : base("Take")
		{
		}

		public QueueTakeCommand(IRedisService redis) : base(redis, "Take")
		{
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(Services.CommandContext context)
		{
			if(context.Expression.Arguments.Length < 1)
				throw new Zongsoft.Services.CommandException("Missing arguments.");

			var index = context.Expression.Options.GetValue<int>("index");
			var queue = this.Redis.GetQueue(context.Expression.Arguments[0]);

			int count;

			if(context.Expression.Options.TryGetValue<int>("count", out count))
				return queue.Take(index, count);

			return queue.Take(index);
		}
		#endregion
	}
}
