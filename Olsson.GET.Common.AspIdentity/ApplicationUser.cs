using Olsson.GET.Common.DataContracts.Users;
using Olsson.GET.Managers;
using Olsson.GET.Managers.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Olsson.GET.Common.AspIdentity
{
    public class ApplicationUser : User
    {
        private readonly IAuthenticationManager _userManager = new ManagerFactory().CreateManager<IAuthenticationManager>();
    }
}
