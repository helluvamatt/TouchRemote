using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.TinyIoc;
using Nancy.Diagnostics;
using TouchRemote.Model;
using Nancy.Conventions;
using Nancy.Bootstrapper;
using Nancy.Session;
using System.IO;
using Nancy.ViewEngines;

namespace TouchRemote.Web
{
    internal class AppBootstrapper : DefaultNancyBootstrapper
    {
        private IRemoteControlService _Service;
        private byte[] _Favicon;

        public AppBootstrapper(IRemoteControlService service)
        {
            _Service = service;
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            DiagnosticsHook.Disable(pipelines);
            StaticConfiguration.DisableErrorTraces = false;
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            container.Register(_Service);
            var assembly = GetType().Assembly;
            ResourceViewLocationProvider.RootNamespaces.Add(assembly, "TouchRemote.Web.Views");
        }

        protected override NancyInternalConfiguration InternalConfiguration
        {
            get
            {
                return NancyInternalConfiguration.WithOverrides(OnConfigurationBuilder);
            }
        }

        private void OnConfigurationBuilder(NancyInternalConfiguration internalConfiguration)
        {
            internalConfiguration.ViewLocationProvider = typeof(ResourceViewLocationProvider);
        }

        protected override IEnumerable<ModuleRegistration> Modules
        {
            get
            {
                return new ModuleRegistration[] {
                    new ModuleRegistration(typeof(AssetsModule)),
                    new ModuleRegistration(typeof(WebModule)),
                };
            }
        }

        protected override byte[] FavIcon
        {
            get
            {
                return _Favicon ?? (_Favicon = LoadFavIcon());
            }
        }

        private byte[] LoadFavIcon()
        {
            using (var resourceStream = GetType().Assembly.GetManifestResourceStream("TouchRemote.appicon.ico"))
            {
                var memoryStream = new MemoryStream();
                resourceStream.CopyTo(memoryStream);
                return memoryStream.GetBuffer();
            }
        }
    }
}
