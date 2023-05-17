using System.Runtime.Serialization;

namespace Olsson.GET.Common.DataContracts.Users
{
    [DataContract]
    public class Role
    {
        [DataMember]
        public int RoleID { get; set; }

        [DataMember]
        public string RoleName { get; set; }

        [DataMember]
        public string RoleDisplayName { get; set; }

        [DataMember]
        public RoleType RoleCategory { get; set; }
    }

    public enum RoleType
    {
        Admin = 1
    }
}
