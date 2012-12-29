using System;
using System.Linq;
using System.Reflection;
using VersionCommander.Implementation.Exceptions;
using VersionCommander.Implementation.Extensions;

namespace VersionCommander.Implementation.Visitors
{
    public class DeltaApplicationVisitor : IVersionControlTreeVisitor
    {
        private readonly bool _includeDescendents;
        private readonly bool _newStatus;
        private readonly Func<TimestampedPropertyVersionDelta, bool> _targetSiteContstraint;

        public DeltaApplicationVisitor(bool includeDescendents, bool setActive, MethodInfo targetSite = null)
        {
            _includeDescendents = includeDescendents;
            _newStatus = setActive;

            if (targetSite == null)
            {
                _targetSiteContstraint = mutation => true;
            }
            else
            {
                _targetSiteContstraint = mutation => mutation.TargetSite == targetSite;
            }
        }

        public void RunOn(IVersionControlNode controlNode)
        {
            var targetMutationSet = _includeDescendents
                                        ? DescendentAggregatorVisitor.GetDescendentMutationsOf(controlNode)
                                        : controlNode.Mutations;

            var targetMutation = targetMutationSet.Where(mutation => mutation.IsActive != _newStatus)
                                                  .Where(_targetSiteContstraint)
                                                  .WithMax(mutation => mutation.TimeStamp);

            if (!targetMutation.IsSingle()) throw new VersionClockResolutionException();

            targetMutation.Single().IsActive = _newStatus;
        }
    }
}