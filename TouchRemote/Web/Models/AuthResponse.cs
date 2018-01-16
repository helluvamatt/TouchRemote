namespace TouchRemote.Web.Models
{
    public class AuthResponse<T>
    {
        public T Data { get; private set; }

        public bool IsValid { get; private set; }

        public static AuthResponse<T> Authenticated(T data)
        {
            return new AuthResponse<T> { IsValid = true, Data = data };
        }

        public static AuthResponse<T> NotAuthenticated
        {
            get
            {
                return new AuthResponse<T> { IsValid = false };
            }
        }
    }
}
