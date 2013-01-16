using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VersionCommander.Implementation.Cloners;
using VersionCommander.Implementation.Extensions;

namespace VersionCommander.Implementation.Visitors
{
    public class FindAndCopyVersioningChildVisitor : VersionControlTreeVisitorBase
    {
        //it really boils my charcholes that I cant make this thing copy the root level:
            // A) it would force this object to be statically typed to TSubject, else it cant use the Proxyfactory's MakeVersioning()
            // B) I have to inject a proxy factory, a flone factory, and the object content (else I cant use ProxyFactory's MakeVersioning()
            // C) the visitor has no concept of a return type, a fundimentally since I'm trying to clone a tree I need to return the tree's clone. 
            // D) it means another custom entry in IVisitorFactory (because this visitor will have to take constructor args)

        public override void OnEntry(IVersionControlNode controlNode)
        {
            Debug.Assert(controlNode.Mutations.IsOrderedBy(mutation => mutation.TimeStamp));

            var candidatesByIndex = GetCadidatesByIndex(controlNode);

            foreach (var indexCandidatePair in candidatesByIndex)
            {
                var versioningChild = indexCandidatePair.Value.Arguments.Single();
                var versioningPropertyValueClone = versioningChild.AsVersionControlNode().CurrentDepthCopy();

                controlNode.Mutations[indexCandidatePair.Key] = new TimestampedPropertyVersionDelta(indexCandidatePair.Value, versioningPropertyValueClone); 
            }

            candidatesByIndex = GetCadidatesByIndex(controlNode);

            var children = candidatesByIndex.GroupBy(mutation => mutation.Value.TargetSite)
                                            .Select(group => group.Last())
                                            .Select(mutation => mutation.Value.Arguments.Single().AsVersionControlNode());

            controlNode.Children.Clear();
            controlNode.Children.AddRange(children);
        }

        private IEnumerable<KeyValuePair<int, TimestampedPropertyVersionDelta>> GetCadidatesByIndex(IVersionControlNode node)
        {
            return from index in Enumerable.Range(0, node.Mutations.Count)
                   where node.Mutations[index].IsSettingVersioningObject
                   select new KeyValuePair<int, TimestampedPropertyVersionDelta>(index, node.Mutations[index]);
        }
    }
}