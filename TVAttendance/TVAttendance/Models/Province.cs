using System.ComponentModel.DataAnnotations;

namespace TVAttendance.Models
{
    public enum Province
    {
        Alberta,
        [Display(Name = "British Columbia")]
        BritishColumbia,
        Manitoba,
        [Display(Name = "New Brunswick")]
        NewBrunswick,
        [Display(Name = "New Foundland")]
        NewFoundland,
        [Display(Name = "Nova Scotia")]
        NovaScotia,
        Nunavut,
        [Display(Name = "North West Territories")]
        NWTerritories,
        Ontario,
        [Display(Name = "Prince Edward Island")]
        PrinceEdwardIsland,
        Quebec,
        Saskatchewan,
        Yukon,
        [Display(Name = "Newfoundland And Labrador")]
        NewfoundlandAndLabrador
    }


}
