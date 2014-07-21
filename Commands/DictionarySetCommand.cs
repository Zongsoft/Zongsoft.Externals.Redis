using System;
using System.Collections.Generic;

namespace Zongsoft.Externals.Redis.Commands
{
	public class DictionarySetCommand : RedisCommandBase
	{
		#region 构造函数
		public DictionarySetCommand() : base("Set")
		{
		}

		public DictionarySetCommand(ServiceStack.Redis.IRedisClient redis) : base(redis, "Set")
		{
		}
		#endregion

		#region 执行方法
		protected override object OnExecute(Services.CommandContext parameter)
		{
			if(parameter.Arguments.Length < 3)
				return null;

			if(parameter.Arguments.Length == 3)
				return this.Redis.SetEntryInHash(parameter.Arguments[0], parameter.Arguments[1], parameter.Arguments[2]);

			var dictionary = new Dictionary<string, string>((parameter.Arguments.Length - 1) / 2);
			for(int i = 0; i < (parameter.Arguments.Length - 1) / 2; i++)
			{
				dictionary.Add(parameter.Arguments[i * 2], parameter.Arguments[i * 2 + 1]);
			}

			this.Redis.SetRangeInHash(parameter.Arguments[0], dictionary);
			return true;
		}
		#endregion
	}
}
