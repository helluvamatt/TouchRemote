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
        private readonly IJwtDecoder _JwtDecoder;

        public WebModule(IRemoteControlService remoteControlService)
        {
            _RemoteControlService = remoteControlService;
            
            var jsonSerializer = new JsonNetSerializer();
            var base64Encoder = new JwtBase64UrlEncoder();
            _JwtEncoder = new JwtEncoder(new HMACSHA512Algorithm(), jsonSerializer, base64Encoder);
            _JwtDecoder = new JwtDecoder(jsonSerializer, new JwtValidator(jsonSerializer, new UtcDateTimeProvider()), base64Encoder);
            _SecretKey = CreateRandomKey(512);

            Before += CheckToken;

            Get["/"] = DefaultHandler;
            Get["/login"] = DefaultHandler;
            //Get["/buttons"] = GetButtonsHandler;
            Post["/login"] = PostLoginHandler;
            //Post["/click"] = PostClickHandler;
        }

        private Response CheckToken(NancyContext ctxt)
        {
            var exceptPaths = new string[] { "/", "/login" };
            if (exceptPaths.Contains(ctxt.Request.Path) || IsAuthenticated(ctxt.Request)) return null;
            return new Response { StatusCode = HttpStatusCode.Unauthorized, ReasonPhrase = "Please login." };
        }

        private dynamic DefaultHandler(dynamic parms)
        {
            return View["index.html"];
        }

        private dynamic GetButtonsHandler(dynamic parms)
        {
            return Response.AsJson(_RemoteControlService.ButtonList);
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

        private dynamic PostClickHandler(dynamic parms)
        {
            ClickRequest request = this.Bind();
            var model = new ClickResponse();
            Guid id;
            if (Guid.TryParse(request.Id, out id) && _RemoteControlService.Click(id))
            {
                model.Success = true;
                model.Button = _RemoteControlService.GetButton(id);
            }
            return Response.AsJson(model);
        }

        private bool IsAuthenticated(Request request)
        {
            if (request.Cookies.ContainsKey(TOKEN_COOKIE))
            {
                string token = request.Cookies[TOKEN_COOKIE];
                return ValidateToken(token);
            }

            if (!string.IsNullOrEmpty(request.Headers.Authorization))
            {
                string[] tokenParts = request.Headers.Authorization.Split(' ');
                if (tokenParts.Length > 1)
                {
                    string token = tokenParts[1];
                    return ValidateToken(token);
                }
            }
            return false;
        }

        private bool ValidateToken(string token)
        {
            try
            {
                var payload = _JwtDecoder.DecodeToObject<TokenPayload>(token);
                return payload.Key == _RemoteControlService.GetRequiredPassword();
            }
            catch (SignatureVerificationException)
            {
                // Fall through
            }
            return false;
        }

        private string CreateRandomKey(int length)
        {
            byte[] keyBytes = new byte[length * 2];
            new Random().NextBytes(keyBytes);
            return BitConverter.ToString(keyBytes).Replace("-", "");
        }
    }
}
