using Nop.Web.Framework;

namespace Nop.Plugin.DiscountRules.HasSpentAmount.Models
{
    public class RequirementModel
    {
        [NopResourceDisplayName("Plugins.DiscountRules.HasSpentAmount.Fields.Amount")]
        public decimal SpentAmount { get; set; }

        public int DiscountId { get; set; }

        public int RequirementId { get; set; }
    }
}