using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Olsson.GET.Common.Shared.Extensions
{
    public static class ExceptionExtensions
    {
        public static string AllExceptionMessages(this Exception ex)
        {
            List<string> ret = new List<string>();
            Exception currentException = ex;

            if (currentException == null)
            {
                return "There is no exception.";
            }

            do
            {
                ret.Add(currentException.Message);
                currentException = currentException.InnerException;
            } while (currentException != null);

            return string.Join("\r\n", ret);
        }

        public static string AllStackTraces(this Exception ex)
        {
            List<string> ret = new List<string>();
            Exception currentException = ex;

            if (currentException == null)
            {
                return "There is no stack trace.";
            }

            do
            {
                ret.Add(currentException.StackTrace);
                currentException = currentException.InnerException;
            } while (currentException != null);

            return string.Join("\r\n", ret);
        }
    }
}
