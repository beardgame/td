using System;

namespace Bearded.TD.Networking.MasterServer
{
    class MasterServerException : Exception
    {
        public Proto.ErrorType Type { get; }
        public string ErrorMessage { get; }

        public MasterServerException(Proto.ErrorType type, string errorMessage)
            : base(errorMessage)
        {
            ErrorMessage = errorMessage;
            Type = type;
        }
    }
}
