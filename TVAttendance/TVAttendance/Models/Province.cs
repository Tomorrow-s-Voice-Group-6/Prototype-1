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
        [Display(Name = "Newfoundland And Labrador")]
        NewfoundlandAndLabrador,
        [Display(Name = "Nova Scotia")]
        NovaScotia,
        Nunavut,
        [Display(Name = "Northwest Territories")]
        NWTerritories,
        Ontario,
        [Display(Name = "Prince Edward Island")]
        PrinceEdwardIsland,
        Quebec,
        Saskatchewan,
        Yukon
        
    }


}
