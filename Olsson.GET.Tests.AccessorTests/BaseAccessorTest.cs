using System.Configuration;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Olsson.GET.Tests.AccessorTests
{
    public class BaseAccessorTest
    {
        TransactionScope transaction;

        [TestInitialize]
        public void Init()
        {
            transaction = new TransactionScope();
        }

        [TestCleanup]
        public void Cleanup()
        {
            transaction?.Dispose();
        }
    }
}
