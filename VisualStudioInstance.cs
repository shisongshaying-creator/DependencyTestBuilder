using System;
using System.IO;

namespace Microsoft.Build.Locator;

public class VisualStudioInstance
{
	public Version Version { get; }

	public string VisualStudioRootPath { get; }

	public string Name { get; }

	public string MSBuildPath { get; }

	public DiscoveryType DiscoveryType { get; }

	internal VisualStudioInstance(string name, string path, Version version, DiscoveryType discoveryType)
	{
		Name = name;
		VisualStudioRootPath = path;
		Version = version;
		DiscoveryType = discoveryType;
		switch (discoveryType)
		{
			case DiscoveryType.DeveloperConsole:
			case DiscoveryType.VisualStudioSetup:
				MSBuildPath = ((version.Major >= 16) ? Path.Combine(VisualStudioRootPath, "MSBuild", "Current", "Bin") : Path.Combine(VisualStudioRootPath, "MSBuild", "15.0", "Bin"));
				break;
			case DiscoveryType.DotNetSdk:
				MSBuildPath = VisualStudioRootPath;
				break;
			default:
				throw new ArgumentOutOfRangeException("discoveryType", discoveryType, null);
		}
	}
}