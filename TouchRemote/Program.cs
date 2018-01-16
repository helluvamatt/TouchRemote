using log4net.Appender;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using TouchRemote.Properties;
using TouchRemote.Utils;

namespace TouchRemote
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                // Check if this is the only instance of this application
                if (!SingleInstance.Start())
                {
                    // This is not the only instance, exit
                    Console.WriteLine(Resources.AnotherInstance);
                    return 1;
                }

                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var vendorName = typeof(AppContext).Assembly.GetCustomAttribute<AssemblyCompanyAttribute>().Company;
                var productName = typeof(AppContext).Assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
                appData = Path.Combine(appData, vendorName, productName);
                var logPath = Path.Combine(appData, "logs");

                // Configure logging
                Directory.CreateDirectory(logPath);
                var layout = new log4net.Layout.SimpleLayout();
                var logFileAppender = new RollingFileAppender { File = Path.Combine(logPath, "TouchRemote.log"), Layout = layout };
                BasicConfigurator.Configure(logFileAppender);

                // Create the TrayApplicationContext - the controller of the entire application
                var app = new AppContext(appData, vendorName, productName, args);
                app.Logger.Info("Initializing...");

                // Load GlobalStyle into the application resources
                app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/TouchRemote;component/UI/GlobalStyle.xaml", UriKind.RelativeOrAbsolute) });

                // Initialize the application
                app.InitializeContext();

                // Run the application
                app.Logger.Info("Initialized. Starting main loop...");
                int returnVal = 0;
                try
                {
                    returnVal = app.Run();
                }
                catch (Exception ex)
                {
                    app.Logger.Error("Unexpected exception:", ex);
                    MessageBox.Show(ex.Message, Resources.ProgramTerminated, MessageBoxButton.OK, MessageBoxImage.Error);
                    returnVal = 1;
                }

                // Log the shutdown
                app.Logger.Info("Exiting...");

                return returnVal;
            }
            finally
            {
                // all finished so release the mutex
                SingleInstance.Stop();
            }

        }
    }
}
