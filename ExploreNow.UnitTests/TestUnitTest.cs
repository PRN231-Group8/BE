using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExploreNow.UnitTests
{
    [TestFixture]
    public class TestUnitTest
    {
        [Test]
        public void AnBaToCom()
        {
            string result = "anbatocom";
            string expected = "anbatocom";

            Assert.That(result,Is.EqualTo(expected));
        }
    }
}
