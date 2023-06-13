using Olsson.GET.Common.DataContracts.Users;
using Olsson.GET.Managers;
using Olsson.GET.Managers.Authentication;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Olsson.GET.Common.AspIdentity
{
    public class CustomRoleStore : IRoleStore<ApplicationRole>
    {
        private readonly IAuthenticationManager _userManager = new ManagerFactory().CreateManager<IAuthenticationManager>();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Task<ApplicationRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return
               Task.Run(
                   () => ConvertToApplicationRole(_userManager.FindRoleById(int.Parse(roleId))), cancellationToken);
        }

        public Task<ApplicationRole> FindByNameAsync(string roleName, CancellationToken cancellationToken)
        {
            return null;
        }

        public Task<IdentityResult> UpdateAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            return;
        }

        private ApplicationRole ConvertToApplicationRole(Role role)
        {
            return role == null
                ? null
                : DTOMapper.Mapper.Map<ApplicationRole>(role);
        }

        private Role ConvertToRole(ApplicationRole role)
        {
            return role == null
                ? null
                : DTOMapper.Mapper.Map<Role>(role);
        }


        public Task<IdentityResult> CreateAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> DeleteAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRoleIdAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetRoleNameAsync(ApplicationRole role, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedRoleNameAsync(ApplicationRole role, string normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
