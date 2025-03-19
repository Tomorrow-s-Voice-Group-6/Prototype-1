using System.ComponentModel.DataAnnotations;

namespace TVAttendance.Models
{
    public enum AttendanceReason
    {
        [Display(Name = "Family Emergency")]
        FamilyEmergency,
        [Display(Name = "Work Obligations")]
        WorkObligations,
        Illness,
        [Display(Name = "Prior Commitment")]
        PriorCommitment,
        Transportation,
        [Display(Name = "Relative Passed")]
        RelativePassed,
        Other
    }
}
