using Microsoft.Extensions.Logging;
using Rocket.Surgery.Experimental.Analyzers.Tests.Helpers;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Experimental.Analyzers.Tests
{
    public class AutoImplementTests : GeneratorTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public AutoImplementTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper, LogLevel.Trace)
        {
            _testOutputHelper = testOutputHelper;
            WithGenerator<AutoImplementGenerator>();
            IgnoreOutputFile("Attribute.cs");
            IgnoreOutputFile("AssemblyData.cs");
        }

        [Fact]
        public async Task Should_Emit_Extension_Class()
        {
            AddSources(
                @"using Rocket.Surgery;
using System.Linq;
using System.Collections.Generic;
namespace Data
{
    [AutoImplement]
    public interface IColorize
    {
        string Color { get; set; }
        public string InverseColor => string.Join("""", Color.Reverse());
        public string GetP() => ""abcd"";
    }
}",
                @"using Data;
namespace Other
{
    partial class Colorable : IColorize
    {
    }
}"
            );

            var result = await GenerateAsync();
            result.AssertGeneratedAsExpected<AutoImplementGenerator>(
                @"using Rocket.Surgery;
using System.Linq;
using System.Collections.Generic;

namespace Other
{
    partial class Colorable : IColorize
    {
        public string Color
        {
            get;
            set;
        }

        public string InverseColor => string.Join("""", Color.Reverse());
        public string GetP() => ""abcd"";
    }
}"
            );
            result.AssertGenerationWasSuccessful();

            // result.AssertCompilationWasSuccessful();
        }


        [Fact]
        public async Task Should_Emit_Extension_Class_Across_Assemblies()
        {
            var reference = new GeneratorTester("Library", AssemblyLoadContext, _testOutputHelper);
            reference.WithGenerator<AutoImplementGenerator>();
            reference.AddSources(
                @"using Rocket.Surgery;
using System.Linq;
using System.Collections.Generic;
namespace Data
{
    [AutoImplement]
    public interface IColorize
    {
        string Color { get; set; }
        public string InverseColor => Color[1..];
        public string GetP() => ""abcd"";
    }
}"
            );

            AddSources(
                @"using Data;
namespace Other
{
    partial class Colorable : IColorize
    {
    }
}"
            );

            AddCompilationReference(reference.Compile());
            var result = await GenerateAsync();
            result.AssertGeneratedAsExpected<AutoImplementGenerator>(
                @"using Rocket.Surgery;
using System.Linq;
using System.Collections.Generic;

namespace Other
{
    partial class Colorable : IColorize
    {
        public string Color
        {
            get;
            set;
        }

        public string InverseColor => Color[1..];
        public string GetP() => ""abcd"";
    }
}"
            );
            result.AssertGenerationWasSuccessful();

            // result.AssertCompilationWasSuccessful();
        }

        [Fact]
        public async Task Supports_Class_Mixins()
        {
            AddSources(
                @"using Rocket.Surgery;
using System.Linq;
using System.Collections.Generic;
namespace Data
{
    [AutoImplement]
    public class Colorize
    {
        public string Color { get; set; }
        public string InverseColor => string.Join("""", Color.Reverse());
        public string GetP() => ""abcd"";
    }
}",
                @"using Data;
using Rocket.Surgery;
namespace Other
{
    [AutoImplement(typeof(Colorize))]
    partial class Colorable
    {
    }
}"
            );

            var result = await GenerateAsync();
            result.AssertGeneratedAsExpected<AutoImplementGenerator>(
                @"using Rocket.Surgery;
using System.Linq;
using System.Collections.Generic;

namespace Other
{
    partial class Colorable
    {
        public string Color
        {
            get;
            set;
        }

        public string InverseColor => string.Join("""", Color.Reverse());
        public string GetP() => ""abcd"";
    }
}"
            );
            result.AssertGenerationWasSuccessful();
        }

        [Fact]
        public async Task Supports_Two_Mixins()
        {
            AddSources(
                @"using Rocket.Surgery;
using System.Linq;
using System.Collections.Generic;
namespace Data
{
    [AutoImplement]
    public interface IColorize
    {
        string Color { get; set; }
        public string InverseColor => string.Join("""", Color.Reverse());
        public string GetP() => ""abcd"";
    }
}",
                @"using Rocket.Surgery;
using System.Linq;
using System.Collections.Generic;
namespace Data
{
    [Mixin]
    public interface ISizeable
    {
        public bool Large { get; set; }
        public bool Small { get; set; }
        public bool ExtraLarge { get; set; }
        public bool ExtraSmall { get; set; }
        public bool Medium => !Large && !Small && !ExtraLarge && !ExtraSmall;
    }
}",
                @"using Data;
namespace Other
{
    partial class Colorable : IColorize, ISizeable
    {
    }
}"
            );

            var result = await GenerateAsync();
            result.AssertGeneratedAsExpected<AutoImplementGenerator>(
                @"using Rocket.Surgery;
using System.Linq;
using System.Collections.Generic;
using Rocket.Surgery;
using System.Linq;
using System.Collections.Generic;

namespace Other
{
    partial class Colorable : IColorize, ISizeable
    {
        public string Color
        {
            get;
            set;
        }

        public string InverseColor => string.Join("""", Color.Reverse());
        public string GetP() => ""abcd"";
        public bool Large
        {
            get;
            set;
        }

        public bool Small
        {
            get;
            set;
        }

        public bool ExtraLarge
        {
            get;
            set;
        }

        public bool ExtraSmall
        {
            get;
            set;
        }

        public bool Medium => !Large && !Small && !ExtraLarge && !ExtraSmall;
    }
}"
            );
            result.AssertGenerationWasSuccessful();
        }
    }
}