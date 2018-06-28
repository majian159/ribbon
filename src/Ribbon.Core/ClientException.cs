using System;

namespace Ribbon.Client
{
    public class ClientException : Exception
    {
        public enum ErrorType
        {
            GENERAL,
            CONFIGURATION,
            NUMBEROF_RETRIES_EXEEDED,
            NUMBEROF_RETRIES_NEXTSERVER_EXCEEDED,
            SOCKET_TIMEOUT_EXCEPTION,
            READ_TIMEOUT_EXCEPTION,
            UNKNOWN_HOST_EXCEPTION,
            CONNECT_EXCEPTION,
            CLIENT_THROTTLED,
            SERVER_THROTTLED,
            NO_ROUTE_TO_HOST_EXCEPTION,
            CACHE_MISSING
        }

        public ClientException(string message) : this(0, message, null)
        {
        }

        public ClientException(int errorCode) : this(errorCode, null, null)
        {
        }

        public ClientException(Exception exception) : this(0, null, exception)
        {
        }

        public ClientException(int errorCode, string message, Exception innerException) : base(message == null && errorCode != 0 ? ", code=" + errorCode + "->" + errorCode : message, innerException)
        {
            ErrorCode = errorCode;
        }

        public ClientException(ErrorType type) : this((int)type, null, null)
        {
        }

        public ClientException(ErrorType type, string message) : this((int)type, message, null)
        {
        }

        public ClientException(ErrorType type, string message, Exception innerException) : base(message == null && type != ErrorType.GENERAL ? ", code=" + type + "->" + type : message, innerException)
        {
            ErrorCode = (int)type;
            Type = type;
        }

        public ErrorType Type { get; }
        public int ErrorCode { get; set; }
    }
}