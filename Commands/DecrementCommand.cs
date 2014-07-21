using System;
using System.Collections.Generic;

namespace Zongsoft.Externals.Redis.Commands
{
	public class DecrementCommand : RedisCommandBase
	{
		#region 构造函数
		public DecrementCommand() : base("Decrement")
		{
		}

		public DecrementCommand(ServiceStack.Redis.IRedisClient redis) : base(redis, "Decrement")
		{
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(Services.CommandContext parameter)
		{
			if(parameter.Arguments.Length < 1)
				return 0L;

			int interval = 1;

			if(parameter.Arguments.Length > 1 && int.TryParse(parameter.Arguments[1], out interval))
				return this.Redis.DecrementValueBy(parameter.Arguments[0], interval);
			else
				return this.Redis.DecrementValue(parameter.Arguments[0]);
		}
		#endregion
	}
}
