using System.Linq;
using VersionCommander.Implementation.Extensions;

namespace VersionCommander.Implementation.Visitors
{
    public class UndoLastChangeVisitor : IPropertyTreeVisitor
    {
        public void RunOn(IVersionControlNode controlNode)
        {
            var targetMutation = DescendentAggregatorVisitor.GetDescendentMutationsOf(controlNode)
                                                            .Where(mutation => mutation.IsActive)
                                                            .WithMax(mutation => mutation.TimeStamp);

            targetMutation.Single().IsActive = false;
        }
    }
}