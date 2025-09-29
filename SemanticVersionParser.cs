using System;
using System.Linq;

namespace Microsoft.Build.Locator;

internal static class SemanticVersionParser
{
	public static bool TryParse(string value, out SemanticVersion version)
	{
		version = null;
		if (value != null)
		{
			var (text, array) = ParseSections(value);
			if (Version.TryParse(text, out Version result))
			{
				string[] array2 = text.Split('.');
				if (array2.Length != 3)
				{
					return false;
				}

				string[] array3 = array2;
				for (int i = 0; i < array3.Length; i++)
				{
					if (!IsValidPart(array3[i], allowLeadingZeros: false))
					{
						return false;
					}
				}

				if (array != null && !array.All((string s) => IsValidPart(s, allowLeadingZeros: false)))
				{
					return false;
				}

				version = new SemanticVersion(NormalizeVersionValue(result), array, value);
				return true;
			}
		}

		return false;
	}

	private static bool IsLetterOrDigitOrDash(char c)
	{
		if ((c < '0' || c > '9') && (c < 'A' || c > 'Z') && (c < 'a' || c > 'z'))
		{
			return c == '-';
		}

		return true;
	}

	private static bool IsValidPart(string s, bool allowLeadingZeros)
	{
		return IsValidPart(s.ToCharArray(), allowLeadingZeros);
	}

	private static bool IsValidPart(char[] chars, bool allowLeadingZeros)
	{
		bool flag = true;
		if (chars.Length == 0)
		{
			flag = false;
		}

		if (!allowLeadingZeros && chars.Length > 1 && chars[0] == '0' && chars.All((char c) => char.IsDigit(c)))
		{
			return false;
		}

		return flag & chars.All((char c) => IsLetterOrDigitOrDash(c));
	}

	private static (string Version, string[] ReleaseLabels) ParseSections(string value)
	{
		string item = null;
		string[] item2 = null;
		int num = -1;
		int num2 = -1;
		char[] array = value.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			bool flag = i == array.Length - 1;
			if (num < 0)
			{
				if (flag || array[i] == '-' || array[i] == '+')
				{
					item = value[..(i + (flag ? 1 : 0))];
					num = i;
					if (array[i] == '+')
					{
						num2 = i;
					}
				}
			}
			else if (num2 < 0 && (flag || array[i] == '+'))
			{
				int num3 = num + 1;
				int num4 = i + (flag ? 1 : 0);
				item2 = value.Substring(num3, num4 - num3).Split('.');
				num2 = i;
			}
		}

		return (Version: item, ReleaseLabels: item2);
	}

	private static Version NormalizeVersionValue(Version version)
	{
		Version result = version;
		if (version.Build < 0 || version.Revision < 0)
		{
			result = new Version(version.Major, version.Minor, Math.Max(version.Build, 0), Math.Max(version.Revision, 0));
		}

		return result;
	}
}