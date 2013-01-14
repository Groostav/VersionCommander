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
            //thanks to use of enumerables, this actually consumes very little memory, meaning I can safely handle fairly large graphs
            //outside of image/audio processing though I suspect its rare to have multi-meg object trees.

            foreach (var indexCandidatePair in candidatesByIndex)
            {
                var versioningChild = indexCandidatePair.Value.Arguments.Single();
                var versioningPropertyValueClone = versioningChild.VersionControlNode().CurrentDepthCopy();
                var versioningPropertyControlNode = versioningPropertyValueClone.VersionControlNode();

                //this node has new memory, but its still referencing the original children.
                versioningPropertyControlNode.Accept(new FindAndCopyVersioningChildVisitor()); //Update those references.

                //given that the TimestampedPropertyVersionDelta is mutable, this line is the only reason Mutations needs to be a mutable list.
                controlNode.Mutations[indexCandidatePair.Key] = new TimestampedPropertyVersionDelta(indexCandidatePair.Value, versioningPropertyValueClone);
            }

            //refresh candidates, since all mutations are now different and in new memory
            candidatesByIndex = GetCadidatesByIndex(controlNode);

            var children = candidatesByIndex.GroupBy(mutation => mutation.Value.TargetSite)
                                            .Select(group => group.Last())
                                            .Select(mutation => mutation.Value.Arguments.Single().VersionControlNode());

            controlNode.Children.Clear(); 
            controlNode.Children.AddRange(children);
        }

        private IEnumerable<KeyValuePair<int, TimestampedPropertyVersionDelta>> GetCadidatesByIndex(IVersionControlNode node)
        {
            //So the profiler thinks this is a problem, but I cant figure out how it could be...
                //im hitting castle's pre_nub or whatever it is, so somethings being intercepted but I cant figure out what.
            return from index in Enumerable.Range(0, node.Mutations.Count)
                   where node.Mutations[index].IsSettingVersioningObject()
                   select new KeyValuePair<int, TimestampedPropertyVersionDelta>(index, node.Mutations[index]);
        }
    }
}