using System;
using System.Runtime.InteropServices;

namespace ColinChang.InteropNet.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var native = Interoptor.Create<INative>();
            Console.WriteLine("2 + 3 = " + native.Sum(2, 3));

            Console.ReadKey();
        }
    }

    public interface INative
    {
        [RuntimeDllImport("NativeLib", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sum")]
        int Sum(int a, int b);
    }
}