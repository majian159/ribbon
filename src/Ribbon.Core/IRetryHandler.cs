using System;

namespace Ribbon.Client
{
    public interface IRetryHandler
    {
        bool IsRetriableException(Exception exception, bool sameServer);

        bool IsCircuitTrippingException(Exception exception);

        int GetMaxRetriesOnSameServer();

        int GetMaxRetriesOnNextServer();
    }
}