using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LSF小脚本.library {
	class Convert {
		/// <summary>
		/// 汉字转换为Unicode编码
		/// </summary>
		/// <param name="str">要编码的汉字字符串</param>
		/// <returns>Unicode编码的的字符串</returns>
		//public static string ToUnicode(string str) {
		//	byte[] bts = Encoding.Unicode.GetBytes(str);
		//	string r = "";
		//	for (int i = 0; i < bts.Length; i += 2) r += "\\u" + bts[i + 1].ToString("x").PadLeft(2, '0') + bts[i].ToString("x").PadLeft(2, '0');
		//	return r;
		//}
		public static string ToUnicode(string str) {
			string outStr = "";
			if (!string.IsNullOrEmpty(str))
			{
				for (int i = 0; i < str.Length; i++)
				{
					//将中文字符转为10进制整数，然后转为16进制unicode字符  
					outStr += "\\u" + ((int)str[i]).ToString("x");
				}
			}
			return outStr;
		}
		/// <summary>
		/// 将Unicode编码转换为汉字字符串
		/// </summary>
		/// <param name="str">Unicode编码字符串</param>
		/// <returns>汉字字符串</returns>
		public static string ToGB2312(string str) {
			string r = "";
			MatchCollection mc = Regex.Matches(str, @"\\u([\w]{2})([\w]{2})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			byte[] bts = new byte[2];
			foreach (Match m in mc)
			{
				bts[0] = (byte)int.Parse(m.Groups[2].Value, NumberStyles.HexNumber);
				bts[1] = (byte)int.Parse(m.Groups[1].Value, NumberStyles.HexNumber);
				r += Encoding.Unicode.GetString(bts);
			}
			return r;
		}
	}
}
