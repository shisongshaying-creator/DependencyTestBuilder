using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Build.Locator;

internal class SemanticVersion : IComparable<SemanticVersion>
{
	private readonly IEnumerable<string> _releaseLabels;

	private Version _version;

	private string _originalValue;

	public int Major => _version.Major;

	public int Minor => _version.Minor;

	public int Patch => _version.Build;

	public IEnumerable<string> ReleaseLabels => _releaseLabels ?? Enumerable.Empty<string>();

	public string OriginalValue => _originalValue;

	public string Release
	{
		get
		{
			if (_releaseLabels == null)
			{
				return string.Empty;
			}

			return string.Join(".", _releaseLabels);
		}
	}

	public bool IsPrerelease
	{
		get
		{
			if (ReleaseLabels != null)
			{
				IEnumerator<string> enumerator = ReleaseLabels.GetEnumerator();
				if (enumerator.MoveNext())
				{
					return !string.IsNullOrEmpty(enumerator.Current);
				}

				return false;
			}

			return false;
		}
	}

	public SemanticVersion(Version version, IEnumerable<string> releaseLabels, string originalValue)
	{
		_version = version ?? throw new ArgumentNullException("version");
		if (releaseLabels != null)
		{
			_releaseLabels = releaseLabels.ToArray();
		}

		_originalValue = originalValue;
	}

	public int CompareTo(SemanticVersion other)
	{
		return VersionComparer.Compare(this, other);
	}
}