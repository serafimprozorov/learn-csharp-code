namespace Learn.CSharp.Code.Threading.Threads
{
    using System;
    
    // I -- interface segreation;
    public interface ILockCookie : IDisposable
    {
        bool IsTaken { get; }
        
        void Release();
    }
}