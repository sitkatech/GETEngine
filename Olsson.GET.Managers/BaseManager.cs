using Microsoft.Extensions.Logging;
using Olsson.GET.Accessors;
using Olsson.GET.Common.Utilities;
using Olsson.GET.Engines;
using System;
using System.Threading.Tasks;

namespace Olsson.GET.Managers
{
    public abstract class BaseManager
    {
        private static readonly ILogger Logger = Logging.GetLogger<BaseManager>();
        public EngineFactory EngineFactory { get; set; }

        public AccessorFactory AccessorFactory { get; set; }

        internal ManagerFactory ManagerFactory { get; set; }

        protected void SendManagerToManagerCall<T>(Func<T, Task> action) where T : class
        {
            var task = action(ManagerFactory.CreateManager<T>());
            if (task.Status == TaskStatus.Created)
            {
                task.Start();
            }
            task.ContinueWith(a => Logger.LogError("Manager to Manager call failed.", a.Exception), TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
