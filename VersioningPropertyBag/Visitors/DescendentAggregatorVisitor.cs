using System.Collections.Generic;
using System.Linq;

namespace VersionCommander.Implementation.Visitors
{
    public class DescendentAggregatorVisitor : IVersionControlTreeVisitor
    {
        public static IEnumerable<IVersionControlNode> GetDescendentsOf(IVersionControlNode targetNode)
        {
            var visitor = new DescendentAggregatorVisitor();
            targetNode.Accept(visitor);
            return visitor.Descendents;
        }

        public static IEnumerable<TimestampedPropertyVersionDelta> GetDescendentMutationsOf(IVersionControlNode targetNode)
        {
            return GetDescendentsOf(targetNode).SelectMany(node => node.Mutations);
        } 
        
        private readonly List<IVersionControlNode> _descendents;

        public DescendentAggregatorVisitor()
        {
            _descendents = new List<IVersionControlNode>();
        }

        public IEnumerable<IVersionControlNode> Descendents
        {
            get { return _descendents; }
        }

        public void RunOn(IVersionControlNode controlNode)
        {
            _descendents.AddRange(controlNode.Children);
        }
    }
}