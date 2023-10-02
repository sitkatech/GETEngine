using System.Configuration;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Olsson.GET.Common.Utilities;

namespace Olsson.GET.Tests.AccessorTests
{
    public class BaseAccessorTest
    {
        TransactionScope transaction;

        [TestInitialize]
        public void Init()
        {
            transaction = new TransactionScope();
            ConfigurationHelper.Build();
        }

        [TestCleanup]
        public void Cleanup()
        {
            transaction?.Dispose();
        }
    }
}
