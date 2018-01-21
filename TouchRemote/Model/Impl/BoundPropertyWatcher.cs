using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TouchRemote.Lib;
using TouchRemote.Model.Persistence.Controls;

namespace TouchRemote.Model.Impl
{
    internal class BoundPropertyWatcher : IDisposable
    {
        private const int DELAY = 100; // millis = 10 hz

        private ILog _Log;

        private List<WatcherContext<FloatBoundProperty, float>> _FloatContexts;
        private List<WatcherContext<BooleanBoundProperty, bool>> _BooleanContexts;
        private List<WatcherContext<StringBoundProperty, string>> _StringContexts;
        private CancellationTokenSource _CancelTokenSource;
        private ReaderWriterLockSlim _Lock;

        public event BoundPropertyErrorHandler BoundPropertyError;

        public BoundPropertyWatcher()
        {
            _Log = LogManager.GetLogger(GetType());
            _FloatContexts = new List<WatcherContext<FloatBoundProperty, float>>();
            _BooleanContexts = new List<WatcherContext<BooleanBoundProperty, bool>>();
            _StringContexts = new List<WatcherContext<StringBoundProperty, string>>();
            _Lock = new ReaderWriterLockSlim();
            _CancelTokenSource = new CancellationTokenSource();
        }

        public void UpdateBoundProperties(IEnumerable<RemoteElement> elements)
        {
            _Lock.EnterWriteLock();
            try
            {
                _FloatContexts.Clear();
                _BooleanContexts.Clear();
                _StringContexts.Clear();
                foreach (var element in elements)
                {
                    AddBoundProperties(element);
                }
            }
            finally
            {
                _Lock.ExitWriteLock();
            }
        }

        public void UpdateBoundProperties(RemoteElement element)
        {
            _Lock.EnterWriteLock();
            try
            {
                for (int i = 0; i < _FloatContexts.Count; i++)
                {
                    var current = _FloatContexts[i];
                    if (current.Owner.Id == element.Id)
                    {
                        _FloatContexts.RemoveAt(i);
                        current.Dispose();
                    }
                }
                for (int i = 0; i < _BooleanContexts.Count; i++)
                {
                    var current = _BooleanContexts[i];
                    if (current.Owner.Id == element.Id)
                    {
                        _BooleanContexts.RemoveAt(i);
                        current.Dispose();
                    }
                }
                for (int i = 0; i < _StringContexts.Count; i++)
                {
                    var current = _StringContexts[i];
                    if (current.Owner.Id == element.Id)
                    {
                        _StringContexts.RemoveAt(i);
                        current.Dispose();
                    }
                }
                AddBoundProperties(element);
            }
            finally
            {
                _Lock.ExitWriteLock();
            }
        }

        private void AddBoundProperties(RemoteElement element)
        {
            foreach (string propName in element.BoundPropertyNames)
            {
                var fbp = element.GetFloatBoundProperty(propName);
                if (fbp != null)
                {
                    _FloatContexts.Add(new WatcherContext<FloatBoundProperty, float>(element, propName, fbp, HandleFloatBoundPropertyChanged, HandleBoundPropertyError));
                }
                var bbp = element.GetBooleanBoundProperty(propName);
                if (bbp != null)
                {
                    _BooleanContexts.Add(new WatcherContext<BooleanBoundProperty, bool>(element, propName, bbp, HandleBooleanBoundPropertyChanged, HandleBoundPropertyError));
                }
                var sbp = element.GetStringBoundProperty(propName);
                if (sbp != null)
                {
                    _StringContexts.Add(new WatcherContext<StringBoundProperty, string>(element, propName, sbp, HandleStringBoundPropertyChanged, HandleBoundPropertyError));
                }
            }
        }

