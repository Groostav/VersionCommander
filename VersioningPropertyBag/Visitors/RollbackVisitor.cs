using System.Linq;

namespace VersionCommander.Implementation.Visitors
{
    public class RollbackVisitor : VersionControlTreeVisitorBase
    {
        private readonly long _targetTime;

        public RollbackVisitor(long targetTime)
        {
            _targetTime = targetTime;
        }

        public override void OnEntry(IVersionControlNode controlNode)
        {
            var relatedMutations = controlNode.Mutations.Where(mutation => mutation.TimeStamp > _targetTime).ToArray();
            foreach (var mutation in relatedMutations)
            {
                controlNode.Mutations.Remove(mutation);
            }
        }
    }
}