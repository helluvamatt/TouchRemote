using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TouchRemote.Utils
{
    internal static class InvocationUtils
    {
        public static void Invoke(this DependencyObject obj, Action action)
        {
            if (!obj.Dispatcher.HasShutdownStarted)
            {
                obj.Dispatcher.Invoke(action);
            }
        }

        public static T Invoke<T>(this DependencyObject obj, Func<T> action)
        {
            if (!obj.Dispatcher.HasShutdownStarted)
            {
                return obj.Dispatcher.Invoke(action);
            }
            return default(T);
        }
    }
}
