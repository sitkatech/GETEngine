using Microsoft.AspNet.Identity;
using Olsson.GET.Common.DataContracts.Users;
using Olsson.GET.Managers;
using Olsson.GET.Managers.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Olsson.GET.Common.AspIdentity
{
    public class ApplicationUser : User, IUser<int>
    {
        private readonly IAuthenticationManager _userManager = new ManagerFactory().CreateManager<IAuthenticationManager>();

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, int> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            var userId = userIdentity.GetUserId<int>();

            var userRoles = _userManager.GetUserRoles(userId);

            foreach (var role in userRoles)
            {
                userIdentity.AddClaim(new Claim(ClaimTypes.Role, role.RoleName));
            }

            // Add custom user claims here
            return userIdentity;
        }
    }
}
