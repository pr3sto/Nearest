using System;

namespace Nearest.GoogleApi.Models
{
    public class ApiCallException : Exception
    {
        public ApiCallException()
        {
        }

        public ApiCallException(string message)
            : base(message)
        {
        }

        public ApiCallException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class OverQueryLimitException : Exception
    {
        public OverQueryLimitException()
        {
        }

        public OverQueryLimitException(string message)
            : base(message)
        {
        }

        public OverQueryLimitException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class QueryAutoCompleteException : Exception
    {
        public QueryAutoCompleteException()
        {
        }

        public QueryAutoCompleteException(string message)
            : base(message)
        {
        }

        public QueryAutoCompleteException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class NearbyPlacesSearchException : Exception
    {
        public NearbyPlacesSearchException()
        {
        }

        public NearbyPlacesSearchException(string message)
            : base(message)
        {
        }

        public NearbyPlacesSearchException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class DirectionsException : Exception
    {
        public DirectionsException()
        {
        }

        public DirectionsException(string message)
            : base(message)
        {
        }

        public DirectionsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
