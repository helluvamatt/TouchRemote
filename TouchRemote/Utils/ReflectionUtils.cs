using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TouchRemote.Utils
{
    internal static class ReflectionUtils
    {
        public static TValue GetAttributeValue<TAttribute, TValue>(this Type type, Func<TAttribute, TValue> valueSelector) where TAttribute : Attribute
        {
            var att = type.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;
            if (att != null)
            {
                return valueSelector(att);
            }
            return default(TValue);
        }

        public static string GetAssemblyDir(this Assembly assembly)
        {
            string codeBase = assembly.CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        public static void Notify<T>(this PropertyChangedEventHandler handler, Expression<Func<T>> memberExpression)
        {
            if (memberExpression == null)
            {
                throw new ArgumentNullException("memberExpression");
            }
            var body = memberExpression.Body as MemberExpression;
            if (body == null)
            {
                throw new ArgumentException("Lambda must return a property.");
            }
            NotifyReal(handler, body);
        }

        public static bool ChangeAndNotify<T>(this PropertyChangedEventHandler handler, ref T field, T value, Expression<Func<T>> memberExpression, Action<T, T> changedCallback = null)
        {
            if (memberExpression == null)
            {
                throw new ArgumentNullException("memberExpression");
            }
            var body = memberExpression.Body as MemberExpression;
            if (body == null)
            {
                throw new ArgumentException("Lambda must return a property.");
            }

            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            T oldValue = field;
            field = value;

            NotifyReal(handler, body);

            changedCallback?.Invoke(oldValue, field);

            return true;
        }

        public static bool ChangeAndNotifyDependency<T>(this PropertyChangedEventHandler handler, ref T field, T value, Expression<Func<T>> memberExpression, PropertyChangedEventHandler parentHandler, Action<T, T> changedCallback = null) where T : INotifyPropertyChanged
        {
            return ChangeAndNotify(handler, ref field, value, memberExpression, (oldValue, newValue) => {
                if (oldValue != null) oldValue.PropertyChanged -= parentHandler;
                if (newValue != null) newValue.PropertyChanged += parentHandler;
                changedCallback?.Invoke(oldValue, newValue);
            });
        }

        private static void NotifyReal(PropertyChangedEventHandler handler, MemberExpression body)
        {
            var vmExpression = body.Expression as ConstantExpression;
            if (vmExpression != null)
            {
                LambdaExpression lambda = Expression.Lambda(vmExpression);
                Delegate vmFunc = lambda.Compile();
                object sender = vmFunc.DynamicInvoke();
                handler?.Invoke(sender, new PropertyChangedEventArgs(body.Member.Name));
            }
        }
    }
}
