﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VersionCommander.Implementation.Extensions;

namespace VersionCommander.Implementation.Visitors
{
    public class FindAndCopyVersioningChildVisitor : IPropertyTreeVisitor
    {
        public void RunOn(IVersionControlNode controlNode)
        {
            Debug.Assert(controlNode.Mutations.IsOrderedBy(mutation => mutation.TimeStamp));

            //command object vs delegate strikes: I used Mutations.Linq... which referenced this, which got nicely closed in by C# and i recursed infinitely.
            //moral: command objects give you a little more type safetly.

            var candidatesByIndex = GetCadidatesByIndex(controlNode);
            //thanks to use of enumerables, this actually consumes very little memory, meaning I can safely handle fairly large graphs
            //outside of image/audio processing though I suspect its rare to have multi-meg object trees.

            foreach (var indexCandidatePair in candidatesByIndex)
            {
                var versioningChild = indexCandidatePair.Value.Arguments.Single();
                var cloneVersionNode = versioningChild.VersionControlNode().CurrentDepthCopy();

                //this node has new memory, but its still referencing the original children.
                cloneVersionNode.Accept(new FindAndCopyVersioningChildVisitor()); //Update those references.

                controlNode.Mutations[indexCandidatePair.Key] = new TimestampedPropertyVersionDelta(indexCandidatePair.Value, cloneVersionNode);
            }

            //refresh candidates, since all mutations are now different and in new memory
            candidatesByIndex = GetCadidatesByIndex(controlNode);

            var children = candidatesByIndex.GroupBy(mutation => mutation.Value.TargetSite)
                                            .Select(group => group.Last())
                                            .Select(mutation => mutation.Value.Arguments.Single().VersionControlNode());

            controlNode.Children = children.ToList();
        }

        private IEnumerable<KeyValuePair<int, TimestampedPropertyVersionDelta>> GetCadidatesByIndex(IVersionControlNode node)
        {
            return from index in Enumerable.Range(0, node.Mutations.Count)
                   where node.Mutations[index].IsSettingVersioningObject()
                   select new KeyValuePair<int, TimestampedPropertyVersionDelta>(index, node.Mutations[index]);
        }
    }
}