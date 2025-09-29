using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text.RegularExpressions;

namespace Microsoft.Build.Locator;

internal static class DotNetSdkLocationHelper
{
	[CompilerGenerated]
	private sealed class _003C_003CGetInstances_003Eg__GetAllAvailableSDKs_007C5_1_003Ed : IEnumerable<string>, IEnumerable, IEnumerator<string>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private string _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private _003C_003Ec__DisplayClass5_0 _003C_003E8__1;

		private bool allowAllDotnetLocations;

		public bool _003C_003E3__allowAllDotnetLocations;

		private bool _003CfoundSdks_003E5__2;

		private IEnumerator<string> _003C_003E7__wrap2;

		private string[] _003C_003E7__wrap3;

		private int _003C_003E7__wrap4;

		string IEnumerator<string>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003C_003CGetInstances_003Eg__GetAllAvailableSDKs_007C5_1_003Ed(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}

			_003C_003E8__1 = null;
			_003C_003E7__wrap2 = null;
			_003C_003E7__wrap3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				if (num != 0)
				{
					if (num != 1)
					{
						return false;
					}

					_003C_003E1__state = -3;
					_003C_003E7__wrap4++;
					goto IL_010c;
				}

				_003C_003E1__state = -1;
				_003C_003E8__1 = new _003C_003Ec__DisplayClass5_0();
				_003CfoundSdks_003E5__2 = false;
				_003C_003E8__1.resolvedPaths = null;
				_003C_003E7__wrap2 = s_dotnetPathCandidates.Value.GetEnumerator();
				_003C_003E1__state = -3;
				goto IL_0139;
			IL_00b2:
				_003CfoundSdks_003E5__2 = true;
				_003C_003E7__wrap3 = _003C_003E8__1.resolvedPaths;
				_003C_003E7__wrap4 = 0;
				goto IL_010c;
			IL_0149:
				_003C_003Em__Finally1();
				_003C_003E7__wrap2 = null;
				if (!_003CfoundSdks_003E5__2)
				{
					throw new InvalidOperationException(SdkResolutionExceptionMessage("hostfxr_get_available_sdks"));
				}

				return false;
			IL_0139:
				while (_003C_003E7__wrap2.MoveNext())
				{
					if (NativeMethods.hostfxr_get_available_sdks(_003C_003E7__wrap2.Current, delegate (NativeMethods.hostfxr_resolve_sdk2_result_key_t key, string[] value)
					{
						_003C_003E8__1.resolvedPaths = value;
					}) != 0 || _003C_003E8__1.resolvedPaths == null)
					{
						continue;
					}

					goto IL_00b2;
				}

				goto IL_0149;
			IL_010c:
				if (_003C_003E7__wrap4 < _003C_003E7__wrap3.Length)
				{
					string text = _003C_003E7__wrap3[_003C_003E7__wrap4];
					_003C_003E2__current = text;
					_003C_003E1__state = 1;
					return true;
				}

				_003C_003E7__wrap3 = null;
				if (_003C_003E8__1.resolvedPaths.Length == 0 || allowAllDotnetLocations)
				{
					goto IL_0139;
				}

