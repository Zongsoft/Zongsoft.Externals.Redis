using System;
using System.Collections.Generic;

namespace Zongsoft.Externals.Redis.Commands
{
	public class RemoveCommand : RedisCommandBase
	{
		#region 构造函数
		public RemoveCommand() : base("Set")
		{
		}

		public RemoveCommand(ServiceStack.Redis.IRedisClient redis) : base(redis, "Set")
		{
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(Services.CommandContext parameter)
		{
			if(parameter.Arguments.Length < 1)
				throw new Zongsoft.Services.CommandException("Invalid arguments of command.");

			if(parameter.Arguments.Length == 1)
				return this.Redis.Remove(parameter.Arguments[0]);
			else
				this.Redis.RemoveAll(parameter.Arguments);

			return true;
		}
		#endregion
	}
}
