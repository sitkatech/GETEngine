using Olsson.GET.Common.DataContracts.Users;
using Olsson.GET.Managers;
using Olsson.GET.Managers.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Olsson.GET.Accessors.EntityFramework;
using User = Olsson.GET.Common.DataContracts.Users.User;

namespace Olsson.GET.Common.AspIdentity
{

    public class CustomUserStore : IUserStore<ApplicationUser>
        , IUserPasswordStore<ApplicationUser>
        , IUserEmailStore<ApplicationUser>
        , IUserLockoutStore<ApplicationUser>
    , IUserTwoFactorStore<ApplicationUser>
    , IUserPhoneNumberStore<ApplicationUser>
    , IUserLoginStore<ApplicationUser>
    , IUserRoleStore<ApplicationUser>
    , IUserSecurityStampStore<ApplicationUser>
    {
        private readonly ILookupNormalizer _keyNormalizer;
        private readonly IAuthenticationManager _userManager = new ManagerFactory().CreateManager<IAuthenticationManager>();

        public CustomUserStore(ILookupNormalizer keyNormalizer)
        {
            _keyNormalizer = keyNormalizer;
        }

        #region IUserStore

        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                try
                {
                    return user.Id.ToString();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }, cancellationToken);
        }

        public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                try
                {
                    return user.UserName;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }, cancellationToken);
        }

        public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return UpdateAsync(user, cancellationToken);
        }

        public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                try
                {
                    return _keyNormalizer.NormalizeEmail(user.UserName);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }, cancellationToken);
        }

        public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                try
                {
                    var newUser = _userManager.CreateUser(ConvertToUser(user));
                    if (newUser != null)
                    {
                        return IdentityResult.Success;
                    }
                    throw new Exception("Failed to create new user");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return IdentityResult.Failed();
                }
            }, cancellationToken);
        }

        public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                try
                {
                    _userManager.DeleteUser(ConvertToUser(user));
                    return IdentityResult.Success;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return IdentityResult.Failed();
                }
            }, cancellationToken);
        }

        public Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return
                Task.Run(() =>
                {
                    try
                    {
                        var userIDInt = int.Parse(userId);
                        return ConvertToApplicationUser(_userManager.FindUserById(userIDInt));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        throw;
                    }
                }, cancellationToken);
        }

        public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                try
                {
                    _userManager.UpdateUser(ConvertToUser(user));
                    return IdentityResult.Success;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return IdentityResult.Failed();
                }
            }, cancellationToken);
        }

        public Task<ApplicationUser> FindByNameAsync(string userName, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                try
                {
                    return ConvertToApplicationUser(_userManager.FindUserByUserName(userName));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }, cancellationToken);
        }

        #endregion

        #region IUserPasswordStore

        public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.Password = passwordHash;

            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Password);
        }

        public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(!string.IsNullOrEmpty(user.Password));
        }

        #endregion

        #region IUserEmailStore

        public Task SetEmailAsync(ApplicationUser user, string email, CancellationToken cancellationToken)
        {
            user.UserName = email;
            return Task.FromResult(0);
        }

        public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public Task<string> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task<ApplicationUser> FindByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return
                Task.Run(
                    () =>
                        ConvertToApplicationUser(_userManager.FindUserByUserName(email)));
        }

        public Task<string> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return GetNormalizedUserNameAsync(user, cancellationToken);
        }

        public Task SetNormalizedEmailAsync(ApplicationUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


        #endregion

        #region IUserLockoutStore

        public Task<bool> GetLockoutEnabledAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.LockoutExpiration);
        }

        public Task SetLockoutEnabledAsync(ApplicationUser user, bool enabled, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public Task SetLockoutEndDateAsync(ApplicationUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            user.LockoutExpiration = lockoutEnd;

            return Task.FromResult(0);
        }

        public Task<int> GetAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.FailedAttemptCount);
        }

        public Task<int> IncrementAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.FailedAttemptCount++);
        }

        public Task ResetAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            user.FailedAttemptCount = 0;

            return Task.FromResult(0);
        }

        #endregion

        #region IUserTwoFactorStore

        public Task SetTwoFactorEnabledAsync(ApplicationUser user, bool enabled, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public Task<bool> GetTwoFactorEnabledAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        #endregion

        #region IUserPhoneNumberStore

        public Task<string> GetPhoneNumberAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(string.Empty);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task SetPhoneNumberAsync(ApplicationUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public Task SetPhoneNumberConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        #endregion

        #region IUserLoginStore

        public Task AddLoginAsync(ApplicationUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveLoginAsync(ApplicationUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.Run(() => (IList<UserLoginInfo>)new List<UserLoginInfo>() { }, cancellationToken);
        }

        public Task<ApplicationUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IUserRoleStore

        public Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            return Task.Run(() => { _userManager.AddRoleToUser(ConvertToUser(user), roleName); }, cancellationToken);
        }

        public Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            return Task.Run(() => { _userManager.RemoveUserFromRole(ConvertToUser(user), roleName); }, cancellationToken);
        }

        public Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                IList<string> ret = _userManager.GetUserRoles(user.UserID).Select(r => r.RoleName).ToList();
                return ret;
            }, cancellationToken);
        }

        public Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            return Task.Run(() => { return _userManager.IsUserInRole(ConvertToUser(user), roleName); }, cancellationToken);
        }

        public Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IUserSecurityStampStore

        public Task<string> GetSecurityStampAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.SecurityStamp);
        }

        public Task SetSecurityStampAsync(ApplicationUser user, string stamp, CancellationToken cancellationToken)
        {
            user.SecurityStamp = stamp;

            return Task.FromResult(0);
        }

        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            return;
        }

        private ApplicationUser ConvertToApplicationUser(DataContracts.Users.User user)
        {
            return user == null
                ? null
                : DTOMapper.Mapper.Map<ApplicationUser>(user);
        }

        private User ConvertToUser(ApplicationUser user)
        {
            return user == null
                ? null
                : DTOMapper.Mapper.Map<User>(user);
        }

    }
}