				goto IL_0149;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap2 != null)
			{
				_003C_003E7__wrap2.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{
			_003C_003CGetInstances_003Eg__GetAllAvailableSDKs_007C5_1_003Ed _003C_003CGetInstances_003Eg__GetAllAvailableSDKs_007C5_1_003Ed;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003C_003CGetInstances_003Eg__GetAllAvailableSDKs_007C5_1_003Ed = this;
			}
			else
			{
				_003C_003CGetInstances_003Eg__GetAllAvailableSDKs_007C5_1_003Ed = new _003C_003CGetInstances_003Eg__GetAllAvailableSDKs_007C5_1_003Ed(0);
			}

			_003C_003CGetInstances_003Eg__GetAllAvailableSDKs_007C5_1_003Ed.allowAllDotnetLocations = _003C_003E3__allowAllDotnetLocations;
			return _003C_003CGetInstances_003Eg__GetAllAvailableSDKs_007C5_1_003Ed;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<string>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass5_0
	{
		public string[] resolvedPaths;

		public NativeMethods.hostfxr_get_available_sdks_result_fn _003C_003E9__3;

		internal void _003CGetInstances_003Eb__3(NativeMethods.hostfxr_resolve_sdk2_result_key_t key, string[] value)
		{
			resolvedPaths = value;
		}
	}

	[CompilerGenerated]
	private sealed class _003CGetInstances_003Ed__5 : IEnumerable<VisualStudioInstance>, IEnumerable, IEnumerator<VisualStudioInstance>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private VisualStudioInstance _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private string workingDirectory;

		public string _003C_003E3__workingDirectory;

		private bool allowAllDotnetLocations;

		public bool _003C_003E3__allowAllDotnetLocations;

		private bool allowQueryAllRuntimes;

		public bool _003C_003E3__allowQueryAllRuntimes;

		private string _003CbestSdkPath_003E5__2;

		private Dictionary<Version, VisualStudioInstance?> _003CversionInstanceMap_003E5__3;

		private string[] _003C_003E7__wrap3;

		private int _003C_003E7__wrap4;

		private VisualStudioInstance _003CdotnetSdk_003E5__6;

		private IEnumerator<VisualStudioInstance> _003C_003E7__wrap6;

		VisualStudioInstance IEnumerator<VisualStudioInstance>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGetInstances_003Ed__5(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 2)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}

			_003CbestSdkPath_003E5__2 = null;
			_003CversionInstanceMap_003E5__3 = null;
			_003C_003E7__wrap3 = null;
			_003CdotnetSdk_003E5__6 = null;
			_003C_003E7__wrap6 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				IOrderedEnumerable<VisualStudioInstance> orderedEnumerable;
				switch (_003C_003E1__state)
				{
					default:
						return false;
					case 0:
						{
							_003C_003E1__state = -1;
							string[] array;
							try
							{
								AddUnmanagedDllResolver();
								_003CbestSdkPath_003E5__2 = _003CGetInstances_003Eg__GetSdkFromGlobalSettings_007C5_2(workingDirectory);
								array = _003CGetInstances_003Eg__GetAllAvailableSDKs_007C5_1(allowAllDotnetLocations).ToArray();
							}
							finally
							{
								RemoveUnmanagedDllResolver();
							}

							_003CversionInstanceMap_003E5__3 = new Dictionary<Version, VisualStudioInstance>();
							_003C_003E7__wrap3 = array;
							_003C_003E7__wrap4 = 0;
							goto IL_0120;
						}
					case 1:
						_003C_003E1__state = -1;
						goto IL_00ee;
					case 2:
						{
							_003C_003E1__state = -3;
							break;
						}

					IL_0120:
						if (_003C_003E7__wrap4 < _003C_003E7__wrap3.Length)
						{
							string dotNetSdkPath = _003C_003E7__wrap3[_003C_003E7__wrap4];
							_003CdotnetSdk_003E5__6 = GetInstance(dotNetSdkPath, allowQueryAllRuntimes);
							if (_003CdotnetSdk_003E5__6 != null)
							{
								if (_003CdotnetSdk_003E5__6.VisualStudioRootPath == _003CbestSdkPath_003E5__2)
								{
									_003CversionInstanceMap_003E5__3[_003CdotnetSdk_003E5__6.Version] = null;
									_003C_003E2__current = _003CdotnetSdk_003E5__6;
									_003C_003E1__state = 1;
									return true;
								}

								goto IL_00ee;
							}

							goto IL_010b;
						}

						_003C_003E7__wrap3 = null;
						orderedEnumerable = from i in _003CversionInstanceMap_003E5__3.Values.OfType<VisualStudioInstance>()
											orderby i.Version descending
											select i;
						_003C_003E7__wrap6 = orderedEnumerable.GetEnumerator();
						_003C_003E1__state = -3;
						break;
					IL_010b:
						_003CdotnetSdk_003E5__6 = null;
						_003C_003E7__wrap4++;
						goto IL_0120;
					IL_00ee:
						_003CversionInstanceMap_003E5__3.TryAdd(_003CdotnetSdk_003E5__6.Version, _003CdotnetSdk_003E5__6);
						goto IL_010b;
				}

				if (_003C_003E7__wrap6.MoveNext())
				{
					VisualStudioInstance current = _003C_003E7__wrap6.Current;
					_003C_003E2__current = current;
					_003C_003E1__state = 2;
					return true;
				}

				_003C_003Em__Finally1();
				_003C_003E7__wrap6 = null;
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap6 != null)
			{
				_003C_003E7__wrap6.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<VisualStudioInstance> IEnumerable<VisualStudioInstance>.GetEnumerator()
		{
			_003CGetInstances_003Ed__5 _003CGetInstances_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CGetInstances_003Ed__ = this;
			}
			else
			{
				_003CGetInstances_003Ed__ = new _003CGetInstances_003Ed__5(0);
			}

			_003CGetInstances_003Ed__.workingDirectory = _003C_003E3__workingDirectory;
			_003CGetInstances_003Ed__.allowQueryAllRuntimes = _003C_003E3__allowQueryAllRuntimes;
			_003CGetInstances_003Ed__.allowAllDotnetLocations = _003C_003E3__allowAllDotnetLocations;
			return _003CGetInstances_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<VisualStudioInstance>)this).GetEnumerator();
		}
	}

	private static readonly Regex VersionRegex = new Regex("^(\\d+)\\.(\\d+)\\.(\\d+)", RegexOptions.Multiline);

	private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

	private static readonly string ExeName = (IsWindows ? "dotnet.exe" : "dotnet");

	private static readonly Lazy<IList<string>> s_dotnetPathCandidates = new Lazy<IList<string>>(() => ResolveDotnetPathCandidates());

	public static VisualStudioInstance? GetInstance(string dotNetSdkPath, bool allowQueryAllRuntimeVersions)
	{
		if (string.IsNullOrWhiteSpace(dotNetSdkPath) || !File.Exists(Path.Combine(dotNetSdkPath, "Microsoft.Build.dll")))
		{
			return null;
		}

		string path = Path.Combine(dotNetSdkPath, ".version");
		if (!File.Exists(path))
		{
			return null;
		}

		Match match = VersionRegex.Match(File.ReadAllText(path));
		if (!match.Success)
		{
			return null;
		}

		if (!int.TryParse(match.Groups[1].Value, out var result) || !int.TryParse(match.Groups[2].Value, out var result2) || !int.TryParse(match.Groups[3].Value, out var result3))
		{
			return null;
		}

		if (!allowQueryAllRuntimeVersions && (result > Environment.Version.Major || (result == Environment.Version.Major && result2 > Environment.Version.Minor)))
		{
			return null;
		}

		return new VisualStudioInstance(".NET Core SDK", dotNetSdkPath, new Version(result, result2, result3), DiscoveryType.DotNetSdk);
	}

	[IteratorStateMachine(typeof(_003CGetInstances_003Ed__5))]
	public static IEnumerable<VisualStudioInstance> GetInstances(string workingDirectory, bool allowQueryAllRuntimes, bool allowAllDotnetLocations)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetInstances_003Ed__5(-2)
		{
			_003C_003E3__workingDirectory = workingDirectory,
			_003C_003E3__allowQueryAllRuntimes = allowQueryAllRuntimes,
			_003C_003E3__allowAllDotnetLocations = allowAllDotnetLocations
		};
	}

	private static void AddUnmanagedDllResolver()
	{
		ModifyUnmanagedDllResolver(delegate (AssemblyLoadContext loadContext)
		{
			loadContext.ResolvingUnmanagedDll += HostFxrResolver;
		});
	}

	private static void RemoveUnmanagedDllResolver()
	{
		ModifyUnmanagedDllResolver(delegate (AssemblyLoadContext loadContext)
		{
			loadContext.ResolvingUnmanagedDll -= HostFxrResolver;
		});
	}

	private static void ModifyUnmanagedDllResolver(Action<AssemblyLoadContext> resolverAction)
	{
		if (!IsWindows)
		{
			AssemblyLoadContext loadContext = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
			if (loadContext != null)
			{
				resolverAction(loadContext);
			}
		}
	}

	private static nint HostFxrResolver(Assembly assembly, string libraryName)
	{
		if (!libraryName.Equals("hostfxr", StringComparison.Ordinal))
		{
			return IntPtr.Zero;
		}

		string text = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "hostfxr.dll" : (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "libhostfxr.dylib" : "libhostfxr.so"));
		string text2 = string.Empty;
		foreach (string item in s_dotnetPathCandidates.Value)
		{
			text2 = Path.Combine(item, "host", "fxr");
			if (!Directory.Exists(text2))
			{
				continue;
			}

			foreach (SemanticVersion item2 in (from v in new FileSystemEnumerable<SemanticVersion>(text2, delegate (ref FileSystemEntry entry)
			{
				SemanticVersion version;
				return (!SemanticVersionParser.TryParse(entry.FileName.ToString(), out version)) ? null : version;
			})
			{
				ShouldIncludePredicate = delegate (ref FileSystemEntry entry)
				{
					return entry.IsDirectory;
				}
			}
											   where v != null
											   select (v) into f
											   orderby f descending
											   select f).ToList())
			{
				if (NativeLibrary.TryLoad(Path.Combine(text2, item2.OriginalValue, text), out var handle))
				{
					return handle;
				}
			}
		}

		throw new InvalidOperationException($".NET SDK cannot be resolved, because {text} cannot be found inside {text2}." + Environment.NewLine + "This might indicate a corrupted SDK installation on the machine.");
	}

	private static string SdkResolutionExceptionMessage(string methodName)
	{
		return "Failed to find all versions of .NET Core MSBuild. Call to " + methodName + ". There may be more details in stderr.";
	}

	private static IList<string> ResolveDotnetPathCandidates()
	{
		List<string> pathCandidates = new List<string>();
		AddIfValid(GetDotnetPathFromROOT());
		string currentProcessPath = GetCurrentProcessPath();
		if (!string.IsNullOrEmpty(currentProcessPath) && Path.GetFileName(currentProcessPath).Equals(ExeName, StringComparison.InvariantCultureIgnoreCase))
		{
			AddIfValid(Path.GetDirectoryName(currentProcessPath));
		}

		string text = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH");
		if (!string.IsNullOrEmpty(text) && File.Exists(text))
		{
			if (!IsWindows)
			{
				text = realpath(text) ?? text;
			}

			AddIfValid(Path.GetDirectoryName(text));
		}

		AddIfValid(FindDotnetPathFromEnvVariable("DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR"));
		AddIfValid(GetDotnetPathFromPATH());
		if (pathCandidates.Count != 0)
		{
			return pathCandidates;
		}

		throw new InvalidOperationException("Path to dotnet executable is not set. The probed variables are: DOTNET_ROOT, DOTNET_HOST_PATH, DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR and PATH. Make sure, that at least one of the listed variables points to the existing dotnet executable.");
		void AddIfValid(string? path)
		{
			if (!string.IsNullOrEmpty(path))
			{
				pathCandidates.Add(path);
			}
		}
	}

	private static string? GetDotnetPathFromROOT()
	{
		return FindDotnetPathFromEnvVariable((IntPtr.Size == 4) ? "DOTNET_ROOT(x86)" : "DOTNET_ROOT");
	}

	private static string? GetCurrentProcessPath()
	{
		return Environment.ProcessPath;
	}

	private static string? GetDotnetPathFromPATH()
	{
		string result = null;
		string[] array = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? Array.Empty<string>();
		for (int i = 0; i < array.Length; i++)
		{
			string text = ValidatePath(array[i]);
			if (!string.IsNullOrEmpty(text))
			{
				result = text;
				break;
			}
		}

		return result;
	}

	private static string? realpath(string path)
	{
		nint ptr = NativeMethods.realpath(path, IntPtr.Zero);
		string result = Marshal.PtrToStringAuto(ptr);
		NativeMethods.free(ptr);
		return result;
	}

	private static string? FindDotnetPathFromEnvVariable(string environmentVariable)
	{
		string environmentVariable2 = Environment.GetEnvironmentVariable(environmentVariable);
		if (!string.IsNullOrEmpty(environmentVariable2))
		{
			return ValidatePath(environmentVariable2);
		}

		return null;
	}

	private static void SetEnvironmentVariableIfEmpty(string name, string value)
	{
		if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(name)))
		{
			Environment.SetEnvironmentVariable(name, value);
		}
	}

	private static string? ValidatePath(string dotnetPath)
	{
		string text = Path.Combine(dotnetPath, ExeName);
		if (File.Exists(text))
		{
			if (!IsWindows)
			{
				text = realpath(text) ?? text;
				if (!File.Exists(text))
				{
					return null;
				}

				return Path.GetDirectoryName(text);
			}

			return dotnetPath;
		}

		return null;
	}

	[IteratorStateMachine(typeof(_003C_003CGetInstances_003Eg__GetAllAvailableSDKs_007C5_1_003Ed))]
	[CompilerGenerated]
	internal static IEnumerable<string> _003CGetInstances_003Eg__GetAllAvailableSDKs_007C5_1(bool allowAllDotnetLocations)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003C_003CGetInstances_003Eg__GetAllAvailableSDKs_007C5_1_003Ed(-2)
		{
			_003C_003E3__allowAllDotnetLocations = allowAllDotnetLocations
		};
	}

	[CompilerGenerated]
	internal static string? _003CGetInstances_003Eg__GetSdkFromGlobalSettings_007C5_2(string workingDirectory)
	{
		string resolvedSdk = null;
		foreach (string item in s_dotnetPathCandidates.Value)
		{
			if (NativeMethods.hostfxr_resolve_sdk2(item, workingDirectory, (NativeMethods.hostfxr_resolve_sdk2_flags_t)0, delegate (NativeMethods.hostfxr_resolve_sdk2_result_key_t key, string value)
			{
				if (key == NativeMethods.hostfxr_resolve_sdk2_result_key_t.resolved_sdk_dir)
				{
					resolvedSdk = value;
				}
			}) == 0)
			{
				SetEnvironmentVariableIfEmpty("DOTNET_HOST_PATH", Path.Combine(item, ExeName));
				return resolvedSdk;
			}
		}

		if (!string.IsNullOrEmpty(resolvedSdk))
		{
			return resolvedSdk;
		}

		throw new InvalidOperationException(SdkResolutionExceptionMessage("hostfxr_resolve_sdk2"));
	}
}