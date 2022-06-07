using BenchmarkDotNet.Attributes;
using System.Runtime.CompilerServices;

namespace UsingUnsafeAs;

[MemoryDiagnoser]
public class CastingObjects
{
	private readonly object data = new Data();

	[Benchmark]
	public Data CastUsingCSharp() => (Data)this.data;

	// https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.unsafe.as
	[Benchmark]
	public Data CastUsingUnsafeAs() => Unsafe.As<Data>(this.data);
}