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

namespace Zongsoft.Externals.Redis.Commands
{
	[Zongsoft.Services.CommandOption("not", Description = "${Text.SetCommand.NotExists}")]
	[Zongsoft.Services.CommandOption("expires", Type = typeof(DateTime), Description = "${Text.SetCommand.Expires}")]
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
		protected override object OnExecute(Services.CommandContext parameter)
		{
			if(parameter.Arguments.Length < 2)
				throw new Zongsoft.Services.CommandException("Missing arguments.");

			if(parameter.Options.Contains("duration") && parameter.Options.Contains("expires"))
				throw new Zongsoft.Services.CommandOptionException("duration, expires");

			var notExists = parameter.Options.Contains("not") || parameter.Options.Contains("notExists");

			TimeSpan duration;
			if(parameter.Options.TryGetValue<TimeSpan>("duration", out duration) && duration > TimeSpan.Zero)
				return this.Redis.SetValue(parameter.Arguments[0], parameter.Arguments[1], duration, notExists);

			DateTime expires;
			if(parameter.Options.TryGetValue<DateTime>("expires", out expires) && expires > DateTime.Now)
				return this.Redis.SetValue(parameter.Arguments[0], parameter.Arguments[1], expires, notExists);

			return this.Redis.SetValue(parameter.Arguments[0], parameter.Arguments[1], TimeSpan.Zero, notExists);
		}
		#endregion
	}
}
