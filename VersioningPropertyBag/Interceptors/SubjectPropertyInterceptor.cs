using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Castle.DynamicProxy;
using VersionCommander.Extensions;

namespace VersionCommander
{
    internal class SubjectPropertyInterceptor<TSubject> : IInterceptor 
    {
        private readonly IVersionControlNode _controller;
        private readonly IEnumerable<PropertyInfo> _members;

        public SubjectPropertyInterceptor(IVersionControlNode controller)
        {
            _controller = controller;
            _members = typeof (TSubject).GetProperties();
        }

        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method.DeclaringType != typeof(TSubject))
            {
                invocation.Proceed();
                return;
            }

            if(invocation.Method.IsPropertyGetter())
            {
                var member = _members.Single(candidate => candidate.GetGetMethod() == invocation.Method);
                invocation.ReturnValue = _controller.Get(member);
            }
            else if (invocation.Method.IsPropertySetter())
            {
                var member = _members.Single(candidate => candidate.GetSetMethod() == invocation.Method);
                _controller.Set(member, invocation.Arguments.Single());
            }
            return;
        }
    }
}