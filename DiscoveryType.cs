using System;

namespace Microsoft.Build.Locator;

[Flags]
public enum DiscoveryType
{
	DeveloperConsole = 1,
	VisualStudioSetup = 2,
	DotNetSdk = 4
}