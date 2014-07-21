using System;
using System.Collections.Generic;
using System.Linq;

namespace Zongsoft.Externals.Redis.Commands
{
	public class GetCommand : RedisCommandBase
	{
		#region 构造函数
		public GetCommand() : base("Get")
		{
		}

		public GetCommand(ServiceStack.Redis.IRedisClient redis) : base(redis, "Get")
		{
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(Services.CommandContext parameter)
		{
			if(parameter.Arguments.Length < 1)
				return null;

			if(parameter.Arguments.Length == 1)
				return this.Redis.Get<object>(parameter.Arguments[0]);

			return this.Redis.GetValues<object>(parameter.Arguments.ToList());
		}
		#endregion
	}
}
