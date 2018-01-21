using Nancy;

namespace TouchRemote.Web
{
    internal class WebModule : NancyModule
    {
        public WebModule()
        {
            Get["/"] = DefaultHandler;
            Get["/{pathInfo*}"] = DefaultHandler;
        }

        private dynamic DefaultHandler(dynamic parms)
        {
            return View["index.html"];
        }
    }
}
