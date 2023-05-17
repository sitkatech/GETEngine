using Olsson.GET.Common.DataContracts.Users;

namespace Olsson.GET.Accessors.Authentication
{
    public interface IUserAccessor
    {
        User FindUser(int userID);

        User FindUserByName(string userName);

        User FindUserByEmail(string userEmail);

        User CreateOrUpdateUser(User user);

        User[] FindUsersForCustomer(int customerID);

        Role FindRole(int roleID);

        void AddRoleToUser(int userID, int roleID);

        Role[] FindUserRoles(int userID);

        bool IsUserInRole(int userID, string roleName);

        void AddUserToRole(int userID, string roleName);

        void RemoveUserFromRole(int userID, string roleName);

        User AcceptEula(int userID);
    }
}
