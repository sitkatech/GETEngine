using Microsoft.AspNet.Identity;
using Olsson.GET.Common.DataContracts.Users;
using Olsson.GET.Managers;
using Olsson.GET.Managers.Authentication;
using System;
using System.Threading.Tasks;

namespace Olsson.GET.Common.AspIdentity
{
    public class CustomRoleStore : IRoleStore<ApplicationRole, int>
    {
        private readonly IAuthenticationManager _userManager = new ManagerFactory().CreateManager<IAuthenticationManager>();

        public Task CreateAsync(ApplicationRole role)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(ApplicationRole role)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Task<ApplicationRole> FindByIdAsync(int roleId)
        {
            return
               Task.Run(
                   () => ConvertToApplicationRole(_userManager.FindRoleById(roleId)));
        }

        public Task<ApplicationRole> FindByNameAsync(string roleName)
        {
            return null;          
        }

        public Task UpdateAsync(ApplicationRole role)
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
    }
}
