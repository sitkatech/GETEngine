using Microsoft.VisualStudio.TestTools.UnitTesting;
using Olsson.GET.Accessors;
using Olsson.GET.Accessors.Containers;
using Olsson.GET.Accessors.Customers;
using Olsson.GET.Accessors.FileIO;

namespace Olsson.GET.Tests.AccessorTests;

[TestClass]
public class ContainerAccessorTests
{
    private readonly IContainerAccessor _containerAccessor = new AccessorFactory().CreateAccessor<IContainerAccessor>();

    [TestMethod]
    public void CanGetAzureContainers()
    {
        var containers = _containerAccessor.GetAzureContainers().Result;
        Assert.IsNotNull(containers);

        var canQueue = _containerAccessor.CanQueueNewContainer().Result;
        Assert.IsNotNull(canQueue);
    }
    

}