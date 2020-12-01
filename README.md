# InteropNet
The library allows you to work with native libraries. Standard approach with the `DllImport` attribute may be inconvenient if you want to build AnyCPU assembly with MS.NET/Mono support. The `Interoptor` class can generate implementation of interface with target signatures of native methods.
