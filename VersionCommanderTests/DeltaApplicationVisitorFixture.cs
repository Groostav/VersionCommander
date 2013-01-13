using System.Linq;
using NUnit.Framework;
using VersionCommander.Implementation;
using VersionCommander.Implementation.Exceptions;
using VersionCommander.Implementation.Extensions;
using VersionCommander.Implementation.Visitors;
using VersionCommander.UnitTests.TestingAssists;

namespace VersionCommander.UnitTests
{
    [TestFixture]
    public class DeltaApplicationVisitorFixture
    {
        private TestHelper _testHelper;

        [SetUp]
        public void setupTestHelper()
        {
            _testHelper = new TestHelper();
        }
        
        [Test]
        public void when_undoing_on_an_empty_set_of_version_deltas()
        {
            //Setup
            var visitor = new DeltaApplicationVisitor(searchWholeTree:true, 
                                                      changeType:ChangeType.Undo,
                                                      targetSite: null);

            _testHelper.ProvidedDeltaApplicationVisitor = visitor;
            var node = _testHelper.MakeConfiguredVersionControlNodeWithChildren();

            //act
            TestDelegate act = () => node.Accept(visitor);

            //assert
            Assert.Throws<VersionDeltaNotFoundException>(act);
        }

        [Test]
        public void when_undoing_a_version_delta_when_all_version_deltas_are_already_undone()
        {
            //setup
            var visitor = new DeltaApplicationVisitor(changeType: ChangeType.Undo, 
                                                      searchWholeTree: true,
                                                      targetSite: null);

            _testHelper.ProvidedDeltaApplicationVisitor = visitor;
            var node = _testHelper.MakeConfiguredVersionControlNodeWithChildren();
            var targetSite = new FlatPropertyBag().PropertyInfoFor(x => x.StringProperty).GetSetMethod();
            node.Mutations.AddRange(new TimestampedPropertyVersionDelta("1", targetSite, 1L, isActive:false),
                                    new TimestampedPropertyVersionDelta("2", targetSite, 2L, isActive:false));

            //act
            TestDelegate act = () => node.Accept(visitor);

            //assert
            Assert.Throws<VersionDeltaNotFoundException>(act);
        }

        [Test]
        public void when_undoing_a_specific_assignment()
        {
            
        }
        
    }
}