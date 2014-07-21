using System;
using System.Collections.Generic;

namespace Zongsoft.Externals.Redis.Commands
{
	public class HashsetSetCommand : RedisCommandBase
	{
		#region 构造函数
		public HashsetSetCommand() : base("Set")
		{
		}

		public HashsetSetCommand(ServiceStack.Redis.IRedisClient redis) : base(redis, "Set")
		{
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(Services.CommandContext parameter)
		{
			if(parameter.Arguments.Length < 2)
				return null;

			if(parameter.Arguments.Length == 2)
				this.Redis.AddItemToSet(parameter.Arguments[0], parameter.Arguments[1]);

			var values = new string[parameter.Arguments.Length - 1];
			Array.Copy(parameter.Arguments, 1, values, 0, values.Length);

			this.Redis.AddRangeToSet(parameter.Arguments[0], new List<string>(values));

			return null;
		}
		#endregion
	}
}
