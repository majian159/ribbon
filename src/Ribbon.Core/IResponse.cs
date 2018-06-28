using System;
using System.Collections.Generic;

namespace Ribbon.Client
{
    public interface IResponse : IDisposable
    {
        object Body { get; }

        bool HasBody { get; }

        bool IsSuccess { get; }

        Uri RequestedUri { get; }

        IDictionary<string, object> Headers { get; }
    }
}