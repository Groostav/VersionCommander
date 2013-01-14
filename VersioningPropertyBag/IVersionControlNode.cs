using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics.Contracts;
using VersionCommander.Implementation.Visitors;

namespace VersionCommander.Implementation
{
    [ContractClass(typeof(VersionControlNodeContracts))]
    public interface IVersionControlNode
    {
        void RollbackTo(long targetVersion);
        object CurrentDepthCopy();

        [ThereBeDragons("I cant remember how important it is that this thing is ordered. If it is, I have no assertions on that.")]
        IList<TimestampedPropertyVersionDelta> Mutations { get; }

        IList<IVersionControlNode> Children { get; }
        [ThereBeDragons("multiple parents... that turns this into a full bore graph... Also unused, remove?")]
        IVersionControlNode Parent { get; set; }

        void Accept(IVersionControlTreeVisitor visitor);
        void RecursiveAccept(IVersionControlTreeVisitor visitor);

        object Get(PropertyInfo targetProperty, long version);
        void Set(PropertyInfo targetProperty, object value, long version);
    }

    [ContractClassFor(typeof(IVersionControlNode))]
    public abstract class VersionControlNodeContracts : IVersionControlNode
    {
        void IVersionControlNode.RollbackTo(long targetVersion)
        {
        }

        [Pure]
        object IVersionControlNode.CurrentDepthCopy()
        {
            //ensures all value type properties are in new memory. 
            return default(IVersionControlNode);
        }

        IList<IVersionControlNode> IVersionControlNode.Children
        {
            [Pure]
            get 
            { 
                Contract.Ensures(Contract.Result<IList<IVersionControlNode>>().Any());
                return default(IList<IVersionControlNode>);
            }
        }

        IVersionControlNode IVersionControlNode.Parent
        {
            [Pure]
            get
            {
                return default(IVersionControlNode);
            }
            set
            {
            }
        }

        void IVersionControlNode.Accept(IVersionControlTreeVisitor visitor)
        {
            Contract.Requires(visitor != null);
            //Contract.Ensures(visitor is run on all children).
        }

        void IVersionControlNode.RecursiveAccept(IVersionControlTreeVisitor visitor)
        {
            Contract.Requires(visitor != null);
        }

        [Pure]
        IList<TimestampedPropertyVersionDelta> IVersionControlNode.Mutations
        {
            get
            {
                Contract.Ensures(Contract.Result<IList<TimestampedPropertyVersionDelta>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<TimestampedPropertyVersionDelta>>(), mutation => mutation != null));
                return default(IList<TimestampedPropertyVersionDelta>);
            }
        }

        [Pure]
        object IVersionControlNode.Get(PropertyInfo targetProperty, long version)
        {
            Contract.Requires(targetProperty != null);
            Contract.Requires(this.GetType().GetInterface(typeof(IVersionController<>).FullName).GetGenericArguments().Single().GetProperties().Contains(targetProperty));
            return default(object);
        }

        void IVersionControlNode.Set(PropertyInfo targetProperty, object value, long version)
        {
            Contract.Requires(targetProperty != null);
            Contract.Requires(this.GetType().GetInterface(typeof(IVersionController<>).FullName).GetGenericArguments().Single().GetProperties().Contains(targetProperty));
        }
    }
}
