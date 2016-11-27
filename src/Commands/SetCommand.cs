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
	[Zongsoft.Services.CommandOption("not", Description = "${Text.SetCommand.NotExists}")]
	[Zongsoft.Services.CommandOption("duration", Type = typeof(TimeSpan), Description = "${Text.SetCommand.Duration}")]
	public class SetCommand : RedisCommandBase
	{
		#region 构造函数
		public SetCommand() : base("Set")
		{
		}

		public SetCommand(IRedisService redis) : base(redis, "Set")
		{
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(Services.CommandContext context)
		{
			if(context.Expression.Arguments.Length < 2)
				throw new Zongsoft.Services.CommandException("Missing arguments.");

			if(context.Expression.Options.Contains("duration") && context.Expression.Options.Contains("expires"))
				throw new Zongsoft.Services.CommandOptionException("duration, expires");

			var notExists = context.Expression.Options.Contains("not") || context.Expression.Options.Contains("notExists");

			TimeSpan duration;
			if(context.Expression.Options.TryGetValue<TimeSpan>("duration", out duration) && duration > TimeSpan.Zero)
				return this.Redis.SetValue(context.Expression.Arguments[0], context.Expression.Arguments[1], duration, notExists);

			return this.Redis.SetValue(context.Expression.Arguments[0], context.Expression.Arguments[1], TimeSpan.Zero, notExists);
		}
		#endregion
	}
}
