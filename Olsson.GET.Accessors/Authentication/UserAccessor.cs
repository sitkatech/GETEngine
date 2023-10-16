using System;
using System.Globalization;
using System.Linq;
using Olsson.GET.Accessors.EntityFramework;
using Role = Olsson.GET.Common.DataContracts.Users.Role;
using User = Olsson.GET.Common.DataContracts.Users.User;

namespace Olsson.GET.Accessors.Authentication
{
    class UserAccessor : BaseTableAccessor, IUserAccessor
    {
        public void AddRoleToUser(int userID, int roleID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var role = EntityFramework.Role.AllLookupDictionary[roleID];
                var userRole = db.UserRoles.SingleOrDefault(u => u.UserID == userID && u.RoleID == role.RoleID);

                if (role != null && userRole == null)
                {
                    db.UserRoles.Add(new UserRole() { UserID = userID, RoleID = roleID});
                    db.SaveChanges();
                }
            }
        }

        public void RemoveUserFromRole(int userID, string roleName)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var role = EntityFramework.Role.All.SingleOrDefault(r => r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase));
                var userRole = db.UserRoles.SingleOrDefault(u => u.UserID == userID && u.RoleID == role.RoleID);

                if (userRole != null)
                {
                    db.UserRoles.Remove(userRole);
                    db.SaveChanges();
                }
            }
        }

        public void AddUserToRole(int userID, string roleName)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var role = EntityFramework.Role.All.SingleOrDefault(r => r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase));
                var user = db.Users.Include("UserRoles").SingleOrDefault(u => u.UserID == userID);

                if (role != null && user != null && user.UserRoles.All(r => r.RoleID != role.RoleID))
                {
                    db.UserRoles.Add(new UserRole() { UserID = userID, RoleID = role.RoleID });
                    db.SaveChanges();
                }
            }
        }

        public User CreateOrUpdateUser(User user)
        {
            return CreateOrUpdate<User, EntityFramework.User, PrimaryDBContext>(user);
        }

        public Role FindRole(int roleID)
        {
            var result = EntityFramework.Role.AllLookupDictionary[roleID];
            return DTOMapper.Mapper.Map<Role>(result);
        }

        public User FindUser(int userID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var result = db.Users.FirstOrDefault(u => u.UserID == userID);

                return DTOMapper.Mapper.Map<User>(result);
            }
        }

        public User FindUserByEmail(string userEmail)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var result = db.Users.FirstOrDefault(u => u.Email == userEmail);

                return DTOMapper.Mapper.Map<User>(result);
            }
        }

        public User FindUserByName(string userName)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var result = db.Users.FirstOrDefault(u => u.UserName == userName);

                return DTOMapper.Mapper.Map<User>(result);
            }
        }

        public Role[] FindUserRoles(int userID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var roleIDs = db.UserRoles.Where(u => u.UserID == userID).Select(x => x.RoleID).ToList();
                var roles = EntityFramework.Role.All.Where(x => roleIDs.Contains(x.RoleID));
                return DTOMapper.Mapper.Map<Role[]>(roles);
            }
        }

        public User[] FindUsersForCustomer(int customerID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var result = db.Users.Where(u => u.CustomerID == customerID).ToArray();

                return DTOMapper.Mapper.Map<User[]>(result);
            }
        }

        public bool IsUserInRole(int userID, string roleName)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var roleIDs = db.UserRoles.Where(u => u.UserID == userID).Select(x => x.RoleID).ToList();

                if (!roleIDs.Any())
                    return false;

                return EntityFramework.Role.All.Any(r => r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase) && roleIDs.Contains(r.RoleID));
            }
        }

        public User AcceptEula(int userID)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var user = (from r in db.Users
                            where r.UserID == userID
                            select r).FirstOrDefault();

                if (user == null)
                {
                    return null;
                }

                user.EulaAcceptedDate = DateTime.UtcNow;

                db.SaveChanges();

                return DTOMapper.Mapper.Map<User>(user);
            }
        }
    }
}
