using System;
using System.Runtime.Serialization;

namespace Olsson.GET.Common.DataContracts.Users
{
    [DataContract]
    public class User
    {
        public int Id => UserID;
        [DataMember]
        public int UserID { get; set; }
        [DataMember]
        public string FullName { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public bool IsLockedOut { get; set; }
        [DataMember]
        public DateTimeOffset? LockoutExpiration { get; set; }
        [DataMember]
        public int FailedAttemptCount { get; set; }
        [DataMember]
        public string SecurityStamp { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public bool EmailConfirmed { get; set; }
        [DataMember]
        public int CustomerID { get; set; }
        [DataMember]
        public string PhoneNumber { get; set; }
        [DataMember]
        public DateTime? EulaAcceptedDate { get; set; }
    }
}
