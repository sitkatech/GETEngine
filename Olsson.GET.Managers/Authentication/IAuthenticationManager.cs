using Olsson.GET.Common.DataContracts.Users;
using System.ServiceModel;

namespace Olsson.GET.Managers.Authentication
{
    [ServiceContract]
    public interface IAuthenticationManager
    {
        [OperationContract]
        Role[] GetUserRoles(int userId);

        [OperationContract]
        Role FindRoleById(int roleId);

        [OperationContract]
        User CreateUser(User user);

        [OperationContract]
        void DeleteUser(User user);

        [OperationContract]
        User FindUserById(int userId);

        [OperationContract]
        User FindUserByUserName(string userName);

        [OperationContract]
        User FindUserByUserEmail(string email);

        [OperationContract]
        User UpdateUser(User user);

        [OperationContract]
        void AddRoleToUser(User user, string roleName);

        [OperationContract]
        bool IsUserInRole(User user, string roleName);

        [OperationContract]
        void RemoveUserFromRole(User user, string roleName);

        [OperationContract]
        User AcceptEula(int userId);
    }
}
