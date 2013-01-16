using System.Collections.Generic;
using System.Reflection;
using FakeItEasy;
using VersionCommander.Implementation;
using VersionCommander.Implementation.Visitors;

namespace VersionCommander.UnitTests.TestingAssists
{
    public class FakeVersionControlNodeBase : IVersionControlNode
    {
        public FakeVersionControlNodeBase()
        {
            Mutations = new List<TimestampedPropertyVersionDelta>();
            Children = new List<IVersionControlNode>();

        }

        public void RollbackTo(long targetVersion)
        {
            return;
        }

        public object CurrentDepthCopy()
        {
            return A.Fake<object>();
        }

        public IList<TimestampedPropertyVersionDelta> Mutations { get; private set; }
        public IList<IVersionControlNode> Children { get; private set; }
        public IVersionControlNode Parent { get; set; }
        public void Accept(IVersionControlTreeVisitor visitor)
        {
            VisitorBehavior.Accept(this, visitor);
        }

        public void RecursiveAccept(IVersionControlTreeVisitor visitor)
        {
            VisitorBehavior.RecursiveAccept(this, visitor);
        }

        public object Get(PropertyInfo targetProperty, long version)
        {
            return A.Fake<object>();
        }

        public void Set(PropertyInfo targetProperty, object value, long version)
        {
            return;
        }
    }
}