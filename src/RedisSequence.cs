using System;
using System.Collections.Generic;

namespace Zongsoft.Externals.Redis
{
	public class RedisSequence : Zongsoft.Common.ISequence
	{
		#region 成员字段
		private IRedisService _redis;
		#endregion

		#region 构造函数
		public RedisSequence()
		{
		}

		public RedisSequence(IRedisService redis)
		{
			if(redis == null)
				throw new ArgumentNullException("redis");

			_redis = redis;
		}
		#endregion

		#region 公共属性
		public IRedisService Redis
		{
			get
			{
				return _redis;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_redis = value;
			}
		}
		#endregion

		#region 公共方法
		public long GetSequenceNumber(string name)
		{
			var dictionary = this.GetRedisDictionary(name);
			var interval = 1;

			if(int.TryParse(dictionary["Interval"], out interval))
				return dictionary.Increment("Value", interval);
			else
				return dictionary.Increment("Value");
		}

		public long GetSequenceNumber(string name, int interval)
		{
			var dictionary = this.GetRedisDictionary(name);
			return dictionary.Increment("Value", interval);
		}

		public string GetSequenceString(string name)
		{
			var dictionary = this.GetRedisDictionary(name);
			var entries = dictionary.GetAllEntries();
			var interval = 1;

			int.TryParse(entries["Interval"], out interval);
			var formatString = entries["FormatString"];

			throw new NotImplementedException();
		}

		public string GetSequenceString(string name, int interval)
		{
			throw new NotImplementedException();
		}

		public Zongsoft.Common.SequenceInfo GetSequenceInfo(string name)
		{
			var dictionary = this.GetRedisDictionary(name);

			if(dictionary == null)
				return null;

			var info = new Zongsoft.Common.SequenceInfo(name);

			foreach(var entry in dictionary)
			{
				switch(entry.Key.ToLowerInvariant())
				{
					case "value":
						info.Value = long.Parse(entry.Value);
						break;
					case "interval":
						info.Interval = int.Parse(entry.Value);
						break;
					case "formatstring":
						info.FormatString = entry.Value;
						break;
				}
			}

			return info;
		}

		public void Reset(string name, int value = 0, int interval = 1, string formatString = null)
		{
			if(interval == 0)
				throw new ArgumentOutOfRangeException("interval");

			var dictionary = this.GetRedisDictionary(name);

			dictionary.SetRange(new KeyValuePair<string, string>[] {
				new KeyValuePair<string, string>("Value", value.ToString()),
				new KeyValuePair<string, string>("Interval", interval.ToString()),
				new KeyValuePair<string, string>("FormatString", formatString ?? string.Empty),
			});
		}
		#endregion

		#region 私有方法
		private IRedisDictionary GetRedisDictionary(string name)
		{
			return _redis.GetDictionary(this.GetType().FullName + ":" + name);
		}
		#endregion
	}
}
