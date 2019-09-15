<!--
GENERATED FILE - DO NOT EDIT
This file was generated by [MarkdownSnippets](https://github.com/SimonCropp/MarkdownSnippets).
Source File: /readme.source.md
To change this file edit the source file and then run MarkdownSnippets.
-->

# <img src="/src/icon.png" height="30px"> XunitLogger

[![Build status](https://ci.appveyor.com/api/projects/status/sdg2ni2jhe2o33le/branch/master?svg=true)](https://ci.appveyor.com/project/SimonCropp/XunitLogger)
[![NuGet Status](https://img.shields.io/nuget/v/XunitLogger.svg?label=ApprovalTests&cacheSeconds=86400)](https://www.nuget.org/packages/XunitLogger/)
[![NuGet Status](https://img.shields.io/nuget/v/Xunit.ApprovalTests.svg?label=Xunit.ApprovalTests&cacheSeconds=86400)](https://www.nuget.org/packages/Xunit.ApprovalTests/)

Extends [xUnit](https://xunit.net/) to simplify logging.

Redirects [Trace.Write](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.trace.write), [Debug.Write](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.debug.write), and [Console.Write and Console.Error.Write](https://docs.microsoft.com/en-us/dotnet/api/system.console.write) to [ITestOutputHelper](https://xunit.net/docs/capturing-output). Also provides static access to the current [ITestOutputHelper](https://xunit.net/docs/capturing-output) for use within testing utility methods.

Uses [AsyncLocal](https://docs.microsoft.com/en-us/dotnet/api/system.threading.asynclocal-1) to track state.

<!-- toc -->
## Contents

  * [NuGet package](#nuget-package)
  * [ClassBeingTested](#classbeingtested)
  * [XunitLoggingBase](#xunitloggingbase)
  * [XunitLogging](#xunitlogging)
  * [Filters](#filters)
  * [Context](#context)
    * [Current Test](#current-test)
    * [Test Failure](#test-failure)
    * [Counters](#counters)
  * [Logging Libs](#logging-libs)
  * [Xunit.ApprovalTests](#xunitapprovaltests)
    * [Usage](#usage)
<!-- endtoc -->



## NuGet package

https://nuget.org/packages/XunitLogger/


## ClassBeingTested

<!-- snippet: ClassBeingTested.cs -->
<a id='snippet-ClassBeingTested.cs'/></a>
```cs
using System;
using System.Diagnostics;

static class ClassBeingTested
{
    public static void Method()
    {
        Trace.WriteLine("From Trace");
        Console.WriteLine("From Console");
        Debug.WriteLine("From Debug");
        Console.Error.WriteLine("From Console Error");
    }
}
```
<sup>[snippet source](/src/XunitLogger.Tests/Snippets/ClassBeingTested.cs#L1-L13) / [anchor](#snippet-ClassBeingTested.cs)</sup>
<!-- endsnippet -->


## XunitLoggingBase

`XunitLoggingBase` is an abstract base class for tests. It exposes logging methods for use from unit tests, and handle the flushing of longs in its `Dispose` method. `XunitLoggingBase` is actually a thin wrapper over `XunitLogging`. `XunitLogging`s `Write*` methods can also be use inside a test inheriting from `XunitLoggingBase`.

<!-- snippet: TestBaseSample.cs -->
<a id='snippet-TestBaseSample.cs'/></a>
```cs
using Xunit;
using Xunit.Abstractions;

public class TestBaseSample  :
    XunitLoggingBase
{
    [Fact]
    public void Write_lines()
    {
        WriteLine("From Test");
        ClassBeingTested.Method();

        var logs = XunitLogging.Logs;

        Assert.Contains("From Test", logs);
        Assert.Contains("From Trace", logs);
        Assert.Contains("From Debug", logs);
        Assert.Contains("From Console", logs);
        Assert.Contains("From Console Error", logs);
    }

    public TestBaseSample(ITestOutputHelper output) :
        base(output)
    {
    }
}
```
<sup>[snippet source](/src/XunitLogger.Tests/Snippets/TestBaseSample.cs#L1-L26) / [anchor](#snippet-TestBaseSample.cs)</sup>
<!-- endsnippet -->


## XunitLogging

`XunitLogging` provides static access to the logging state for tests. It exposes logging methods for use from unit tests, however registration of [ITestOutputHelper](https://xunit.net/docs/capturing-output) and flushing of logs must be handled explicitly.

<!-- snippet: XunitLoggerSample.cs -->
<a id='snippet-XunitLoggerSample.cs'/></a>
```cs
using System;
using Xunit;
using Xunit.Abstractions;

public class XunitLoggerSample :
    IDisposable
{
    [Fact]
    public void Usage()
    {
        XunitLogging.WriteLine("From Test");

        ClassBeingTested.Method();

        var logs = XunitLogging.Logs;

        Assert.Contains("From Test", logs);
        Assert.Contains("From Trace", logs);
        Assert.Contains("From Debug", logs);
        Assert.Contains("From Console", logs);
        Assert.Contains("From Console Error", logs);
    }

    public XunitLoggerSample(ITestOutputHelper testOutput)
    {
        XunitLogging.Register(testOutput);
    }

    public void Dispose()
    {
        XunitLogging.Flush();
    }
}
```
<sup>[snippet source](/src/XunitLogger.Tests/Snippets/XunitLoggerSample.cs#L1-L33) / [anchor](#snippet-XunitLoggerSample.cs)</sup>
<!-- endsnippet -->

`XunitLogging` redirects [Trace.Write](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.trace.write), [Console.Write](https://docs.microsoft.com/en-us/dotnet/api/system.console.write), and [Debug.Write](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.debug.write) in its static constructor.

<!-- snippet: writeRedirects -->
<a id='snippet-writeredirects'/></a>
```cs
Trace.Listeners.Clear();
Trace.Listeners.Add(new TraceListener());
#if (NETSTANDARD)
DebugPoker.Overwrite(
    text =>
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        if (text.EndsWith(Environment.NewLine))
        {
            WriteLine(text.TrimTrailingNewline());
            return;
        }

        Write(text);
    });
#else
Debug.Listeners.Clear();
Debug.Listeners.Add(new TraceListener());
#endif
var writer = new TestWriter();
Console.SetOut(writer);
Console.SetError(writer);
```
<sup>[snippet source](/src/XunitLogger/XunitLogging.cs#L49-L78) / [anchor](#snippet-writeredirects)</sup>
<!-- endsnippet -->

These API calls are then routed to the correct xUnit [ITestOutputHelper](https://xunit.net/docs/capturing-output) via a static [AsyncLocal](https://docs.microsoft.com/en-us/dotnet/api/system.threading.asynclocal-1).


## Filters

`XunitLogger.Filters` can be used to filter out unwanted lines:

<!-- snippet: FilterSample.cs -->
<a id='snippet-FilterSample.cs'/></a>
```cs
using Xunit;
using Xunit.Abstractions;
using XunitLogger;

public class FilterSample :
    XunitLoggingBase
{
    static FilterSample()
    {
        Filters.Add(x => x != null && !x.Contains("ignored"));
    }

    [Fact]
    public void Write_lines()
    {
        WriteLine("first");
        WriteLine("with ignored string");
        WriteLine("last");
        var logs = XunitLogging.Logs;

        Assert.Contains("first", logs);
        Assert.DoesNotContain("with ignored string", logs);
        Assert.Contains("last", logs);
    }

    public FilterSample(ITestOutputHelper output) :
        base(output)
    {
    }
}
```
<sup>[snippet source](/src/XunitLogger.Tests/Snippets/FilterSample.cs#L1-L30) / [anchor](#snippet-FilterSample.cs)</sup>
<!-- endsnippet -->

Filters are static and shared for all tests.


## Context

For every tests there is a contextual API to perform several operations.

 * `Context.TestOutput`: Access to [ITestOutputHelper](https://xunit.net/docs/capturing-output).
 * `Context.Write` and `Context.WriteLine`: Write to the current log.
 * `Context.LogMessages`: Access to all log message for the current test.
 * Counters: Provide access in predicable and incrementing values for the following types: `Guid`, `Int`, `Long`, `UInt`, and `ULong`.
 * `Context.Test`: Access to the current `ITest`.

There is also access via a static API.

<!-- snippet: ContextSample.cs -->
<a id='snippet-ContextSample.cs'/></a>
```cs
using Xunit;
using Xunit.Abstractions;

public class ContextSample  :
    XunitLoggingBase
{
    [Fact]
    public void Usage()
    {
        var currentGuid = Context.CurrentGuid;

        var nextGuid = Context.NextGuid();

        Context.WriteLine("Some message");

        var currentLogMessages = Context.LogMessages;

        var testOutputHelper = Context.TestOutput;

        var currentTest = Context.Test;
    }

    [Fact]
    public void StaticUsage()
    {
        var currentGuid = XunitLogging.Context.CurrentGuid;

        var nextGuid = XunitLogging.Context.NextGuid();

        XunitLogging.Context.WriteLine("Some message");

        var currentLogMessages = XunitLogging.Context.LogMessages;

        var testOutputHelper = XunitLogging.Context.TestOutput;

        var currentTest = XunitLogging.Context.Test;
    }

    public ContextSample(ITestOutputHelper output) :
        base(output)
    {
    }
}
```
<sup>[snippet source](/src/XunitLogger.Tests/Snippets/ContextSample.cs#L1-L43) / [anchor](#snippet-ContextSample.cs)</sup>
<!-- endsnippet -->


### Current Test

There is currently no API in xUnit to retrieve information on the current test. See issues [#1359](https://github.com/xunit/xunit/issues/1359), [#416](https://github.com/xunit/xunit/issues/416), and [#398](https://github.com/xunit/xunit/issues/398).

To work around this, this project exposes the current instance of `ITest` via reflection.

Usage:

<!-- snippet: CurrentTestSample.cs -->
<a id='snippet-CurrentTestSample.cs'/></a>
```cs
using Xunit;
using Xunit.Abstractions;

public class CurrentTestSample :
    XunitLoggingBase
{
    [Fact]
    public void Usage()
    {
        var currentTest = Context.Test;
        // DisplayName will be 'TestNameSample.Usage'
        var displayName = currentTest.DisplayName;
    }

    [Fact]
    public void StaticUsage()
    {
        var currentTest = XunitLogging.Context.Test;
        // DisplayName will be 'TestNameSample.StaticUsage'
        var displayName = currentTest.DisplayName;
    }

    public CurrentTestSample(ITestOutputHelper output) :
        base(output)
    {
    }
}
```
<sup>[snippet source](/src/XunitLogger.Tests/Snippets/CurrentTestSample.cs#L1-L27) / [anchor](#snippet-CurrentTestSample.cs)</sup>
<!-- endsnippet -->

Implementation:

<!-- snippet: LoggingContext_CurrentTest.cs -->
<a id='snippet-LoggingContext_CurrentTest.cs'/></a>
```cs
using System;
using System.Reflection;
using Xunit.Abstractions;

namespace XunitLogger
{
    public partial class Context
    {
        ITest test;

        static FieldInfo testMember;
        public ITest Test
        {
            get
            {
                if (test == null)
                {
                    InitTestMethod();

                    test = (ITest) testMember.GetValue(TestOutput);
                }

                return test;
            }
        }

        void InitTestMethod()
        {
            if (testMember != null)
            {
                return;
            }
            var testOutputType = TestOutput.GetType();
            testMember = testOutputType.GetField("test", BindingFlags.Instance | BindingFlags.NonPublic);
            if (testMember == null)
            {
                throw new Exception($"Unable to find 'test' field on {testOutputType.FullName}");
            }
        }
    }
}
```
<sup>[snippet source](/src/XunitLogger/LoggingContext_CurrentTest.cs#L1-L41) / [anchor](#snippet-LoggingContext_CurrentTest.cs)</sup>
<!-- endsnippet -->


### Test Failure

When a test fails it is expressed as an exception. The exception can be viewed by enabling exception capture, and then accessing `Context.TestException`. The `TestException` will be null if the test has passed.

One common case is to perform some logic, based on the existence of the exception, in the `Dispose` of a test.

<!-- snippet: TestExceptionSample -->
<a id='snippet-testexceptionsample'/></a>
```cs
public class TestExceptionSample :
    XunitLoggingBase
{
    static TestExceptionSample()
    {
        //Should be called once at appdomain startup
        XunitLogging.EnableExceptionCapture();
    }

    [Fact]
    public void Usage()
    {
        //This tests will fail
        Assert.False(true);
    }

    public TestExceptionSample(ITestOutputHelper output) :
        base(output)
    {
    }

    public override void Dispose()
    {
        var theExceptionThrownByTest = Context.TestException;
        var testDisplayName = Context.Test.DisplayName;
        var testCase = Context.Test.TestCase;
        base.Dispose();
    }
}
```
<sup>[snippet source](/src/XunitLogger.Tests/Snippets/TestExceptionSample.cs#L6-L37) / [anchor](#snippet-testexceptionsample)</sup>
<!-- endsnippet -->


### Counters

Provide access to predicable and incrementing values for the following types: `Guid`, `Int`, `Long`, `UInt`, and `ULong`.


#### Non Test Context usage

Counters can also be used outside of the current test context:

<!-- snippet: NonTestContextUsage -->
<a id='snippet-nontestcontextusage'/></a>
```cs
var current = Counters.CurrentGuid;

var next = Counters.NextGuid();

var counter = new GuidCounter();
var localCurrent = counter.Current;
var localNext = counter.Next();
```
<sup>[snippet source](/src/XunitLogger.Tests/Snippets/CountersSample.cs#L9-L17) / [anchor](#snippet-nontestcontextusage)</sup>
<!-- endsnippet -->


#### Implementation

<!-- snippet: LoggingContext_Counters.cs -->
<a id='snippet-LoggingContext_Counters.cs'/></a>
```cs
using System;

namespace XunitLogger
{
    public partial class Context
    {
        GuidCounter GuidCounter = new GuidCounter();
        IntCounter IntCounter = new IntCounter();
        LongCounter LongCounter = new LongCounter();
        UIntCounter UIntCounter = new UIntCounter();
        ULongCounter ULongCounter = new ULongCounter();

        public uint CurrentUInt
        {
            get => UIntCounter.Current;
        }

        public int CurrentInt
        {
            get => IntCounter.Current;
        }

        public long CurrentLong
        {
            get => LongCounter.Current;
        }

        public ulong CurrentULong
        {
            get => ULongCounter.Current;
        }

        public Guid CurrentGuid
        {
            get => GuidCounter.Current;
        }

        public uint NextUInt()
        {
            return UIntCounter.Next();
        }

        public int NextInt()
        {
            return IntCounter.Next();
        }

        public long NextLong()
        {
            return LongCounter.Next();
        }

        public ulong NextULong()
        {
            return ULongCounter.Next();
        }

        public Guid NextGuid()
        {
            return GuidCounter.Next();
        }
    }
}
```
<sup>[snippet source](/src/XunitLogger/LoggingContext_Counters.cs#L1-L63) / [anchor](#snippet-LoggingContext_Counters.cs)</sup>
<!-- endsnippet -->

<!-- snippet: Counters.cs -->
<a id='snippet-Counters.cs'/></a>
```cs
using System;

namespace XunitLogger
{
    public static class Counters
    {
        static GuidCounter GuidCounter = new GuidCounter();
        static IntCounter IntCounter = new IntCounter();
        static LongCounter LongCounter = new LongCounter();
        static UIntCounter UIntCounter = new UIntCounter();
        static ULongCounter ULongCounter = new ULongCounter();

        public static uint CurrentUInt
        {
            get => UIntCounter.Current;
        }

        public static int CurrentInt
        {
            get => IntCounter.Current;
        }

        public static long CurrentLong
        {
            get => LongCounter.Current;
        }

        public static ulong CurrentULong
        {
            get => ULongCounter.Current;
        }

        public static Guid CurrentGuid
        {
            get => GuidCounter.Current;
        }

        public static uint NextUInt()
        {
            return UIntCounter.Next();
        }

        public static int NextInt()
        {
            return IntCounter.Next();
        }

        public static long NextLong()
        {
            return LongCounter.Next();
        }

        public static ulong NextULong()
        {
            return ULongCounter.Next();
        }

        public static Guid NextGuid()
        {
            return GuidCounter.Next();
        }
    }
}
```
<sup>[snippet source](/src/XunitLogger/Counters.cs#L1-L63) / [anchor](#snippet-Counters.cs)</sup>
<!-- endsnippet -->

<!-- snippet: GuidCounter.cs -->
<a id='snippet-GuidCounter.cs'/></a>
```cs
using System;
using System.Threading;

namespace XunitLogger
{
    public class GuidCounter
    {
        int current;

        public Guid Current
        {
            get => IntToGuid(current);
        }

        public Guid Next()
        {
            var value = Interlocked.Increment(ref current);
            return IntToGuid(value);
        }

        static Guid IntToGuid(int value)
        {
            var bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            return new Guid(bytes);
        }
    }
}
```
<sup>[snippet source](/src/XunitLogger/Counters/GuidCounter.cs#L1-L28) / [anchor](#snippet-GuidCounter.cs)</sup>
<!-- endsnippet -->

<!-- snippet: LongCounter.cs -->
<a id='snippet-LongCounter.cs'/></a>
```cs
using System.Threading;

namespace XunitLogger
{
    public class LongCounter
    {
        long current;

        public long Current
        {
            get => current;
        }

        public long Next()
        {
            return Interlocked.Increment(ref current);
        }
    }
}
```
<sup>[snippet source](/src/XunitLogger/Counters/LongCounter.cs#L1-L19) / [anchor](#snippet-LongCounter.cs)</sup>
<!-- endsnippet -->


## Logging Libs

Approaches to routing common logging libraries to Diagnostics.Trace:

 * [Serilog](https://serilog.net/) use [Serilog.Sinks.Trace](https://github.com/serilog/serilog-sinks-trace).
 * [NLog](https://github.com/NLog/NLog) use a [Trace target](https://github.com/NLog/NLog/wiki/Trace-target).


## Xunit.ApprovalTests

The default behavior of ApprovalTests uses the [StackTrace](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.stacktrace) to derive the current test and hence compute the name of the approval file. This has several drawbacks/issues:

 * Fragility: Deriving the test name from a stack trace is dependent on several things to be configured correctly. Optimization must be disabled to avoid in-lining and debug symbols enabled and parsable.
 * Performance impact: Computing a stack trace is a relatively expensive operation. Disabling optimization also impacts performance

Xunit.ApprovalTests avoids these problems by using the current xUnit context to derive the approval file name.


### Usage

Usage is done via inheriting from a base class `XunitApprovalBase`

<!-- snippet: Xunit.ApprovalTests.Tests/UsingTestBase.cs -->
<a id='snippet-Xunit.ApprovalTests.Tests/UsingTestBase.cs'/></a>
```cs
using ApprovalTests;
using ApprovalTests.Namers;
using Xunit;
using Xunit.Abstractions;

public class UsingTestBase :
    XunitApprovalBase
{
    [Fact]
    public void Simple()
    {
        Approvals.Verify("SimpleResult");
    }

    [Fact]
    [UseApprovalSubdirectory("SubDir")]
    public void InSubDir()
    {
        Approvals.Verify("SimpleResult");
    }

    [Fact]
    public void AsEnvironmentSpecificTest()
    {
        using (NamerFactory.AsEnvironmentSpecificTest(() => "Foo"))
        {
            Approvals.Verify("Value");
        }
    }

    [Theory]
    [InlineData("Foo")]
    [InlineData(9)]
    [InlineData(true)]
    public void Theory(object value)
    {
        Approvals.Verify(value);
    }

    [Theory]
    [InlineData("Foo")]
    public void TheoryAsEnvironmentSpecificTest(object value)
    {
        using (NamerFactory.AsEnvironmentSpecificTest(() => "Bar"))
        {
            Approvals.Verify(value);
        }
    }

    [Theory]
    [InlineData("Foo", "Bar")]
    [InlineData(9, false)]
    [InlineData(true, -1)]
    public void MultiTheory(object value1, object value2)
    {
        Approvals.Verify($"{value1} {value2}");
    }

    public UsingTestBase(ITestOutputHelper testOutput) :
        base(testOutput)
    {
    }
}
```
<sup>[snippet source](/src/Xunit.ApprovalTests.Tests/UsingTestBase.cs#L1-L63) / [anchor](#snippet-Xunit.ApprovalTests.Tests/UsingTestBase.cs)</sup>
<!-- endsnippet -->


## Release Notes

See [closed milestones](../../milestones?state=closed).


## Icon

[Wolverine](https://thenounproject.com/term/wolverine/18415/) designed by [Mike Rowe](https://thenounproject.com/itsmikerowe/) from [The Noun Project](https://thenounproject.com/).
