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
	[Zongsoft.Services.CommandOption("pairs", Description = "${Text.SetCommand.Pairs}")]
	[Zongsoft.Services.CommandOption("exists", Type = typeof(bool), Description = "${Text.SetCommand.Exists}")]
	[Zongsoft.Services.CommandOption("expires", Type = typeof(DateTime), Description = "${Text.SetCommand.Expires}")]
	[Zongsoft.Services.CommandOption("duration", Type = typeof(TimeSpan), Description = "${Text.SetCommand.Duration}")]
	public class SetCommand : RedisCommandBase
	{
		#region 构造函数
		public SetCommand() : base("Set")
		{
		}

		public SetCommand(ServiceStack.Redis.IRedisClient redis) : base(redis, "Set")
		{
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(Services.CommandContext parameter)
		{
			if(parameter.Arguments.Length < 2)
				throw new Zongsoft.Services.CommandException("Invalid arguments of command.");

			if(parameter.Options.Contains("duration") && parameter.Options.Contains("expires"))
				throw new Zongsoft.Services.CommandOptionException("duration, expires");

			TimeSpan duration;

			if(parameter.Options.TryGetValue<TimeSpan>("duration", out duration) && duration > TimeSpan.Zero)
			{
				if(parameter.Options.Contains("pairs"))
					return this.SetPairs(parameter.Arguments, (name, value) => this.Redis.Set<string>(name, value, duration));
				else
					return this.SetValues(parameter.Arguments, (name, values) => this.Redis.Set<string[]>(name, values, duration));
			}

			DateTime expires;

			if(parameter.Options.TryGetValue<DateTime>("expires", out expires) && expires > DateTime.Now)
			{
				if(parameter.Options.Contains("pairs"))
					return this.SetPairs(parameter.Arguments, (name, value) => this.Redis.Set<string>(name, value, expires));
				else
					return this.SetValues(parameter.Arguments, (name, values) => this.Redis.Set<string[]>(name, values, expires));
			}

			if(parameter.Options.Contains("pairs"))
				return this.SetPairs(parameter.Arguments, (name, value) => this.Redis.Set<string>(name, value));
			else
				return this.SetValues(parameter.Arguments, (name, values) => this.Redis.Set<string[]>(name, values));
		}
		#endregion

		#region 私有方法
		private bool SetValues(string[] args, Func<string, string[], bool> invoke)
		{
			var values = new string[args.Length - 1];
			Array.Copy(args, 1, values, 0, values.Length);
			return invoke(args[0], values);
		}

		private bool SetPairs(string[] args, Func<string, string, bool> invoke)
		{
			if(args.Length == 2)
				return invoke(args[0], args[1]);

			bool result = true;

			using(var pipeline = this.Redis.CreatePipeline())
			{
				for(int i = 0; i < args.Length - 1; i += 2)
				{
					result &= invoke(args[i], args[i + 1]);
				}

				pipeline.Replay();
			}

			return result;
		}
		#endregion
	}
}
