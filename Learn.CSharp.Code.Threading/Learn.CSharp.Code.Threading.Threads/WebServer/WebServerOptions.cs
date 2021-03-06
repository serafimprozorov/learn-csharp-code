﻿using System.Collections.Generic;


namespace Learn.CSharp.Code.Threading.Threads.WebServer
{
    public class WebServerOptions
    {
        public string Host { get; set; }
        
        public int Port { get; set; }
        
        public int Concurrency { get; set; }
        
        public int QueueWaitingTimeoutMs { get; set; }
        
        public List<IRequestHandler> Handlers { get; } = new List<IRequestHandler>();
    }
}