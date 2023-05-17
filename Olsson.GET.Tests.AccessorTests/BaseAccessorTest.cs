using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Olsson.GET.Accessors.Containers;

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
