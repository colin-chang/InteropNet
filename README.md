# InteropNet #

The library allows you to work with native libraries. Standard approach with the `DllImport` attribute may be inconvenient if you want to build AnyCPU assembly with MS.NET/Mono support. The `Interoptor` class can generate implementation of interface with target signatures of native methods.

For example, let's create a native library (`NativeLib`) with the function `int sum(int a, int b) { return a + b; }` and build it in four configuration (Windows/Unix, x86/x64):

	x86/NativeLib.dll
	x86/libNativeLib.so
	x64/NativeLib.dll
	x64/libNativeLib.so

Now we can write the following code:

	public interface INative
	{
	    [RuntimeDllImport("NativeLib", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sum")]
	    int Sum(int a, int b);
	}
	
	class Program
	{
	    static void Main()
	    {
	        var native = Interoptor.Create<INative>();
	        Console.WriteLine("2 + 3 = " + native.Sum(2, 3));
	    }
	}

In the program, we declared interface `INative` with signatures of the target native methods. Each signature should be marked with the `RuntimeDllImport` attribute with name of a native library and other options. The instance of `Interoptor` helped us to create instance of the `INative` interface on the fly. The implementation of the `sum` method call corresponding native method from library that correspond to current architecture and OS. The `Interoptor` generates the following code:

    namespace Interoptor.NativeInstance
    {
      [UnmanagedFunctionPointer(CallingConvention.Cdecl, BestFitMapping = false, CharSet = (CharSet) 0, SetLastError = false, ThrowOnUnmappableChar = false)]
      [StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
      public delegate int SumDelegate(int a, int b);

      public class NativeImplementation : INative
      {
        private SumDelegate SumField;

        public NativeImplementation(LibraryLoader loader)
        {
          IntPtr dllHandle = loader.LoadLibrary("NativeLib", (string) null);
          this.SumField = (SumDelegate) Marshal.GetDelegateForFunctionPointer(loader.GetProcAddress(dllHandle, "sum"), typeof (SumDelegate));
        }

        public int Sum(int a, int b)
        {
          return this.SumField(a, b);
        }
      }
    } 


As a result, we received a single .NET cross-platform AnyCPU-program with calls of native methods because of the `LibraryLoader` class loaded handles for specific user environment.

## Platform
Only Windows and Linux(CentOS 8,Debian 9,Ubuntu 20.04 are tested) are supported.

**We use libdl.so.2 to load library(*.so), it means some Linux release version like alpine doesn't support it.**

the libdl.so.2 file is a library in most of Linux. Generally,it's in a system path configured in system environment variables.

OS | Path
:-|:-
CentOS | `/usr/lib64/libdl.so.2`
Debian | `/lib/x86_64-linux-gnu/libdl.so.2`
Ubuntu | `/usr/lib/x86_64-linux-gnu/libdl.so.2`,`/usr/lib64/ld-linux-x86-64.so.2`

## NuGet

You can install the library via NuGet.
 
[https://www.nuget.org/packages/ColinChang.InteropNet](https://www.nuget.org/packages/ColinChang.InteropNet)

## Thanks
https://github.com/AndreyAkinshin/InteropDotNet
