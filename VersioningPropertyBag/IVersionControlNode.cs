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
        IVersionControlNode CurrentDepthCopy();

        IList<IVersionControlNode> Children { get; set; }
        IVersionControlNode Parent { get; set; }

        void Accept(IPropertyTreeVisitor visitor);

        IList<TimestampedPropertyVersionDelta> Mutations { get; }

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
        IVersionControlNode IVersionControlNode.CurrentDepthCopy()
        {
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
            set
            {
                Contract.Requires(value != null);
                Contract.Requires( ! value.IsReadOnly);
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
                throw new NotImplementedException();
            }
        }

        void IVersionControlNode.Accept(IPropertyTreeVisitor visitor)
        {
            throw new NotImplementedException();
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