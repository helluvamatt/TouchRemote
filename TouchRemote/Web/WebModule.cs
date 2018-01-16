using Nancy;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchRemote.Model;
using TouchRemote.Web.Models;
using JWT;
using JWT.Serializers;
using JWT.Algorithms;
using System.Reflection;
using System.Collections.Concurrent;

namespace TouchRemote.Web
{
    internal class WebModule : NancyModule
    {
        private const string TOKEN_COOKIE = "touchremote.token";

        private IRemoteControlService _RemoteControlService;
        private readonly string _SecretKey;
        private readonly IJwtEncoder _JwtEncoder;

        public WebModule(IRemoteControlService remoteControlService)
        {
            _RemoteControlService = remoteControlService;
            
            _JwtEncoder = new JwtEncoder(new HMACSHA512Algorithm(), new JsonNetSerializer(), new JwtBase64UrlEncoder());
            byte[] keyBytes = new byte[1024];
            new Random().NextBytes(keyBytes);
            _SecretKey = BitConverter.ToString(keyBytes).Replace("-", "");

            Get["/"] = DefaultHandler;
            Get["/{pathInfo*}"] = DefaultHandler;

            Post["/login"] = PostLoginHandler;
        }

        private dynamic DefaultHandler(dynamic parms)
        {
            return View["index.html"];
        }

        private dynamic PostLoginHandler(dynamic parms)
        {
            var loginRequest = this.Bind<LoginRequest>();
            string requiredPassword = _RemoteControlService.GetRequiredPassword();
            if (string.IsNullOrEmpty(requiredPassword))
            {
                return HttpStatusCode.Forbidden;
            }
            if (loginRequest.Password == requiredPassword)
            {
                string token = _JwtEncoder.Encode(TokenPayload.WithKey(requiredPassword), _SecretKey);
                return Response.AsJson(new Token { TokenValue = token }).WithCookie(TOKEN_COOKIE, token);
            }
            else
            {
                return HttpStatusCode.Unauthorized;
            }
        }
    }
}
