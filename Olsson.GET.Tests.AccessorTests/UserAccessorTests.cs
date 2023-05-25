using System.Configuration;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Olsson.GET.Accessors;
using Olsson.GET.Accessors.Authentication;

namespace Olsson.GET.Tests.AccessorTests
{
    [TestClass]
    public class UserAccessorTests : BaseAccessorTest
    {
        IUserAccessor _userAccessor = new AccessorFactory().CreateAccessor<IUserAccessor>();

        [TestMethod]
        public void UserAccessor_UserCRUD()
        {
            var newName = "New Name";
            var name = "Name";
            var email = "santi@gmail.com";
            var phone = "1234567890";

            var newUser = _userAccessor.CreateOrUpdateUser(new Common.DataContracts.Users.User { UserName = name, FullName = name, Email = email, PhoneNumber = phone, CustomerID = 1 });

            Assert.IsNotNull(newUser);
            Assert.IsTrue(newUser.UserID > 0);

            var foundUserById = _userAccessor.FindUser(newUser.UserID);

            Assert.IsNotNull(foundUserById);

            var foundUserByName = _userAccessor.FindUserByName(name);

            Assert.IsNotNull(foundUserByName);

            foundUserByName.UserName = newName;

            var updatedUser = _userAccessor.CreateOrUpdateUser(foundUserByName);

            Assert.AreEqual(newName, updatedUser.UserName);
            Assert.AreEqual(email, updatedUser.Email);

            _userAccessor.AddRoleToUser(updatedUser.UserID, 1);

            var roles = _userAccessor.FindUserRoles(updatedUser.UserID);

            Assert.AreEqual(1, roles.Length);
            Assert.AreEqual("Admin", roles[0].RoleName);

            var usersForCustomer = _userAccessor.FindUsersForCustomer(1);

            Assert.IsTrue(usersForCustomer.Any(u => u.Email == email));

            var foundUserByEmail = _userAccessor.FindUserByEmail(email);

            Assert.IsNotNull(foundUserByEmail);
        }

        [TestMethod]
        public void UserAccessor_RoleTests()
        {
            var adminRole = _userAccessor.FindRole(1);

            Assert.IsNotNull(adminRole);
            Assert.AreEqual("Admin", adminRole.RoleName);
        }
    }
}
