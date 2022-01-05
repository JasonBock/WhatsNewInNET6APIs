using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

//DemonstrateCollectionsEnsureCapacity();
//DemonstrateCollectionsPriorityQueue();
DemonstrateCollectionsDictionaryRefValues();
//DemonstrateLinqImprovementsDefaultValues();
//DemonstrateLinqImprovementsByOperators();
//DemonstrateDatesAndTimes();
//await DemonstrateAsynchronousWaitAsync().ConfigureAwait(false);
//DemonstrateOneShotCryptography();
//DemonstrateArgumentNullException();
//DemonstrateNullabilityInfo();

static void DemonstrateCollectionsEnsureCapacity()
{
	Console.WriteLine(nameof(DemonstrateCollectionsEnsureCapacity));
	// Before: You could set the capacity (and you should in the constructor
	// if you know how many elements it will have), but you couldn't change it
	// on some collections, like a HashSet.
	// Note that a HashSet sets the capacity based on a primes table:
	// https://source.dot.net/#System.Private.CoreLib/HashSet.cs,831ca3e6d9c0d78b,references
	var beforeItems = new HashSet<string>(3) { "a", "b", "c" };
	// Future additions will probably incur an allocation.
	beforeItems.Add("d");
	beforeItems.Add("e");
	beforeItems.Add("f");
	beforeItems.Add("g");
	beforeItems.Add("h");
	beforeItems.Add("i");

	// After: If you need to add items after initialization,
	// and you have a good idea how many,
	// you can call EnsureCapacity()
	var afterItems = new HashSet<string>(3) { "a", "b", "c" };
	afterItems.EnsureCapacity(9);
	afterItems.Add("d");
	afterItems.Add("e");
	afterItems.Add("f");
	afterItems.Add("g");
	afterItems.Add("h");
	afterItems.Add("i");
}

static void DemonstrateCollectionsPriorityQueue()
{
	Console.WriteLine(nameof(DemonstrateCollectionsPriorityQueue));
	// Before: A queue is FIFO, but there's no way
	// to prioritize them.
	var beforeQueue = new Queue<int>(6);
	beforeQueue.Enqueue(1);
	beforeQueue.Enqueue(2);
	beforeQueue.Enqueue(3);
	beforeQueue.Enqueue(4);
	beforeQueue.Enqueue(5);
	beforeQueue.Enqueue(6);

	while (beforeQueue.TryDequeue(out var beforeItem))
	{
		Console.WriteLine($"Queue: {beforeItem}");
	}

	// After: PriorityQueue gives you a way
	// to do prioritization.
	var afterQueue = new PriorityQueue<int, int>(6);
	afterQueue.Enqueue(1, 3);
	afterQueue.Enqueue(2, 7);
	afterQueue.Enqueue(3, 5);
	afterQueue.Enqueue(4, 1);
	afterQueue.Enqueue(5, -1);
	afterQueue.Enqueue(6, 5);

	while (afterQueue.TryDequeue(out var afterItem, out var afterPriority))
	{
		Console.WriteLine($"PriorityQueue: {afterItem}, {afterPriority}");
	}
}

static void DemonstrateCollectionsDictionaryRefValues()
{
	Console.WriteLine(nameof(DemonstrateCollectionsDictionaryRefValues));

	var items = new Dictionary<string, int>()
	{
		{ "a", 1}, { "b", 2 }, { "c", 3 }
	};

	// Before: Changing a collection with struct values means you do a copy.
	items["a"]++;
	items["b"]++;
	items["c"]++;

	foreach (var item in items)
	{
		Console.WriteLine($"Before: {item}");
	}

	// After: You can now use GetValueRefOrNullRef(), though the suggestion
	// is to do it for hot paths only.
	ref var valueA = ref CollectionsMarshal.GetValueRefOrNullRef(items, "a");
	valueA++;
	ref var valueB = ref CollectionsMarshal.GetValueRefOrNullRef(items, "b");
	valueB++;
	ref var valueC = ref CollectionsMarshal.GetValueRefOrNullRef(items, "c");
	valueC++;

	foreach (var item in items)
	{
		Console.WriteLine($"After: {item}");
	}
}

