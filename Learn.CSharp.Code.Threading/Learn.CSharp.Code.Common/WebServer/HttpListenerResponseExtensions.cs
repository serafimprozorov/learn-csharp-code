using System.Net;


namespace Learn.CSharp.Code.Threading.WebServer
{
    public static class HttpListenerResponseExtensions
    {
        public static bool Forbidden(this HttpListenerResponse @this)
        {
            @this.StatusCode = 403;
            @this.StatusDescription = "You are not allowed to access this resource";
            
            return true;
        }

        public static bool Ok(this HttpListenerResponse @this)
        {
            @this.StatusCode = 200;
            @this.StatusDescription = "OK";
            
            return true;
        }
        
        public static void NotFound(this HttpListenerResponse @this)
        {
            @this.StatusCode = 404;
            @this.StatusDescription = "The resource not found";
        }
        
        public static void IntenalServerError(this HttpListenerResponse @this)
        {
            @this.StatusCode = 500;
            @this.StatusDescription = "An unknown error has occured";
        }
    }
}