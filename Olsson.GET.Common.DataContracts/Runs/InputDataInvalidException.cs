using System;

namespace Olsson.GET.Common.DataContracts.Runs
{
    public class InputDataInvalidException : Exception
    {
        public InputDataInvalidException(string message) : base(message)
        {
        }
    }
}