static void DemonstrateLinqImprovementsDefaultValues()
{
	Console.WriteLine(nameof(DemonstrateLinqImprovementsDefaultValues));

	var items = new List<int>() { 1, 2, 3, 4, 5, 6 };

	// Before: The "default" value wasn't settable for those
	// LINQ functions that provide a default value, so in this case, we get 0
	// (the default value of int).
	Console.WriteLine($"Before: {items.FirstOrDefault(_ => _ < 0)}");

	// After: You can provide a default value.
	Console.WriteLine($"After: {items.FirstOrDefault(_ => _ < 0, -1)}");
}

// https://github.com/dotnet/runtime/issues/27687
static void DemonstrateLinqImprovementsByOperators()
{
	Console.WriteLine(nameof(DemonstrateLinqImprovementsByOperators));

	var guitars = new Guitar[]
	{
		new Guitar("PRS", 7),
		new Guitar("Ovation", 12),
		new Guitar("Warwick", 5),
		new Guitar("Charvel", 6),
	};

	// Before: With Min(), you'd actually get the key value.
	Console.WriteLine($"Before: {guitars.Min(_ => _.StringCount)}");

	// After: You can now use MinBy() which will return
	// the object itself.
	Console.WriteLine($"After: {guitars.MinBy(_ => _.StringCount)}");
}

static void DemonstrateDatesAndTimes()
{
	Console.WriteLine(nameof(DemonstrateDatesAndTimes));

	// Before: If you wanted to represent just a date or time...
	// you couldn't. You had to use a Date, or a TimeSpan for time.
	// But timezones and "overflow" can make for unexpected results.
	var beforeDate = new DateTime(2022, 1, 5);
	var beforeTime = new DateTime(2022, 1, 5, 13, 13, 0);
	var beforeTimeSpan = new TimeSpan(13, 13, 0);
	Console.WriteLine($"Before: Date is {beforeDate}");
	Console.WriteLine($"Before: Time is {beforeTime}");
	Console.WriteLine($"Before: Time + Time is {beforeTime.Add(beforeTime.TimeOfDay)}");
	Console.WriteLine($"Before: TimeSpan is {beforeTimeSpan}");
	Console.WriteLine($"Before: TimeSpan + TimeSpan is {beforeTimeSpan.Add(beforeTimeSpan)}");

	// After: Now you can use DateOnly and TimeOnly.
	var afterDate = new DateOnly(2022, 1, 5);
	var afterTime = new TimeOnly(13, 13, 0);
	Console.WriteLine($"After: Date is {afterDate}");
	Console.WriteLine($"After: Time is {afterTime}");
	Console.WriteLine($"After: Time + Time is {afterTime.Add(afterTime.ToTimeSpan())}");
}

static async Task DemonstrateAsynchronousWaitAsync()
{
	Console.WriteLine(nameof(DemonstrateOneShotCryptography));

	// Before: You have to synchronous wait for a task
	// to complete.
	// Note that CA1849 is telling you to do it the new,
	// "right" way.
	Console.WriteLine("Before: Starting...");
	var beforeTask = Task.Delay(1000);
#pragma warning disable CA1849 // Call async methods when in an async method
	beforeTask.Wait();
#pragma warning restore CA1849 // Call async methods when in an async method
	Console.WriteLine("Before: Ended");

	// After: Now you can use WaitAsync().
	var afterTask = Task.Delay(1000);
	Console.WriteLine("After: Starting...");
	await afterTask.WaitAsync(TimeSpan.FromHours(1)).ConfigureAwait(false);
	Console.WriteLine("Before: Ending...");
}

