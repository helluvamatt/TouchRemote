using log4net;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using TouchRemote.Model.Persistence;
using TouchRemote.Properties;
using TouchRemote.Utils;
using TouchRemote.Web.Hubs;
using TouchRemote.Web.Models;

namespace TouchRemote.Model
{
    internal class RemoteControlService : DependencyObject, IRemoteControlService
    {
        public ObservableCollection<Element> Buttons { get; private set; }

        private PluginManager _PluginManager;

        private Dictionary<Guid, Element> _ElementDict;

        private ILog _Log;

        private PersistenceManager _PersistenceManager;

        public RemoteControlService(string appDataPath, PluginManager pluginManager)
        {
            _Log = LogManager.GetLogger(GetType());
            _PluginManager = pluginManager;
            _PersistenceManager = new PersistenceManager(_PluginManager, Path.Combine(appDataPath, "Buttons.xml"));

            Buttons = new ObservableCollection<Element>();
            Buttons.CollectionChanged += Buttons_CollectionChanged;
            _ElementDict = new Dictionary<Guid, Element>();
            Load();
        }

        private void Buttons_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            Save();
            GetBroadcastContext().RefreshControls();
        }

        private void Element_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            var e = sender as Button;
            Save();
            GetBroadcastContext().UpdateControl(new WebControl(e.Id, (int)Math.Round(e.X), (int)Math.Round(e.Y), GetControlType(e), e.WebProperties));
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

        public bool AddElement(Element element)
        {
            if (_ElementDict.ContainsKey(element.Id)) return false;
            _ElementDict.Add(element.Id, element);
            Buttons.Add(element);
            element.PropertyChanged += Element_PropertyChanged;
            return true;
        }

        public bool RemoveElement(Element element)
        {
            if (_ElementDict.Remove(element.Id) && Buttons.Remove(element))
            {
                element.PropertyChanged -= Element_PropertyChanged;
                return true;
            }
            return false;
        }

        private void Save()
        {
            try
            {
                _PersistenceManager.Save(Buttons);
            }
            catch (Exception ex)
            {
                _Log.Error(string.Format("Failed to save Buttons.xml: {0}", ex.Message), ex);
            }
        }

        private void Load()
        {
            try
            {
                Buttons.CollectionChanged -= Buttons_CollectionChanged;
                foreach (var e in Buttons)
                {
                    RemoveElement(e);
                }
                foreach (var e in _PersistenceManager.Load())
                {
                    AddElement(e);
                }
            }
            catch (Exception ex)
            {
                _Log.Error(string.Format("Failed to load Buttons.xml: {0}", ex.Message), ex);
            }
            finally
            {
                Buttons.CollectionChanged += Buttons_CollectionChanged;
            }
        }

        #region IRemoteControlService implementation

        public bool ProcessEvent(Guid id, string eventName, object eventData)
        {
            return this.Invoke(() => {
                _Log.InfoFormat("Handling button click for \"{0}\"...", id);
                if (_ElementDict.ContainsKey(id))
                {
                    Element control = _ElementDict[id];
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
            });
        }

        public IEnumerable<WebControl> ControlList
        {
            get
            {
                return this.Invoke(() => {
                    return new List<WebControl>(Buttons.Select(e => new WebControl(e.Id, (int)Math.Round(e.X), (int)Math.Round(e.Y), GetControlType(e), e.WebProperties)));
                });
            }
        }

        public WebControl GetControl(Guid guid)
        {
            return this.Invoke(() => {
                var e = _ElementDict[guid];
                
                return _ElementDict.ContainsKey(guid) ? new WebControl(e.Id, (int)Math.Round(e.X), (int)Math.Round(e.Y), GetControlType(e), e.WebProperties) : null;
            });
        }

        public string GetRequiredPassword()
        {
            return Settings.Default.RequiredPassword;
        }

        #endregion

        #region SignalR notifications

        private IClient GetBroadcastContext()
        {
            return GlobalHost.ConnectionManager.GetHubContext<RemoteHub, IClient>().Clients.All;
        }

        #endregion

        private WebControl.WebControlType GetControlType(Element control)
        {
            if (control is Button)
            {
                return WebControl.WebControlType.Button;
            }
            return WebControl.WebControlType.Unknown;
        }

    }
}
