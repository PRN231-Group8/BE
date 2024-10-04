using NUnit.Framework;

namespace ExploreNow.UnitTests;

[TestFixture]
public class TestUnitTest
{
    [Test]
    public void AnBaToCom()
    {
        var result = "anbatocom";
        var expected = "anbatocom";

        Assert.That(result, Is.EqualTo(expected));
    }
}