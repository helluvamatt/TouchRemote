using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using TouchRemote.Model.Persistence.Controls;

namespace TouchRemote.Model.Persistence
{
    internal class PersistenceManager
    {
        private PluginManager _PluginManager;
        private string _XmlFilename;
        private XmlSchema _Schema;
        private ILog _Log;

        private FixedSizedQueue<string> _SaveQueue;
        private readonly ReaderWriterLockSlim lockObj = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        public string XmlFilename => _XmlFilename;

        public PersistenceManager(PluginManager pluginManager, string xmlFilename)
        {
            _Log = LogManager.GetLogger(GetType());
            _PluginManager = pluginManager;
            _XmlFilename = xmlFilename;
            _SaveQueue = new FixedSizedQueue<string>(1);
            using (Stream schemaStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TouchRemote.Model.Persistence.Controls.xsd"))
            {
                _Schema = XmlSchema.Read(schemaStream, null);
            }
        }

        public void Save(IEnumerable<RemoteElement> elements)
        {
            XmlDocument document = new XmlDocument();
            document.Schemas.Add(_Schema);
            RemoteControls persistenceModel = new RemoteControls();
            foreach (var e in elements)
            {
                e.Deflate(document);
            }
            persistenceModel.Items = elements.ToArray();

            using (StringWriter writer = new StringWriter())
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                using (XmlWriter xmlWriter = XmlWriter.Create(writer, settings))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(RemoteControls));
                    xmlSerializer.Serialize(xmlWriter, persistenceModel);
                }
                _SaveQueue.Enqueue(writer.ToString());
            }

            Task.Factory.StartNew(() =>
            {
                string toSave;
                while (_SaveQueue.TryDequeue(out toSave))
                {
                    lockObj.EnterWriteLock();
                    try
                    {
                        using (var stream = File.Create(_XmlFilename))
                        {
                            using (StreamWriter writer = new StreamWriter(stream))
                            {
                                writer.WriteLine(toSave);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _Log.Error(string.Format("Failed to save \"Controls.xml\": {0}", ex.Message), ex);
                    }
                    finally
                    {
                        lockObj.ExitWriteLock();
                    }
                }
            });
        }

        public IEnumerable<RemoteElement> Load()
        {
            lockObj.EnterReadLock();
            try
            {
                using (StreamReader reader = new StreamReader(File.OpenRead(_XmlFilename)))
                {
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.IgnoreComments = true;
                    settings.IgnoreProcessingInstructions = true;
                    settings.IgnoreWhitespace = true;
                    settings.Schemas.Add(_Schema);
                    settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
                    settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
                    settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                    using (XmlReader xmlReader = XmlReader.Create(reader, settings))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(RemoteControls));
                        RemoteControls deserialized = (RemoteControls)xmlSerializer.Deserialize(xmlReader);
                        foreach (RemoteElement element in deserialized.Items)
                        {
                            var result = element.Inflate(_PluginManager);
                            if (!result.Success)
                            {
                                foreach (var error in result.Errors)
                                {
                                    _Log.WarnFormat("Failed to inflate control \"{0}\": {1}", element, error);
                                }
                            }
                        }
                        return new List<RemoteElement>(deserialized.Items);
                    }
                }
            }
            finally
            {
                lockObj.ExitReadLock();
            }
        }

        private class FixedSizedQueue<T>
        {
            private ConcurrentQueue<T> _Queue;
            private object lockObject = new object();

            public int Limit { get; private set; }

            public FixedSizedQueue(int limit)
            {
                Limit = limit;
                _Queue = new ConcurrentQueue<T>();
            }

            public bool TryDequeue(out T result)
            {
                return _Queue.TryDequeue(out result);
            }

            public void Enqueue(T obj)
            {
                _Queue.Enqueue(obj);
                lock (lockObject)
                {
                    T overflow;
                    while (_Queue.Count > Limit && _Queue.TryDequeue(out overflow)) ;
                }
            }
        }

    }
}
