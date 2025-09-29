using System.Runtime.InteropServices;

namespace Microsoft.Build.Locator;

internal class NativeMethods
{
	internal enum hostfxr_resolve_sdk2_flags_t
	{
		disallow_prerelease = 1
	}

	internal enum hostfxr_resolve_sdk2_result_key_t
	{
		resolved_sdk_dir,
		global_json_path
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
	internal delegate void hostfxr_resolve_sdk2_result_fn(hostfxr_resolve_sdk2_result_key_t key, string value);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Auto)]
	internal delegate void hostfxr_get_available_sdks_result_fn(hostfxr_resolve_sdk2_result_key_t key, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] string[] value);

	internal const string HostFxrName = "hostfxr";

	[DllImport("hostfxr", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, ExactSpelling = true)]
	internal static extern int hostfxr_resolve_sdk2(string exe_dir, string working_dir, hostfxr_resolve_sdk2_flags_t flags, hostfxr_resolve_sdk2_result_fn result);

	[DllImport("hostfxr", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, ExactSpelling = true)]
	internal static extern int hostfxr_get_available_sdks(string exe_dir, hostfxr_get_available_sdks_result_fn result);

	[DllImport("libc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern nint realpath(string path, nint buffer);

	[DllImport("libc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	internal static extern void free(nint ptr);
}