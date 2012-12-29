using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using VersionCommander.Implementation.Extensions;

namespace VersionCommander.Implementation
{
    public class VersionControlInterceptor<TSubject> : IInterceptor 
    {
        private readonly IVersionControlNode _controller;

        public VersionControlInterceptor(IVersionControlNode controller)
        {
            _controller = controller;
        }

        public void Intercept(IInvocation invocation)
        {
            TSubject previousValue;
            if (IsSettingPreviouslyVersioningChild(invocation, out previousValue))
            {
                _controller.Children.Remove(previousValue.VersionControlNode());
            }
            if (IsSettingNewVersionableObjectOnSubject(invocation))
            {
                var newRepo = invocation.Arguments.Single().VersionControlNode();
                _controller.Children.Add(newRepo);
                newRepo.Parent = invocation.Proxy.VersionControlNode();

                //still actually need to 'set' the object on the parent, leave that to SubjectPropertyInterceptor
                invocation.Proceed(); 
            }
            
            if (IsRelaventToVersionControl(invocation))
            {
                invocation.ReturnValue = _controller;
            }
            else
            {
                invocation.Proceed();
            }
            return;
        }

        private bool IsSettingPreviouslyVersioningChild(IInvocation invocation, out TSubject childSubject)
        {
            if ( ! (invocation.Method.IsPropertySetter() && invocation.Arguments.Single() is IVersionControlNode))
            {
                childSubject = default(TSubject);
                return false;
            }

            childSubject = (TSubject)invocation.Method.GetParentProperty().GetGetMethod().Invoke(invocation.Proxy, new object[0]);

            return childSubject != null
                && childSubject.GetType().CanInterfaceAs(typeof (IVersionController<>));
        }

        private bool IsRelaventToVersionControl(IInvocation invocation)
        {
            return invocation.Method.DeclaringType == typeof (IVersionControlProvider);
        }

        private static bool IsSettingNewVersionableObjectOnSubject(IInvocation invocation)
        {
            return invocation.Method.IsPropertySetter() 
                   && invocation.Arguments.Single() != null
                   && invocation.Arguments.Single().GetType().CanInterfaceAs(typeof(IVersionControlProvider))
                   && invocation.TargetType == typeof(TSubject);
        }
    }
}