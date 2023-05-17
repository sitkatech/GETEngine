using System.ComponentModel.DataAnnotations;

namespace Olsson.GET.Common.DataContracts.Models
{
    public enum ModelTypeEnum
    {
        [Display(Name = "Steady State")]
        SteadyState,
        [Display(Name = "Transient")]
        Transient
    }
}