using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Plugins;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Orders;
using System;
using System.Linq;

namespace Nop.Plugin.DiscountRules.HasSpentAmount
{
    public partial class HasSpentAmountDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
    {
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IOrderService _orderService;
        private readonly IPriceCalculationService _priceCalculationService;

        public HasSpentAmountDiscountRequirementRule(ISettingService settingService,
            IOrderService orderService, IPriceCalculationService priceCalculationService, ILocalizationService localizationService)
        {
            this._localizationService = localizationService;
            this._settingService = settingService;
            this._orderService = orderService;
            this._priceCalculationService = priceCalculationService;
        }

        /// <summary>
        /// Check discount requirement
        /// </summary>
        /// <param name="request">Object that contains all information required to check the requirement (Current customer, discount, etc)</param>
        /// <returns>Result</returns>
        public DiscountRequirementValidationResult CheckRequirement(DiscountRequirementValidationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            //invalid by default
            var result = new DiscountRequirementValidationResult();


            var spentAmountRequirement = _settingService.GetSettingByKey<decimal>(string.Format("DiscountRequirement.HasSpentAmount-{0}", request.DiscountRequirementId));

            if (spentAmountRequirement == decimal.Zero)
            {
                result.IsValid = true;
                return result;
            }

            if (request.Customer == null)
                return result;

            decimal spentAmount = 0;


            foreach (var item in request.Customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart))
	        {
		        spentAmount += _priceCalculationService.GetSubTotal(item, false);
	        }

            if (spentAmount > spentAmountRequirement)
            {
                result.IsValid = true;
            }
            else
            {
                result.UserError = _localizationService.GetResource("Plugins.DiscountRules.HasSpentAmount.NotEnough");
            }

            return result;
        }

        /// <summary>
        /// Get URL for rule configuration
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <param name="discountRequirementId">Discount requirement identifier (if editing)</param>
        /// <returns>URL</returns>
        public string GetConfigurationUrl(int discountId, int? discountRequirementId)
        {
            //configured in RouteProvider.cs
            string result = "Plugins/DiscountRulesHasSpentAmount/Configure/?discountId=" + discountId;
            if (discountRequirementId.HasValue)
                result += string.Format("&discountRequirementId={0}", discountRequirementId.Value);
            return result;
        }

        public override void Install()
        {
            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasSpentAmount.Fields.Amount", "Required spent amount");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasSpentAmount.Fields.Amount.Hint", "Discount will be applied if customer has spent/purchased x.xx amount.");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasSpentAmount.NotEnough", "Sorry, this offer requires more money spent");
            base.Install();
        }

        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.DiscountRules.HasSpentAmount.Fields.Amount");
            this.DeletePluginLocaleResource("Plugins.DiscountRules.HasSpentAmount.Fields.Amount.Hint");
            this.DeletePluginLocaleResource("Plugins.DiscountRules.HasSpentAmount.NotEnough");
            base.Uninstall();
        }
    }
}