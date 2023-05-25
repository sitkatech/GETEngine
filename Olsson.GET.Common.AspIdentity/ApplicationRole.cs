using Microsoft.AspNet.Identity;

namespace Olsson.GET.Common.AspIdentity
{
    public class ApplicationRole : IRole<int>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
