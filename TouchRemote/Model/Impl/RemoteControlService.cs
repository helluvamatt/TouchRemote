using log4net;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using TouchRemote.Model.Persistence.Controls;
using TouchRemote.Model.Persistence;
using TouchRemote.Properties;
using TouchRemote.Utils;
using TouchRemote.Web.Hubs;
using TouchRemote.Web.Models;

namespace TouchRemote.Model.Impl
{
    internal class RemoteControlService : IRemoteControlService, INotifyPropertyChanged, IDisposable
    {
        #region Private members

        private PluginManager _PluginManager;

        private ConcurrentDictionary<Guid, RemoteElement> _ElementDict;
        private bool _RaiseElementsChanged = true;

        private ILog _Log;

        private PersistenceManager _PersistenceManager;

        private BoundPropertyWatcher _Watcher;

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<RemoteElement> Buttons => _ElementDict.Values;

        public int Count => _ElementDict.Count;

        public RemoteControlService(string appDataPath, PluginManager pluginManager)
        {
            _Log = LogManager.GetLogger(GetType());
            _PluginManager = pluginManager;
            _PersistenceManager = new PersistenceManager(_PluginManager, Path.Combine(appDataPath, "Controls.xml"));
            _ElementDict = new ConcurrentDictionary<Guid, RemoteElement>();
            _Watcher = new BoundPropertyWatcher();
            Load();
            _Watcher.Start();
        }

        public Guid CreateId()
        {
            Guid guid = Guid.NewGuid();
            while (_ElementDict.ContainsKey(guid))
            {
                guid = Guid.NewGuid();
            }
            return guid;
        }

        public bool AddElement(RemoteElement element)
        {
            if (element.MaxControlTypeCount > 0)
            {
                int existingCount = _ElementDict.Where(kvp => kvp.Value.GetType().Equals(element.GetType())).Count();
                if (existingCount + 1 > element.MaxControlTypeCount) return false;
            }

            if (_ElementDict.TryAdd(element.Id, element))
            {
                element.PropertyChanged += Element_PropertyChanged;
                ElementsChanged();
                return true;
            }
            return false;
        }

        public bool RemoveElement(Guid id)
        {
            RemoteElement element;
            if (_ElementDict.TryRemove(id, out element))
            {
                element.PropertyChanged -= Element_PropertyChanged;
                ElementsChanged();
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            if (_Watcher != null) _Watcher.Dispose();
        }

        #region IRemoteControlService implementation

        public bool ProcessEvent(Guid id, string eventName, object eventData)
        {
            _Log.InfoFormat("Handling event \"{0}\" for \"{1}\"...", eventName, id);
            if (_ElementDict.ContainsKey(id))
            {
                RemoteElement control = _ElementDict[id];
                if (control.CanHandleEvent(eventName))
                {
                    try
                    {
                        control.ProcessEvent(eventName, eventData);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _Log.Error(string.Format("{0} in event handler for \"{1}\": {2}", ex.GetType().Name, control, ex.Message), ex);
                    }
                }
                else
                {
                    _Log.ErrorFormat("Control \"{0}\" cannot handle event \"{1}\"", control, eventName);
                }
            }
            else
            {
                _Log.ErrorFormat("Element not found: \"{0}\"", id);
            }
            return false;
        }

        public IEnumerable<WebControl> ControlList
        {
            get
            {
                return new List<WebControl>(_ElementDict.Values.Select(e => e.ToWebControl()));
            }
        }

        #endregion

        #region Private methods

        #region Event handlers

        private void ElementsChanged(bool fromSelf = false)
        {
            if (_RaiseElementsChanged)
            {
                if (!fromSelf) Save();
                RemoteHub.GetBroadcastContext().RefreshControls();
                _Watcher.UpdateBoundProperties(Buttons);
                PropertyChanged.Notify(() => Buttons);
            }
        }

        private void Element_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (_RaiseElementsChanged)
            {
                var e = sender as RemoteElement;
                Save();
                RemoteHub.GetBroadcastContext().UpdateControl(e.ToWebControl());
                if (e.BoundPropertyNames.Contains(args.PropertyName)) _Watcher.UpdateBoundProperties(e);
            }
        }

        #endregion

        private void Save()
        {
            try
            {
                _PersistenceManager.Save(Buttons);
            }
            catch (Exception ex)
            {
                _Log.Error(string.Format("Failed to save \"{0}\": {1}", _PersistenceManager.XmlFilename, ex.Message), ex);
            }
        }

        private void Load()
        {
            _Log.InfoFormat("Loading controls from \"{0}\"...", _PersistenceManager.XmlFilename);
            _RaiseElementsChanged = false;
            try
            {
                foreach (var e in Buttons)
                {
                    RemoveElement(e.Id);
                }
                foreach (var e in _PersistenceManager.Load())
                {
                    AddElement(e);
                }
                _Log.InfoFormat("Loaded {0} controls from \"{1}\".", _ElementDict.Count, _PersistenceManager.XmlFilename);
            }
            catch (Exception ex)
            {
                _Log.Error(string.Format("Failed to load \"{0}\": {1}", _PersistenceManager.XmlFilename, ex.Message), ex);
            }
            finally
            {
                _RaiseElementsChanged = true;
                ElementsChanged(true); // In case of reload
            }
        }

        #endregion

    }
}
