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
using System.Linq;

namespace Zongsoft.Externals.Redis.Commands
{
	[Zongsoft.Services.CommandOption("length", Type = typeof(int), DefaultValue = 1)]
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
		protected override void OnExecute(Services.CommandContext context)
		{
			if(context.Arguments.Length < 1)
				throw new Services.CommandException("Missing arguments.");

			var length = (int)context.Options["length"];

			if(context.Arguments.Length == 1)
			{
				context.Result = this.Redis.GetQueue(context.Arguments[0]).Dequeue(length);
				return;
			}

			var result = new string[context.Arguments.Length * length];

			for(int i = 0; i < context.Arguments.Length; i++)
			{
				var queue = this.Redis.GetQueue(context.Arguments[i]);
				var items = queue.Dequeue(length);

				int j = 0;

				foreach(var item in items)
				{
					result[i * context.Arguments.Length + j++] = (string)item;
				}
			}

			context.Result = result;
		}
		#endregion
	}
}
