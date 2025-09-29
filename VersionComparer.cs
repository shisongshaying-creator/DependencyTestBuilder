using System;
using System.Collections.Generic;

namespace Microsoft.Build.Locator;

internal static class VersionComparer
{
	public static bool Equals(SemanticVersion x, SemanticVersion y)
	{
		return Compare(x, y) == 0;
	}

	public static int Compare(SemanticVersion x, SemanticVersion y)
	{
		if (x == y)
		{
			return 0;
		}

		if (y == null)
		{
			return 1;
		}

		if (x == null)
		{
			return -1;
		}

		if (x != null && y != null)
		{
			int num = x.Major.CompareTo(y.Major);
			if (num != 0)
			{
				return num;
			}

			num = x.Minor.CompareTo(y.Minor);
			if (num != 0)
			{
				return num;
			}

			num = x.Patch.CompareTo(y.Patch);
			if (num != 0)
			{
				return num;
			}

			if (x.IsPrerelease && !y.IsPrerelease)
			{
				return -1;
			}

			if (!x.IsPrerelease && y.IsPrerelease)
			{
				return 1;
			}

			if (x.IsPrerelease && y.IsPrerelease)
			{
				num = CompareReleaseLabels(x.ReleaseLabels, y.ReleaseLabels);
				if (num != 0)
				{
					return num;
				}
			}
		}

		return 0;
	}

	private static int CompareReleaseLabels(IEnumerable<string> version1, IEnumerable<string> version2)
	{
		int num = 0;
		IEnumerator<string> enumerator = version1.GetEnumerator();
		IEnumerator<string> enumerator2 = version2.GetEnumerator();
		bool flag = enumerator.MoveNext();
		bool flag2 = enumerator2.MoveNext();
		while (flag || flag2)
		{
			if (!flag && flag2)
			{
				return -1;
			}

			if (flag && !flag2)
			{
				return 1;
			}

			num = CompareRelease(enumerator.Current, enumerator2.Current);
			if (num != 0)
			{
				return num;
			}

			flag = enumerator.MoveNext();
			flag2 = enumerator2.MoveNext();
		}

		return num;
	}

	private static int CompareRelease(string version1, string version2)
	{
		int result;
		bool flag = int.TryParse(version1, out result);
		int result2;
		bool flag2 = int.TryParse(version2, out result2);
		if (flag && flag2)
		{
			return result.CompareTo(result2);
		}

		if (flag || flag2)
		{
			return (!flag) ? 1 : (-1);
		}

		return StringComparer.OrdinalIgnoreCase.Compare(version1, version2);
	}
}