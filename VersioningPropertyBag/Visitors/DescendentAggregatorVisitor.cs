using System.Collections.Generic;
using System.Linq;

namespace VersionCommander.Implementation.Visitors
{
    public class DescendentAggregatorVisitor : VersionControlTreeVisitorBase
    {
        private readonly List<IVersionControlNode> _descendents;
        private bool _hasRunOnce;

        public DescendentAggregatorVisitor()
        {
            _descendents = new List<IVersionControlNode>();
            _hasRunOnce = false;
        }

        public IEnumerable<IVersionControlNode> Descendents
        {
            get { return _descendents; }
        }
        
        public IEnumerable<TimestampedPropertyVersionDelta> Mutations
        {
            get { return Descendents.SelectMany(descendent => descendent.Mutations); }
        }

        public override void OnEntry(IVersionControlNode controlNode)
        {
            if (! VisitAllNodes && _hasRunOnce)
            {
                return;
            }

            _hasRunOnce = true;
            _descendents.Add(controlNode);
        }
    }
}