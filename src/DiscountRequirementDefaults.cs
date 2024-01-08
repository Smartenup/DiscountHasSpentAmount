namespace Nop.Plugin.DiscountRules.HasSpentAmount
{
    /// <summary>
    /// Represents defaults for the discount requirement rule
    /// </summary>
    public static class DiscountRequirementDefaults
    {
        /// <summary>
        /// The system name of the discount requirement rule
        /// </summary>
        public static string SystemName => "DiscountRequirement.HasSpentAmount";

        /// <summary>
        /// The key of the settings to save restricted customer roles
        /// </summary>
        public static string SettingsKey => "DiscountRequirement.HasSpentAmount-{0}";

        /// <summary>
        /// The HTML field prefix for discount requirements
        /// </summary>
        public static string HtmlFieldPrefix => "DiscountRulesHasSpentAmount{0}";
    }
}