        public void Start()
        {
            var token = _CancelTokenSource.Token;
            Task.Factory.StartNew(() => WatchBoundProperties(token), token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void Dispose()
        {
            if (_CancelTokenSource != null) _CancelTokenSource.Cancel();
        }

        #region Event handlers

        private void HandleFloatBoundPropertyChanged(RemoteElement owner, string propertyName, BoundPropertyValueChangedEventArgs<float> args)
        {
            owner.OnFloatBoundPropertyValueChanged(propertyName, args.NewValue);
        }

        private void HandleBooleanBoundPropertyChanged(RemoteElement owner, string propertyName, BoundPropertyValueChangedEventArgs<bool> args)
        {
            owner.OnBooleanBoundPropertyValueChanged(propertyName, args.NewValue);
        }

        private void HandleStringBoundPropertyChanged(RemoteElement owner, string propertyName, BoundPropertyValueChangedEventArgs<string> args)
        {
            owner.OnStringBoundPropertyValueChanged(propertyName, args.NewValue);
        }

        private void HandleBoundPropertyError(BoundPropertyErrorEventArgs args)
        {
            _Log.Error(string.Format("{0} while watching property for [{1}].{2}: {3}", args.Exception.GetType().Name, args.Owner, args.PropertyName, args.Exception.Message), args.Exception);
            BoundPropertyError?.Invoke(args);
        }

        #endregion

        private void WatchBoundProperties(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                DateTime start = DateTime.Now;

                _Lock.EnterReadLock();
                try
                {
                    foreach (var ctxt in _FloatContexts)
                    {
                        ctxt.Run();
                    }
                    foreach (var ctxt in _BooleanContexts)
                    {
                        ctxt.Run();
                    }
                    foreach (var ctxt in _StringContexts)
                    {
                        ctxt.Run();
                    }
                }
                finally
                {
                    _Lock.ExitReadLock();
                }

                TimeSpan duration = DateTime.Now - start;
                double millis = duration.TotalMilliseconds;
                if (millis < DELAY)
                {
                    if (millis < 0) millis = 0;
                    var cancelled = token.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(DELAY - millis));
                    if (cancelled) break;
                }
            }
        }

        private class WatcherContext<T, TProp> : IDisposable where T : IBoundProperty<TProp>
        {
            private T _BoundProperty;
            private Action<RemoteElement, string, BoundPropertyValueChangedEventArgs<TProp>> _ValueChangedCallback;
            private Action<BoundPropertyErrorEventArgs> _ErrorCallback;
            private TProp _OldValue = default(TProp);

            public RemoteElement Owner { get; private set; }

            public string PropertyName { get; private set; }

            private bool _NeedsWatching = true;

            public WatcherContext(RemoteElement owner, string propertyName, T boundProperty, Action<RemoteElement, string, BoundPropertyValueChangedEventArgs<TProp>> valueChangedCallback, Action<BoundPropertyErrorEventArgs> errorCallback)
            {
                Owner = owner;
                PropertyName = propertyName;
                _BoundProperty = boundProperty;
                _ValueChangedCallback = valueChangedCallback;
                _ErrorCallback = errorCallback;
                if (_BoundProperty.SupportsValueChanged) _BoundProperty.ValueChanged += _BoundProperty_ValueChanged;
                _BoundProperty.Initialize();
            }

            private void _BoundProperty_ValueChanged(object sender, BoundPropertyValueChangedEventArgs<TProp> args)
            {
                _ValueChangedCallback?.Invoke(Owner, PropertyName, args);
            }

            public void Run()
            {
                if (_NeedsWatching)
                {
                    _NeedsWatching = !_BoundProperty.SupportsValueChanged;
                    TProp value;
                    try
                    {
                        value = _BoundProperty.GetValue();
                    }
                    catch (Exception ex)
                    {
                        _ErrorCallback?.Invoke(new BoundPropertyErrorEventArgs(Owner, PropertyName, ex));
                        return;
                    }
                    if ((_OldValue == null && value != null) || (_OldValue != null && value == null) || (_OldValue != null && value != null && !_OldValue.Equals(value)))
                    {
                        _ValueChangedCallback?.Invoke(Owner, PropertyName, new BoundPropertyValueChangedEventArgs<TProp>(_OldValue, value));
                        _OldValue = value;
                    }
                }
            }

            public void Dispose()
            {
                _BoundProperty.Dispose();
                if (_BoundProperty.SupportsValueChanged) _BoundProperty.ValueChanged -= _BoundProperty_ValueChanged;
            }
        }

    }

    internal delegate void BoundPropertyErrorHandler(BoundPropertyErrorEventArgs args);

    internal class BoundPropertyErrorEventArgs : EventArgs
    {
        public BoundPropertyErrorEventArgs(RemoteElement owner, string propertyName, Exception ex)
        {
            Owner = owner;
            PropertyName = propertyName;
            Exception = ex;
        }

        public RemoteElement Owner { get; private set; }

        public string PropertyName { get; private set; }

        public Exception Exception { get; private set; }
    }
}
