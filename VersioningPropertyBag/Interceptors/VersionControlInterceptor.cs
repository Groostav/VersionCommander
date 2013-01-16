using System.Linq;
using Castle.DynamicProxy;
using VersionCommander.Implementation.Extensions;

namespace VersionCommander.Implementation.Interceptors
{
    public class VersionControlInterceptor<TSubject> : IInterceptor where TSubject : class
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
                _controller.Children.Remove(previousValue.AsVersionControlNode());
            }
            if (IsSettingNewVersionableObjectOnSubject(invocation))
            {
                var newRepo = invocation.Arguments.Single().AsVersionControlNode();
                _controller.Children.Add(newRepo);
                newRepo.Parent = invocation.Proxy.AsVersionControlNode();

                //still actually need to 'set' the object on the parent, leave that to SubjectPropertyInterceptor
                invocation.Proceed(); 
            }
            
            if (IsRelaventToThisVersionControl(invocation))
            {
                if (IsAskingForNativeObject(invocation))
                {
                    invocation.ReturnValue = (TSubject) invocation.Proxy;
                }
                else
                {
                    invocation.ReturnValue = _controller;
                }
            }
            else
            {
                invocation.Proceed();
            }
            return;
        }

        private bool IsAskingForNativeObject(IInvocation invocation)
        {
            return invocation.Method == MethodInfoExtensions.GetMethodInfo<IVersionControlledObject, TSubject>(
                                        x => x.AsNativeObject<TSubject>());
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

        private bool IsRelaventToThisVersionControl(IInvocation invocation)
        {
            return invocation.Method.DeclaringType == typeof (IVersionControlledObject);
        }

        private static bool IsSettingNewVersionableObjectOnSubject(IInvocation invocation)
        {
            return invocation.Method.IsPropertySetter() 
                   && invocation.Arguments.Single() != null
                   && invocation.Arguments.Single().GetType().CanInterfaceAs(typeof(IVersionControlledObject))
                   && invocation.TargetType == typeof(TSubject);
        }
    }
}