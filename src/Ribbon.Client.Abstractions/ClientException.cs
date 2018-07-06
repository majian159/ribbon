using System;

namespace Ribbon.Client
{
    public class ClientException : Exception
    {
        public enum ErrorType
        {
            General,
            Configuration,
            NumberofRetriesExceeded,
            NumberofRetriesNextServerExceeded,
            TimeoutException,
            UnknownHostException,
            ConnectException,
            ClientThrottled,
            ServerThrottled,
            NoRouteToHostException
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

        public ClientException(int errorCode, string message, Exception innerException)
            : base(message == null && errorCode != 0 ? ", code=" + errorCode + "->" + (ErrorType)errorCode : message, innerException)
        {
            ErrorCode = errorCode;
        }

        public ClientException(ErrorType type)
            : this((int)type, null, null)
        {
        }

        public ClientException(ErrorType type, string message)
            : this((int)type, message, null)
        {
        }

        public ClientException(ErrorType type, string message, Exception innerException)
            : base(message == null && type != ErrorType.General ? ", code=" + (int)type + "->" + type : message, innerException)
        {
            ErrorCode = (int)type;
            Type = type;
        }

        public ErrorType Type { get; }
        public int ErrorCode { get; set; }
    }
}