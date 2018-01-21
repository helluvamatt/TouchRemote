using TouchRemote.Model;

namespace TouchRemote.Web.Models
{
    public class AuthResponse<T>
    {
        private AuthResponse() { }

        public T Data { get; private set; }

        public AuthState AuthState { get; private set; }

        public static AuthResponse<T> Authenticated(T data)
        {
            return new AuthResponse<T> { AuthState = AuthState.Authenticated, Data = data };
        }

        public static AuthResponse<T> NotAuthenticated(AuthState authState)
        {
            return new AuthResponse<T> { AuthState = authState };
        }
    }

    public class AuthResponse
    {
        private AuthResponse() { }

        public AuthState AuthState { get; private set; }

        public static AuthResponse Authenticated
        {
            get
            {
                return new AuthResponse { AuthState = AuthState.Authenticated };
            }
        }

        public static AuthResponse NotAuthenticated(AuthState authState)
        {
            return new AuthResponse { AuthState = authState };
        }
    }
}
