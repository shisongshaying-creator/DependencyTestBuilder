using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;

namespace Microsoft.Build.Locator;

public static class MSBuildLocator
{
	[CompilerGenerated]
	private sealed class _003CGetInstances_003Ed__29 : IEnumerable<VisualStudioInstance>, IEnumerable, IEnumerator<VisualStudioInstance>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private VisualStudioInstance _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private VisualStudioInstanceQueryOptions options;

		public VisualStudioInstanceQueryOptions _003C_003E3__options;

		private IEnumerator<VisualStudioInstance> _003C_003E7__wrap1;

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
		public _003CGetInstances_003Ed__29(int _003C_003E1__state)
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

			_003C_003E7__wrap1 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
					default:
						return false;
					case 0:
						{
							_003C_003E1__state = -1;
							bool allowQueryAllRuntimes = AllowQueryAllRuntimeVersions || options.AllowAllRuntimeVersions;
							bool allowAllDotnetLocations = AllowQueryAllDotnetLocations || options.AllowAllDotnetLocations;
							_003C_003E7__wrap1 = DotNetSdkLocationHelper.GetInstances(options.WorkingDirectory, allowQueryAllRuntimes, allowAllDotnetLocations).GetEnumerator();
							_003C_003E1__state = -3;
							break;
						}
					case 1:
						_003C_003E1__state = -3;
						break;
				}

				if (_003C_003E7__wrap1.MoveNext())
				{
					VisualStudioInstance current = _003C_003E7__wrap1.Current;
					_003C_003E2__current = current;
					_003C_003E1__state = 1;
					return true;
				}

				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = null;
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
			if (_003C_003E7__wrap1 != null)
			{
				_003C_003E7__wrap1.Dispose();
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
			_003CGetInstances_003Ed__29 _003CGetInstances_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CGetInstances_003Ed__ = this;
			}
			else
			{
				_003CGetInstances_003Ed__ = new _003CGetInstances_003Ed__29(0);
			}

			_003CGetInstances_003Ed__.options = _003C_003E3__options;
			return _003CGetInstances_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<VisualStudioInstance>)this).GetEnumerator();
		}
	}

	private const string MSBuildPublicKeyToken = "b03f5f7f11d50a3a";

	private static readonly string[] s_msBuildAssemblies = new string[5] { "Microsoft.Build", "Microsoft.Build.Engine", "Microsoft.Build.Framework", "Microsoft.Build.Tasks.Core", "Microsoft.Build.Utilities.Core" };

	private static Func<AssemblyLoadContext, AssemblyName, Assembly> s_registeredHandler;

	public static bool IsRegistered => s_registeredHandler != null;

	public static bool AllowQueryAllRuntimeVersions { get; set; } = false;


	public static bool AllowQueryAllDotnetLocations { get; set; } = false;


	public static bool CanRegister
	{
		get
		{
			if (!IsRegistered)
			{
				return !LoadedMsBuildAssemblies.Any();
			}

			return false;
		}
	}

	private static IEnumerable<Assembly> LoadedMsBuildAssemblies => AppDomain.CurrentDomain.GetAssemblies().Where(IsMSBuildAssembly);

	public static IEnumerable<VisualStudioInstance> QueryVisualStudioInstances()
	{
		return QueryVisualStudioInstances(VisualStudioInstanceQueryOptions.Default);
	}

	public static IEnumerable<VisualStudioInstance> QueryVisualStudioInstances(VisualStudioInstanceQueryOptions options)
	{
		return QueryVisualStudioInstances(GetInstances(options), options);
	}

	internal static IEnumerable<VisualStudioInstance> QueryVisualStudioInstances(IEnumerable<VisualStudioInstance> instances, VisualStudioInstanceQueryOptions options)
	{
		return instances.Where((VisualStudioInstance i) => options.DiscoveryTypes.HasFlag(i.DiscoveryType));
	}

	public static VisualStudioInstance RegisterDefaults()
	{
		VisualStudioInstance? obj = GetInstances(VisualStudioInstanceQueryOptions.Default).FirstOrDefault() ?? throw new InvalidOperationException("No instances of MSBuild could be detected." + Environment.NewLine + "Try calling RegisterInstance or RegisterMSBuildPath to manually register one.");
		RegisterInstance(obj);
		return obj;
	}

	public static void RegisterInstance(VisualStudioInstance instance)
	{
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}

		if (instance.DiscoveryType == DiscoveryType.DotNetSdk)
		{
			ApplyDotNetSdkEnvironmentVariables(instance.MSBuildPath);
		}

		string fullPath = Path.GetFullPath(Path.Combine(instance.MSBuildPath, "..", "..", "..", "Common7", "IDE", "CommonExtensions", "Microsoft", "NuGet"));
		if (Directory.Exists(fullPath))
		{
			RegisterMSBuildPathsInternally(new string[2] { instance.MSBuildPath, fullPath });
		}
		else
		{
			RegisterMSBuildPathsInternally(new string[1] { instance.MSBuildPath });
		}
	}

	public static void RegisterMSBuildPath(string msbuildPath)
	{
		ApplyDotNetSdkEnvironmentVariables(msbuildPath);
		RegisterMSBuildPathsInternally(new string[1] { msbuildPath });
	}

	public static void RegisterMSBuildPath(string[] msbuildSearchPaths)
	{
		if (msbuildSearchPaths.Any())
		{
			ApplyDotNetSdkEnvironmentVariables(msbuildSearchPaths.FirstOrDefault());
		}

		RegisterMSBuildPathsInternally(msbuildSearchPaths);
	}

	private static void RegisterMSBuildPathsInternally(string[] msbuildSearchPaths)
	{
		if (msbuildSearchPaths.Length < 1)
		{
			throw new ArgumentException("Must provide at least one search path to RegisterMSBuildPath.");
		}

		List<ArgumentException> list = new List<ArgumentException>();
		for (int i = 0; i < msbuildSearchPaths.Length; i++)
		{
			if (string.IsNullOrWhiteSpace(msbuildSearchPaths[i]))
			{
				list.Add(new ArgumentException($"Value at position {i + 1} may not be null or whitespace", "msbuildSearchPaths"));
			}
		}

		if (list.Count > 0)
		{
			throw new AggregateException("Search paths for MSBuild assemblies cannot be null and must contain non-whitespace characters.", list);
		}

		IEnumerable<string> source = msbuildSearchPaths.Where((string path) => !Directory.Exists(path));
		if (source.Any())
		{
			throw new AggregateException("A directory or directories in \"msbuildSearchPaths\" do not exist", source.Select((string path) => new ArgumentException("Directory \"" + path + "\" does not exist", "msbuildSearchPaths")));
		}

		if (!CanRegister)
		{
			string text = string.Join(Environment.NewLine, LoadedMsBuildAssemblies.Select((Assembly a) => a.GetName()));
			throw new InvalidOperationException($"{typeof(MSBuildLocator)}.{"RegisterInstance"} was called, but MSBuild assemblies were already loaded." + Environment.NewLine + "Ensure that RegisterInstance is called before any method that directly references types in the Microsoft.Build namespace has been called." + Environment.NewLine + "This dependency arises from when a method is just-in-time compiled, so it breaks even if the reference to a Microsoft.Build type has not been executed." + Environment.NewLine + "For more details, see aka.ms/RegisterMSBuildLocator" + Environment.NewLine + "Loaded MSBuild assemblies: " + text);
		}

		Dictionary<string, Assembly> loadedAssemblies = new Dictionary<string, Assembly>();
		s_registeredHandler = (AssemblyLoadContext _, AssemblyName assemblyName) => TryLoadAssembly(assemblyName);
		AssemblyLoadContext.Default.Resolving += s_registeredHandler;
		Assembly TryLoadAssembly(AssemblyName assemblyName)
		{
			lock (loadedAssemblies)
			{
				if (loadedAssemblies.TryGetValue(assemblyName.FullName, out var value))
				{
					return value;
				}

				string[] array = msbuildSearchPaths;
				for (int j = 0; j < array.Length; j++)
				{
					string text2 = Path.Combine(array[j], assemblyName.Name + ".dll");
					if (File.Exists(text2))
					{
						value = Assembly.LoadFrom(text2);
						loadedAssemblies.Add(assemblyName.FullName, value);
						return value;
					}
				}

				return null;
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static void Unregister()
	{
	}

	private static void ApplyDotNetSdkEnvironmentVariables(string dotNetSdkPath)
	{
		foreach (KeyValuePair<string, string> item in new Dictionary<string, string>
		{
			["MSBUILD_EXE_PATH"] = Path.Combine(dotNetSdkPath, "MSBuild.dll"),
			["MSBuildExtensionsPath"] = dotNetSdkPath,
			["MSBuildSDKsPath"] = Path.Combine(dotNetSdkPath, "Sdks")
		})
		{
			Environment.SetEnvironmentVariable(item.Key, item.Value);
		}
	}

	private static bool IsMSBuildAssembly(Assembly assembly)
	{
		return IsMSBuildAssembly(assembly.GetName());
	}

	private static bool IsMSBuildAssembly(AssemblyName assemblyName)
	{
		if (!s_msBuildAssemblies.Contains<string>(assemblyName.Name, StringComparer.OrdinalIgnoreCase))
		{
			return false;
		}

		byte[] publicKeyToken = assemblyName.GetPublicKeyToken();
		if (publicKeyToken == null || publicKeyToken.Length == 0)
		{
			return false;
		}

		StringBuilder stringBuilder = new StringBuilder();
		byte[] array = publicKeyToken;
		foreach (byte value in array)
		{
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(0, 1, stringBuilder2);
			handler.AppendFormatted(value, "x2");
			stringBuilder2.Append(ref handler);
		}

		return stringBuilder.ToString().Equals("b03f5f7f11d50a3a", StringComparison.OrdinalIgnoreCase);
	}

	[IteratorStateMachine(typeof(_003CGetInstances_003Ed__29))]
	private static IEnumerable<VisualStudioInstance> GetInstances(VisualStudioInstanceQueryOptions options)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetInstances_003Ed__29(-2)
		{
			_003C_003E3__options = options
		};
	}
}