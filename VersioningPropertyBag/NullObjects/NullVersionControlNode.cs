<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Reflection;
using VersionCommander.Implementation.Visitors;

namespace VersionCommander.Implementation.NullObjects
{
    public class NullVersionControlNode : IVersionControlNode
    {
        private readonly IList<IVersionControlNode> _children;
        private readonly IList<TimestampedPropertyVersionDelta> _mutations;

        public NullVersionControlNode()
        {
            _children = new IVersionControlNode[0];
            _mutations = new TimestampedPropertyVersionDelta[0];
        }

        public void RollbackTo(long targetVersion)
        {
            throw new NotImplementedException();
        }

        public object CurrentDepthCopy()
        {
            throw new NotImplementedException();
        }

        public IList<IVersionControlNode> Children
        {
            get { return _children; }
            set { throw new NullReferenceException("attempted to set 'children' of null object"); }
        }

        public IVersionControlNode Parent
        {
            get { return null; }
            set { throw new NullReferenceException("attempted to set 'parent' of null object"); }
        }

        public void Accept(IVersionControlTreeVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public void LevelAccept(IVersionControlTreeVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public IList<TimestampedPropertyVersionDelta> Mutations
        {
            get { return _mutations; }
        }

        public object Get(PropertyInfo targetProperty, long version)
        {
            throw new NotImplementedException();
        }

        public void Set(PropertyInfo targetProperty, object value, long version)
        {
            throw new NotImplementedException();
        }
    }
=======
﻿using System;
using System.Collections.Generic;
using System.Reflection;
using VersionCommander.Implementation.Visitors;

namespace VersionCommander.Implementation.NullObjects
{
    public class NullVersionControlNode : IVersionControlNode
    {
        public NullVersionControlNode()
        {
            Mutations = new TimestampedPropertyVersionDelta[0];
        }

        public void RollbackTo(long targetVersion)
        {
            throw new NotImplementedException();
        }

        public object CurrentDepthCopy()
        {
            throw new NotImplementedException();
        }

        public IList<IVersionControlNode> Children { get; set; }
        public IVersionControlNode Parent { get; set; }
        public void Accept(IVersionControlTreeVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public void RecursiveAccept(IVersionControlTreeVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public void LevelAccept(IVersionControlTreeVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public IList<TimestampedPropertyVersionDelta> Mutations { get; private set; }
        public object Get(PropertyInfo targetProperty, long version)
        {
            throw new NotImplementedException();
        }

        public void Set(PropertyInfo targetProperty, object value, long version)
        {
            throw new NotImplementedException();
        }
    }
>>>>>>> f8d34094a494492933f5dc19bf749c84b70c5bac
}