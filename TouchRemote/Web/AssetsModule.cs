using Nancy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TouchRemote.Web
{
    internal class AssetsModule : NancyModule
    {
        private const string BASE_NAMESPACE = "TouchRemote.Web.Assets";

        private HashSet<string> _Assets;
        private Assembly _Assembly;

        public AssetsModule() : base("/assets")
        {
            _Assets = new HashSet<string>();
            _Assembly = Assembly.GetExecutingAssembly();
            string baseNsTest = BASE_NAMESPACE + ".";
            foreach (var resourceName in _Assembly.GetManifestResourceNames())
            {
                if (resourceName.StartsWith(baseNsTest))
                {
                    _Assets.Add(resourceName);
                }
            }
            Get["/{path*}"] = GetAsset;
        }

        private dynamic GetAsset(dynamic parms)
        {
            string resourceName = BASE_NAMESPACE + "." + TranslatePath(parms.path);
            if (_Assets.Contains(resourceName))
            {
                return Response.FromStream(CreateReadStream(resourceName), MimeTypes.MimeTypeMap.GetMimeType(Path.GetExtension(resourceName)));
            }
            return HttpStatusCode.NotFound;
        }

        private Stream CreateReadStream(string resourceName)
        {
            return _Assembly.GetManifestResourceStream(resourceName);
        }

        private static string TranslatePath(string path)
        {
            return path.Replace('/', '.').Replace('\\', '.');
        }
    }
}
