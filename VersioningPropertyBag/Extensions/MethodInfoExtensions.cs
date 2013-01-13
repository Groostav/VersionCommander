using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace VersionCommander.Implementation.Extensions
{
    public static class MethodInfoExtensions
    {
        //uhhh theres somethin funky goin on here.
        public static void RefocusDynamicInvocationExceptions(Action invocation)
        {
            try
            {
                invocation.Invoke();
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException;
            }
        }

        public static object GetDefaultValue(this Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            else
            {
                return null;
            }
        }

        [Pure]
        public static PropertyInfo PropertyInfoFor<TSubject, TResult>(this TSubject subject,
                                                                      Expression<Func<TSubject, TResult>> propertyPointer)
        {
            return GetPropertyInfo(propertyPointer);
        }

        [Pure]
        public static MethodInfo MethodInfoFor<TSubject, TResult>(this TSubject subject, 
                                                                  Expression<Func<TSubject, TResult>> targetSite)
        {
            return GetMethodInfo(targetSite);
        }
        
        [Pure]
        public static bool IsPropertySetter(this MethodInfo method)
        {
            Contract.Requires(method != null && method.DeclaringType != null && method.DeclaringType.GetProperties() != null);
            Contract.Ensures(method.GetParentProperty() != null || Contract.Result<bool>() == false);

            return method.DeclaringType.GetProperties().Any(prop => prop.GetSetMethod() == method);
        }

        [Pure]
        public static bool IsPropertyGetter(this MethodInfo method)
        {
            Contract.Requires(method != null && method.DeclaringType != null);
            Contract.Ensures(method.GetParentProperty() != null || Contract.Result<bool>() == false);

            return method.DeclaringType.GetProperties().Any(prop => prop.GetGetMethod() == method);
        }


        [Pure]
        public static PropertyInfo GetParentProperty(this MethodInfo method)
        {
            if (method == null) throw new ArgumentNullException("method");
            if (method.DeclaringType == null) throw new TypeAccessException("cannot determine methods declaring type");

            var takesArg = method.GetParameters().Length == 1;
            var hasReturn = method.ReturnType != typeof (void);
            if (!(takesArg || hasReturn)) return null;

            if (takesArg && !hasReturn)
            {
                return method.DeclaringType.GetProperties().FirstOrDefault(prop => prop.GetSetMethod() == method);
            }
            else
            {
                return method.DeclaringType.GetProperties().FirstOrDefault(prop => prop.GetGetMethod() == method);
            }
        }

        /// <summary>
        /// Given a lambda expression that calls a method, returns the method info.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        [Pure]
        public static MethodInfo GetMethodInfo(Expression<Action> expression)
        {
            return GetMethodInfo((LambdaExpression)expression);
        }

        /// <summary>
        /// Given a lambda expression that calls a method, returns the method info.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        [Pure]
        public static MethodInfo GetMethodInfo<T>(Expression<Action<T>> expression)
        {
            return GetMethodInfo((LambdaExpression)expression);
        }

        /// <summary>
        /// Given a lambda expression that calls a method, returns the method info.
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        [Pure]
        public static MethodInfo GetMethodInfo<TSubject, TResult>(Expression<Func<TSubject, TResult>> expression)
        {
            return GetMethodInfo((LambdaExpression)expression);
        }

        /// <summary>
        /// Given a lambda expression that calls a method, returns the method info.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        /// 
        public static MethodInfo GetMethodInfo(LambdaExpression expression)
        {
            var outermostExpression = expression.Body as MethodCallExpression;

            if (outermostExpression == null)
            {
                throw new ArgumentException("Invalid Expression. Expression should consist of a Method call only.");
            }

            return outermostExpression.Method;
        }

        public static PropertyInfo GetPropertyInfo<TProperty>(Expression<Func<TProperty>> propertyLinq)
        {
            return GetPropertyInfo(propertyLinq as LambdaExpression);
        }
        public static PropertyInfo GetPropertyInfo<TParent, TProperty>(Expression<Func<TParent, TProperty>> propertyLinq)
        {
            return GetPropertyInfo(propertyLinq as LambdaExpression);
        }

        private static PropertyInfo GetPropertyInfo(LambdaExpression propertyLinq)
        {
            //in the case of an indexer, we hit this method.
            var call = propertyLinq.Body as MethodCallExpression;
            if (call != null)
            {
                var parentProperty = call.Method.GetParentProperty();
                if (parentProperty != null)
                {
                    return parentProperty;
                }

                throw new ArgumentException(string.Format("Expression is a method call, not a property expression."));
            }

            var member = propertyLinq.Body as MemberExpression;
            if (member == null)
            {
                throw new ArgumentException(string.Format("Expression is not a member expression (it is a {0} expression). " +
                                                          "Expression should consist of a Property-getter call only.",
                                                          propertyLinq.Body.NodeType));
            }
            var property = member.Member as PropertyInfo;
            if (property == null)
            {
                throw new ArgumentException(string.Format("Expression is not a property-member expression (it is a member-{0} expression). " +
                                                          "Expression should consist of a Property-getter call only.",
                                                          member.NodeType));
            }

            return property;
        }
    }
}