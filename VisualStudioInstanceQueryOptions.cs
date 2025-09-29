using System;

namespace Microsoft.Build.Locator;

public class VisualStudioInstanceQueryOptions
{
	public static VisualStudioInstanceQueryOptions Default => new VisualStudioInstanceQueryOptions
	{
		DiscoveryTypes = DiscoveryType.DotNetSdk
	};

	public DiscoveryType DiscoveryTypes { get; set; }

	public bool AllowAllRuntimeVersions { get; set; }

	public bool AllowAllDotnetLocations { get; set; }

	public string WorkingDirectory { get; set; } = Environment.CurrentDirectory;

}