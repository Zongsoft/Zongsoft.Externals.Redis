using System;
using System.Collections.Generic;

namespace Zongsoft.Externals.Redis
{
	public class RedisException : Exception
	{
		#region 构造函数
		public RedisException()
		{
		}

		public RedisException(string message) : base(message)
		{
		}

		public RedisException(string message, Exception innerException) : base(message, innerException)
		{
		}
		#endregion
	}
}
