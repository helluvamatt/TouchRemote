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
        public ObservableCollection<Button> Buttons { get; private set; }

        private PluginManager _PluginManager;

        private Dictionary<Guid, Button> _ButtonDict;

        private ILog _Log;

        private PersistenceManager _PersistenceManager;

        public RemoteControlService(string appDataPath, PluginManager pluginManager)
        {
            _Log = LogManager.GetLogger(GetType());
            _PluginManager = pluginManager;
            _PersistenceManager = new PersistenceManager(_PluginManager, Path.Combine(appDataPath, "Buttons.xml"));

            Buttons = new ObservableCollection<Button>();
            Buttons.CollectionChanged += Buttons_CollectionChanged;
            _ButtonDict = new Dictionary<Guid, Button>();
            Load();
        }

        private void Buttons_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Save();
            GetBroadcastContext().RefreshButtons();
        }

        private void Button_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var btn = sender as Button;
            Save();
            GetBroadcastContext().UpdateButton(new WebButton(btn.Id, btn.Label, btn.Icon));
        }

        public Guid CreateId()
        {
            Guid guid = Guid.NewGuid();
            while (_ButtonDict.ContainsKey(guid))
            {
                guid = Guid.NewGuid();
            }
            return guid;
        }

        public bool AddButton(Button button)
        {
            if (_ButtonDict.ContainsKey(button.Id)) return false;
            _ButtonDict.Add(button.Id, button);
            Buttons.Add(button);
            button.PropertyChanged += Button_PropertyChanged;
            return true;
        }

        public bool RemoveButton(Button button)
        {
            if (_ButtonDict.Remove(button.Id) && Buttons.Remove(button))
            {
                button.PropertyChanged -= Button_PropertyChanged;
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
                foreach (var btn in Buttons)
                {
                    RemoveButton(btn);
                }
                foreach (var btn in _PersistenceManager.Load())
                {
                    AddButton(btn);
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

        public bool Click(Guid id)
        {
            return this.Invoke(() => {
                _Log.InfoFormat("Handling button click for \"{0}\"...", id);
                if (_ButtonDict.ContainsKey(id))
                {
                    try
                    {
                        _ButtonDict[id].Click();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _Log.Error(string.Format("{0} in click action for \"{1}\": {2}", ex.GetType().Name, id, ex.Message), ex);
                    }
                }
                else
                {
                    _Log.ErrorFormat("Button not found: \"{0}\"", id);
                }
                return false;
            });
        }

        public IEnumerable<WebButton> ButtonList
        {
            get
            {
                return this.Invoke(() => {
                    return new List<WebButton>(Buttons.Select(btn => new WebButton(btn.Id, btn.Label, btn.Icon)));
                });
            }
        }

        public WebButton GetButton(Guid guid)
        {
            return this.Invoke(() => {
                var btn = _ButtonDict[guid];
                return _ButtonDict.ContainsKey(guid) ? new WebButton(btn.Id, btn.Label, btn.Icon) : null;
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

    }
}