static void DemonstrateOneShotCryptography()
{
	Console.WriteLine(nameof(DemonstrateOneShotCryptography));

	// Side note: CA5394 will warn you about using Random,
	// so use RandomNumberGenerator.
	// Also, this type added GetInt32() for .NET Core 3.0, which
	// was a nice addition.
	var data = RandomNumberGenerator.GetBytes(256);
	var key = RandomNumberGenerator.GetBytes(16);
	var iv = RandomNumberGenerator.GetBytes(16);

	// Before: For cryptographic operations, you used to have to do more:
	using (var hash = SHA512.Create())
	{
		var beforeDigest = hash.ComputeHash(data);
		Console.WriteLine("Before hash: [{0}]", string.Join(", ", beforeDigest));
		Console.WriteLine();

		using var beforeAes = Aes.Create();
#pragma warning disable CA5401 // Do not use CreateEncryptor with non-default IV
		using var transform = beforeAes.CreateEncryptor(key, iv);
#pragma warning restore CA5401 // Do not use CreateEncryptor with non-default IV
		var beforeEncrypted = transform.TransformFinalBlock(data, 0, data.Length);
		Console.WriteLine("Before encrypted: [{0}]", string.Join(", ", beforeEncrypted));
		Console.WriteLine();
	}

	// After: Now, there are "one-shot" operations.
	var afterDigest = SHA512.HashData(data);
	Console.WriteLine("After hash: [{0}]", string.Join(", ", afterDigest));
	Console.WriteLine();

	using var afterAes = Aes.Create();
	afterAes.Key = key;
	var afterEncrypted = afterAes.EncryptCbc(data, iv);
	Console.WriteLine("After encrypted: [{0}]", string.Join(", ", afterEncrypted));
}

static void DemonstrateArgumentNullException()
{
	Console.WriteLine(nameof(DemonstrateArgumentNullException));

	// Before: You'd have to do a bit to check a parameter for null.
	static void BeforeHandleParameter(string data)
	{
		if (data is null) { throw new ArgumentNullException(nameof(data)); }
	}

	// After: Now this helper method does a bunch for you, including
	// capturing the name of the parameter (thanks to CallerArgumentExpressionAttribute).
	// Note that if this C# feature makes it, it'll probably
	// use this helper method underneath the scenes:
	// https://github.com/dotnet/csharplang/issues/2145
	static void AfterHandleParameter(string data) =>
		ArgumentNullException.ThrowIfNull(data);

	try
	{
		BeforeHandleParameter(null!);
	}
	catch (ArgumentNullException e)
	{
		Console.WriteLine($"Before: {e.Message}");
	}

	try
	{
		AfterHandleParameter(null!);
	}
	catch (ArgumentNullException e)
	{
		Console.WriteLine($"After: {e.Message}");
	}
}

static void DemonstrateNullabilityInfo()
{
	Console.WriteLine(nameof(DemonstrateNullabilityInfo));

	// Before: It's a LONG story, but trying to figure out if a member in the 
	// Reflection API had a nullable annotations was HARD. The values are stored
	// in a synthesized attribute, and you have to look at the values there, or 
	// in its parent, or in the module...it's not fun. I talk about how I solved this
	// in one of my FreeCodeSession videos when I had to do this in Rocks:
	// https://youtu.be/0RbAzPn-67s?t=377.
	// Here's what the code looked like at that time:
	// https://github.com/JasonBock/Rocks/blob/60e2af532035a18e3dc7e2e2e4312961f89bbcc2/Rocks/Extensions/NullableContext.cs
	// Thankfully, we now have a way to do this in the Reflection API proper!

	var method = typeof(ReflectionExample).GetMethod(nameof(ReflectionExample.MethodWithNullableParameters))!;

	var context = new NullabilityInfoContext();

	foreach (var parameter in method.GetParameters())
	{
		var parameterNullabilityInfo = context.Create(parameter);
		Console.WriteLine(
			$"Parameter {parameter.Name} has type {parameter.ParameterType.Name}, and its nullability info value is {parameterNullabilityInfo.WriteState}");
	}
}

public record Guitar(string Manufacturer, int StringCount);

public static class ReflectionExample
{
	public static void MethodWithNullableParameters(string value1, string? value2) { }
}