using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace TouchRemote.Utils
{
	static public class SingleInstance
	{
		static Mutex mutex;
		static public bool Start()
		{
			bool onlyInstance = false;
			string mutexName = String.Format("Local\\{0}", AssemblyGuid);
			mutex = new Mutex(true, mutexName, out onlyInstance);
			return onlyInstance;
		}

		static public void Stop()
		{
			mutex.ReleaseMutex();
		}

		static public string AssemblyGuid
		{
			get
			{
				object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(GuidAttribute), false);
                if (attributes.Length > 0)
                {
                    return ((GuidAttribute)attributes[0]).Value;
                }
                throw new ArgumentNullException("GuidAttribute", "Entry assembly must specify a GuidAttribute");
            }
		}
	}
}
