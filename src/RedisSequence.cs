/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2014-2015 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Text.RegularExpressions;

namespace Zongsoft.Externals.Redis
{
	public class RedisSequence : Zongsoft.Common.ISequence
	{
		#region 静态字段
		private static readonly Regex _regex = new Regex(@"(?<expr>\{\s*(?<name>([#.])|([A-Za-z_][^{\}\:]*))(\:(?<format>[^\{\}]+))?\s*\})", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);
		#endregion

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

		public long GetSequenceNumber(string name, int interval, int seed = 0)
		{
			var dictionary = this.GetRedisDictionary(name, interval, seed);
			return dictionary.Increment("Value", interval);
		}

		public string GetSequenceString(string name)
		{
			var dictionary = this.GetRedisDictionary(name);
			var values = dictionary.GetValues("Interval", "FormatString");
			var sequenceNumber = 0L;
			var interval = 1;

			if(int.TryParse(values[0], out interval))
				sequenceNumber = dictionary.Increment("Value", interval);
			else
				sequenceNumber = dictionary.Increment("Value");

			return this.GetFormattedText(values[1], sequenceNumber);
		}

		public string GetSequenceString(string name, int interval, int seed = 0, string formatString = null)
		{
			var dictionary = this.GetRedisDictionary(name, interval, seed, formatString);
			var sequenceNumber = dictionary.Increment("Value", interval);

			return this.GetFormattedText(dictionary["FormatString"], sequenceNumber);
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

			dictionary.SetRange(new Dictionary<string, string> {
				{ "Value", value.ToString() },
				{ "Interval", interval == 0 ? "1" : interval.ToString() },
				{ "FormatString", formatString ?? string.Empty },
			});
		}
		#endregion

		#region 私有方法
		private IRedisDictionary GetRedisDictionary(string name, int interval = 1, int seed = 0, string formatString = null)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			var key = this.GetType().FullName + ":" + name.Trim();

			return _redis.GetDictionary(key, new Dictionary<string, string>
			{
				{ "Value", seed.ToString() },
				{ "Interval", interval == 0 ? "1" : interval.ToString() },
				{ "FormatString", formatString ?? string.Empty },
			});
		}

		private string GetFormattedText(string formatString, long sequenceNumber)
		{
			if(string.IsNullOrWhiteSpace(formatString))
				return sequenceNumber.ToString();

			return _regex.Replace(formatString, match =>
			{
				var number = 0L;
				var text = match.Groups["name"].Value;

				if(text == "#" || text == ".")
					number = sequenceNumber;
				else
					long.TryParse(this.GetRedisDictionary(text)["Value"], out number);

				text = match.Groups["format"].Value;

				if(string.IsNullOrWhiteSpace(text))
					return number.ToString();
				else
					return number.ToString(text);
			});
		}
		#endregion
	}
}
