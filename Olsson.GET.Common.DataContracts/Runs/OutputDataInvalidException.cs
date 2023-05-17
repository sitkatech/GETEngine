using System;

namespace Olsson.GET.Common.DataContracts.Runs
{
    public class OutputDataInvalidException : Exception
    {
        public OutputDataInvalidException(string message, int status) : base(message)
        {
            Status = status;
        }

        public int Status { get; set; }
    }
}