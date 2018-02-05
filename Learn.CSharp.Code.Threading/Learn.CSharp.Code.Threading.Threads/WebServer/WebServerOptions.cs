namespace Learn.CSharp.Code.Threading.Threads.WebServer
{
    using System;
    using System.Collections.Generic;
    
    
    public class WebServerOptions
    {
        public string Host { get; set; }

        public int Port { get; set; }
        
        public IList<IRequestHandler> Handlers => new List<IRequestHandler>();

        public int Concurrency { get; set; } = Environment.ProcessorCount * 2 - 2;

        public int QueueWaitingTimeoutMs { get; set; } = 500;
    }
}