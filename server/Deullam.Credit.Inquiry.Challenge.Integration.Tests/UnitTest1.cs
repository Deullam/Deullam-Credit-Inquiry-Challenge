using FluentAssertions;

namespace Deullam.Credit.Inquiry.Challenge.Integration.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            int var = 1;
            var.Should().Be(1);
        }
    }
}