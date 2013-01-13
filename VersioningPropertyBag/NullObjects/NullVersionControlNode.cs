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
}