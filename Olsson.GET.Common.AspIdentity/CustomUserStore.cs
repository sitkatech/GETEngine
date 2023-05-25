using Microsoft.AspNet.Identity;
using Olsson.GET.Common.DataContracts.Users;
using Olsson.GET.Managers;
using Olsson.GET.Managers.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olsson.GET.Common.AspIdentity
{
    public class CustomUserStore : IUserStore<ApplicationUser, int>,
                                   IUserPasswordStore<ApplicationUser, int>,
                                   IUserEmailStore<ApplicationUser, int>,
                                   IUserLockoutStore<ApplicationUser, int>,
                                   IUserTwoFactorStore<ApplicationUser, int>,
                                   IUserPhoneNumberStore<ApplicationUser, int>,
                                   IUserLoginStore<ApplicationUser, int>,
                                   IUserRoleStore<ApplicationUser, int>,
                                   IUserSecurityStampStore<ApplicationUser, int>
    {
        private readonly IAuthenticationManager _userManager = new ManagerFactory().CreateManager<IAuthenticationManager>();

        public Task CreateAsync(ApplicationUser user)
        {
            return Task.Run(() => _userManager.CreateUser(ConvertToUser(user)));
        }

        public Task DeleteAsync(ApplicationUser user)
        {
            return Task.Run(() => _userManager.DeleteUser(ConvertToUser(user)));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Task<ApplicationUser> FindByEmailAsync(string email)
        {
            return
                Task.Run(
                    () =>
                    ConvertToApplicationUser(_userManager.FindUserByUserName(email)));
        }

        public Task<ApplicationUser> FindByIdAsync(int userId)
        {
            return
                Task.Run(
                    () => ConvertToApplicationUser(_userManager.FindUserById(userId)));
        }

        public Task<ApplicationUser> FindByNameAsync(string userName)
        {
            return
                Task.Run(
                    () =>
                    ConvertToApplicationUser(
                        _userManager.FindUserByUserName(userName)));
        }

        public Task<int> GetAccessFailedCountAsync(ApplicationUser user)
        {
            return Task.FromResult(user.FailedAttemptCount);
        }

        public Task<string> GetEmailAsync(ApplicationUser user)
        {
            return Task.FromResult(user.UserName);
        }

        public Task<bool> GetEmailConfirmedAsync(ApplicationUser user)
        {
            return Task.FromResult(true);
        }

        public Task<bool> GetLockoutEnabledAsync(ApplicationUser user)
        {
            return Task.FromResult(true);
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(ApplicationUser user)
        {
            return Task.FromResult(user.LockoutExpiration ?? DateTimeOffset.MinValue);
        }

        public Task<string> GetPasswordHashAsync(ApplicationUser user)
        {
            return Task.FromResult(user.Password);
        }

        public Task<string> GetPhoneNumberAsync(ApplicationUser user)
        {
            return Task.FromResult(string.Empty);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(ApplicationUser user)
        {
            return Task.FromResult(true);
        }

        public Task<string> GetSecurityStampAsync(ApplicationUser user)
        {
            return Task.FromResult(user.SecurityStamp);
        }

        public Task<bool> GetTwoFactorEnabledAsync(ApplicationUser user)
        {
            return Task.FromResult(false);
        }

        public Task<bool> HasPasswordAsync(ApplicationUser user)
        {
            return Task.FromResult(!string.IsNullOrEmpty(user.Password));
        }

        public Task<int> IncrementAccessFailedCountAsync(ApplicationUser user)
        {
            return Task.FromResult(user.FailedAttemptCount++);
        }

        public Task ResetAccessFailedCountAsync(ApplicationUser user)
        {
            user.FailedAttemptCount = 0;

            return Task.FromResult(0);
        }

        public Task SetEmailAsync(ApplicationUser user, string email)
        {
            user.UserName = email;
            return Task.FromResult(0);
        }

        public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed)
        {
            return Task.FromResult(0);
        }

        public Task SetLockoutEnabledAsync(ApplicationUser user, bool enabled)
        {
            return Task.FromResult(0);
        }

        public Task SetLockoutEndDateAsync(ApplicationUser user, DateTimeOffset lockoutEnd)
        {
            user.LockoutExpiration = lockoutEnd;

            return Task.FromResult(0);
        }

        public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash)
        {
            user.Password = passwordHash;

            return Task.FromResult(0);
        }

        public Task SetPhoneNumberAsync(ApplicationUser user, string phoneNumber)
        {
            return Task.FromResult(0);
        }

        public Task SetPhoneNumberConfirmedAsync(ApplicationUser user, bool confirmed)
        {
            return Task.FromResult(0);
        }

        public Task SetSecurityStampAsync(ApplicationUser user, string stamp)
        {
            user.SecurityStamp = stamp;

            return Task.FromResult(0);
        }

        public Task SetTwoFactorEnabledAsync(ApplicationUser user, bool enabled)
        {
            return Task.FromResult(0);
        }

        public Task UpdateAsync(ApplicationUser user)
        {
            return Task.Run(() => _userManager.UpdateUser(ConvertToUser(user)));
        }

        public Task<bool> GetTwoFactorEnabledAsync(User user)
        {
            return Task.FromResult(false);
        }

        public Task SetTwoFactorEnabledAsync(User user, bool enabled)
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            return;
        }

        private ApplicationUser ConvertToApplicationUser(User user)
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

        public Task<ApplicationUser> FindByIdAsync(string userId)
        {
            return
               Task.Run(
                   () => ConvertToApplicationUser(_userManager.FindUserById(int.Parse(userId))));
        }

        public Task AddLoginAsync(ApplicationUser user, UserLoginInfo login)
        {
            throw new NotImplementedException();
        }

        public Task RemoveLoginAsync(ApplicationUser user, UserLoginInfo login)
        {
            throw new NotImplementedException();
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user)
        {
            return Task.Run(() => (IList<UserLoginInfo>)new List<UserLoginInfo>() { });
        }

        public Task<ApplicationUser> FindAsync(UserLoginInfo login)
        {
            throw new NotImplementedException();
        }

        public Task AddToRoleAsync(ApplicationUser user, string roleName)
        {
            return Task.Run(() => { _userManager.AddRoleToUser(ConvertToUser(user), roleName); });
        }

        public Task RemoveFromRoleAsync(ApplicationUser user, string roleName)
        {
            return Task.Run(() => { _userManager.RemoveUserFromRole(ConvertToUser(user), roleName); });
        }

        public Task<IList<string>> GetRolesAsync(ApplicationUser user)
        {
            return Task.Run(() =>
            {
                IList<string> ret = _userManager.GetUserRoles(user.UserID).Select(r => r.RoleName).ToList();
                return ret;
            });
        }

        public Task<bool> IsInRoleAsync(ApplicationUser user, string roleName)
        {
            return Task.Run(() => { return _userManager.IsUserInRole(ConvertToUser(user), roleName); });
        }
    }
}
