using System;

namespace Olsson.GET.Accessors.APIFunctions
{
    public interface IAPIFunctionsAccessor
    {
        void MakeFunctionCall(string url);

        void NotificationFunctionCall(int runId, bool isSystemFailure, Exception ex);
    }
}
