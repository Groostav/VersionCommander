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

            bool takesArg = method.GetParameters().Length == 1;
            bool hasReturn = method.ReturnType != typeof(void);
            if ( ! (hasReturn ^ takesArg)) return null;

            if (takesArg)
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
        public static MethodInfo GetMethodInfo<T>(Expression<Action<T>> expression)
        {
            return GetMethodInfo((LambdaExpression)expression);
        }

        /// <summary>
        /// Given a lambda expression that calls a method, returns the method info.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static MethodInfo GetMethodInfo<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            return GetMethodInfo((LambdaExpression)expression);
        }

        /// <summary>
        /// Given a lambda expression that calls a method, returns the method info.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
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
            var expression = propertyLinq.Body as MemberExpression;
            if (expression == null)
            {
                throw new ArgumentException("Invalid Expression. Expression should consist of a Property-chain only.");
            }
            return expression.Member as PropertyInfo; //this will grab the most-rightward property.
        }
    }
}