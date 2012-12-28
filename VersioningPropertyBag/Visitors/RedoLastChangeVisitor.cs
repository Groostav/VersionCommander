using System;
using System.Linq;
using VersionCommander.Implementation.Extensions;

namespace VersionCommander.Implementation.Visitors
{
    public class RedoLastChangeVisitor : IPropertyTreeVisitor
    {
        private readonly bool _includeDescendents;
        private readonly Func<object, bool> _additionalPredicate;

        public RedoLastChangeVisitor(bool includeDescendents, Func<object, bool> additionalPredicate)
        {
            _includeDescendents = includeDescendents;
            _additionalPredicate = additionalPredicate;
        }

        public void RunOn(IVersionControlNode controlNode)
        {
            var candidates = _includeDescendents
                                 ? DescendentAggregatorVisitor.GetDescendentMutationsOf(controlNode)
                                 : controlNode.Mutations;

            candidates = candidates.Where(item => _additionalPredicate(item));

            var targetMutation = candidates
                                 .Where(mutation => ! mutation.IsActive)
                                 .WithMax(mutation => mutation.TimeStamp);

            targetMutation.Single().IsActive = true;
        }
    }
}