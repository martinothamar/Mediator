using ObjectLayoutInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Mediator.Tests
{
    public enum SomeError { Generic, NotFound }

    public readonly partial struct SomeResult : IResult<float, SomeError>
    {
    }

    public sealed class ResultTests
    {
        [Fact]
        public void Test_Basic_Result()
        {
            var result = new SomeResult(2.0f);
            var value = result.Match(v => v, _ => 0f);
            Assert.Equal(2.0f, value);

            result = new SomeResult(SomeError.NotFound);
            var value2 = result.Match(_ => (SomeError)int.MaxValue, e => e);
            Assert.Equal(SomeError.NotFound, value2);

            var layout = TypeLayout.GetLayout<SomeResult>();
        }
    }
}
