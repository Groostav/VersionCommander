<<<<<<< HEAD
﻿using System.Collections.Generic;
using System.Linq;

namespace VersionCommander.Implementation.Visitors
{
    public class DescendentAggregatorVisitor : VersionControlTreeVisitorBase
    {
        [ThereBeDragons("This Code's complexity warrents propery DI, but yet its still static.")]
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

        public override void OnEntry(IVersionControlNode controlNode)
        {
            _descendents.AddRange(controlNode.Children);
        }
    }
=======
﻿using System.Collections.Generic;
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
>>>>>>> f8d34094a494492933f5dc19bf749c84b70c5bac
}