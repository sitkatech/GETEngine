using log4net;
using Olsson.GET.Accessors.Authentication;
using Olsson.GET.Common.DataContracts.Users;
using Olsson.GET.Common.Utilities;

namespace Olsson.GET.Managers.Authentication
{
    public class AuthenticationManager : BaseManager, IAuthenticationManager
    {
        private static readonly ILog Logger = Logging.GetLogger(typeof(AuthenticationManager));

        public Role[] GetUserRoles(int userId)
        {
            Logger.Info($"Getting roles for user {userId}");

            return AccessorFactory.CreateAccessor<IUserAccessor>().FindUserRoles(userId);
        }

        public Role FindRoleById(int roleId)
        {
            Logger.Info($"Finding role by id {roleId}");

            return AccessorFactory.CreateAccessor<IUserAccessor>().FindRole(roleId);
        }

        public User CreateUser(User user)
        {
            Logger.Info($"Creating user {user?.UserName}");

            return AccessorFactory.CreateAccessor<IUserAccessor>().CreateOrUpdateUser(user);
        }

        public void DeleteUser(User user)
        {

        }

        public User FindUserByUserName(string userName)
        {
            Logger.Info($"Finding user by user name {userName}");

            return AccessorFactory.CreateAccessor<IUserAccessor>().FindUserByName(userName);
        }

        public User FindUserByUserEmail(string email)
        {
            Logger.Info($"Finding user by email {email}");

            return AccessorFactory.CreateAccessor<IUserAccessor>().FindUserByEmail(email);
        }

        public User UpdateUser(User user)
        {
            Logger.Info($"Updating user {user?.UserName}");

            return AccessorFactory.CreateAccessor<IUserAccessor>().CreateOrUpdateUser(user);
        }

        public bool IsUserInRole(User user, string roleName)
        {
            Logger.Info($"Checking user {user.UserName} for role {roleName}");

            return AccessorFactory.CreateAccessor<IUserAccessor>().IsUserInRole(user.UserID, roleName);
        }

        public void AddRoleToUser(User user, string roleName)
        {
            Logger.Info($"Adding user {user.UserName} to role {roleName}");

            AccessorFactory.CreateAccessor<IUserAccessor>().AddUserToRole(user.Id, roleName);
        }

        public User FindUserById(int userId)
        {
            Logger.Info($"Finding user by id {userId}");

            return AccessorFactory.CreateAccessor<IUserAccessor>().FindUser(userId);
        }

        public User AcceptEula(int userId)
        {
            return AccessorFactory.CreateAccessor<IUserAccessor>().AcceptEula(userId);
        }

        public void RemoveUserFromRole(User user, string roleName)
        {
            Logger.Info($"Removing user {user.UserName} from role {roleName}");

            AccessorFactory.CreateAccessor<IUserAccessor>().RemoveUserFromRole(user.UserID, roleName);
        }
    }
}
