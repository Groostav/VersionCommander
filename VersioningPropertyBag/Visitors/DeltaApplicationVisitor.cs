using System;
using System.Linq;
using System.Reflection;
using VersionCommander.Implementation.Exceptions;
using VersionCommander.Implementation.Extensions;

namespace VersionCommander.Implementation.Visitors
{
    public class DeltaApplicationVisitor : DescendentAggregatorVisitor
    {
        private readonly bool _visitAllNodes;
        private readonly bool _newStatus;
        private readonly Func<TimestampedPropertyVersionDelta, bool> _targetSiteContstraint;

        public DeltaApplicationVisitor(ChangeType changeType, MethodInfo targetSite, bool searchWholeTree)
        {
            _visitAllNodes = searchWholeTree;
            _newStatus = changeType == ChangeType.Redo;

            if (targetSite == null)
            {
                _targetSiteContstraint = mutation => true;
            }
            else
            {
                _targetSiteContstraint = mutation => mutation.TargetSite == targetSite;
            }
        }

        public override void OnLastExit()
        {         
            var targetMutation = Mutations.Where(mutation => mutation.IsActive != _newStatus)
                                          .Where(_targetSiteContstraint)
                                          .WithMax(mutation => mutation.TimeStamp);

            if ( ! targetMutation.Any()) throw new VersionDeltaNotFoundException();
            if ( ! targetMutation.IsSingle()) throw new VersionClockResolutionException();

            targetMutation.Single().IsActive = _newStatus;
        }

        public override bool VisitAllNodes
        {
            get { return _visitAllNodes; }
        }
    }
}