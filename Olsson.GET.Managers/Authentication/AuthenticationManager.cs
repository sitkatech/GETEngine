using log4net;
using Microsoft.Extensions.Logging;
using Olsson.GET.Accessors.Authentication;
using Olsson.GET.Common.DataContracts.Users;
using Olsson.GET.Common.Utilities;

namespace Olsson.GET.Managers.Authentication
{
    public class AuthenticationManager : BaseManager, IAuthenticationManager
    {
        private static readonly ILogger Logger = Logging.GetLogger<AuthenticationManager>();

        public Role[] GetUserRoles(int userId)
        {
            Logger.LogInformation($"Getting roles for user {userId}");

            return AccessorFactory.CreateAccessor<IUserAccessor>().FindUserRoles(userId);
        }

        public Role FindRoleById(int roleId)
        {
            Logger.LogInformation($"Finding role by id {roleId}");

            return AccessorFactory.CreateAccessor<IUserAccessor>().FindRole(roleId);
        }

        public User CreateUser(User user)
        {
            Logger.LogInformation($"Creating user {user?.UserName}");

            return AccessorFactory.CreateAccessor<IUserAccessor>().CreateOrUpdateUser(user);
        }

        public void DeleteUser(User user)
        {

        }

        public User FindUserByUserName(string userName)
        {
            Logger.LogInformation($"Finding user by user name {userName}");

            return AccessorFactory.CreateAccessor<IUserAccessor>().FindUserByName(userName);
        }

        public User FindUserByUserEmail(string email)
        {
            Logger.LogInformation($"Finding user by email {email}");

            return AccessorFactory.CreateAccessor<IUserAccessor>().FindUserByEmail(email);
        }

        public User UpdateUser(User user)
        {
            Logger.LogInformation($"Updating user {user?.UserName}");

            return AccessorFactory.CreateAccessor<IUserAccessor>().CreateOrUpdateUser(user);
        }

        public bool IsUserInRole(User user, string roleName)
        {
            Logger.LogInformation($"Checking user {user.UserName} for role {roleName}");

            return AccessorFactory.CreateAccessor<IUserAccessor>().IsUserInRole(user.UserID, roleName);
        }

        public void AddRoleToUser(User user, string roleName)
        {
            Logger.LogInformation($"Adding user {user.UserName} to role {roleName}");

            AccessorFactory.CreateAccessor<IUserAccessor>().AddUserToRole(user.Id, roleName);
        }

        public User FindUserById(int userId)
        {
            Logger.LogInformation($"Finding user by id {userId}");

            return AccessorFactory.CreateAccessor<IUserAccessor>().FindUser(userId);
        }

        public User AcceptEula(int userId)
        {
            return AccessorFactory.CreateAccessor<IUserAccessor>().AcceptEula(userId);
        }

        public void RemoveUserFromRole(User user, string roleName)
        {
            Logger.LogInformation($"Removing user {user.UserName} from role {roleName}");

            AccessorFactory.CreateAccessor<IUserAccessor>().RemoveUserFromRole(user.UserID, roleName);
        }
    }
}
