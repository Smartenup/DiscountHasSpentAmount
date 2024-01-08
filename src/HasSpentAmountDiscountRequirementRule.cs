using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Plugins;
using Microsoft.AspNetCore.Mvc;

namespace Nop.Plugin.DiscountRules.HasSpentAmount


{
    public partial class HasSpentAmountDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
    {
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IOrderService _orderService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductService _productServise;
        protected readonly IDiscountService _discountService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IWebHelper _webHelper;
        private readonly IActionContextAccessor _actionContextAccessor;

        public HasSpentAmountDiscountRequirementRule(ISettingService settingService,
            IOrderService orderService, IPriceCalculationService priceCalculationService,
            ILocalizationService localizationService, IShoppingCartService shoppingCartService, IProductService productServise,
            IDiscountService discountService, IUrlHelperFactory urlHelperFactory,
            IWebHelper webHelper, IActionContextAccessor actionContextAccessor)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _orderService = orderService;
            _priceCalculationService = priceCalculationService;
            _shoppingCartService = shoppingCartService;
            _productServise = productServise;
            _discountService = discountService;
            _actionContextAccessor = actionContextAccessor;
            _urlHelperFactory = urlHelperFactory;
            _webHelper = webHelper;
        }

        /// <summary>
        /// Check discount requirement
        /// </summary>
        /// <param name="request">Object that contains all information required to check the requirement (Current customer, discount, etc)</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(DiscountRequirementValidationRequest request)
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

            IList<ShoppingCartItem> cart = await _shoppingCartService.GetShoppingCartAsync(request.Customer, ShoppingCartType.ShoppingCart, request.Store.Id);

            foreach (var item in cart)
            {
                var product = await _productServise.GetProductByIdAsync(item.ProductId);
                var finalPrice = (await _priceCalculationService.GetFinalPriceAsync(product, request.Customer, request.Store)).finalPrice;
                spentAmount += finalPrice;
            }

            if (spentAmount > spentAmountRequirement)
            {
                result.IsValid = true;
            }
            else
            {
                result.UserError = await _localizationService.GetResourceAsync("Plugins.DiscountRules.HasSpentAmount.NotEnough");                         
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
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            return urlHelper.Action("Configure", "DiscountRulesHasSpentAmount",
                new { discountId = discountId, discountRequirementId = discountRequirementId }, _webHelper.GetCurrentRequestProtocol());

            ////configured in RouteProvider.cs
            //string result = "Plugins/DiscountRulesHasSpentAmount/Configure/?discountId=" + discountId;
            //if (discountRequirementId.HasValue)
            //    result += string.Format("&discountRequirementId={0}", discountRequirementId.Value);
            //return result;
        }

            public override async Task InstallAsync()
        {
            
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.DiscountRules.HasSpentAmount.Fields.Amount"] = "Required spent amount",
                ["Plugins.DiscountRules.HasSpentAmount.Fields.Amount.Hint"] = "Discount will be applied if customer has spent/purchased x.xx amount.",
                ["Plugins.DiscountRules.HasSpentAmount.NotEnough"] = "Sorry, this offer requires more money spent"
                
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //discount requirements
            var discountRequirements = (await _discountService.GetAllDiscountRequirementsAsync())
                .Where(discountRequirement => discountRequirement.DiscountRequirementRuleSystemName == DiscountRequirementDefaults.SystemName);
            foreach (var discountRequirement in discountRequirements)
            {
                await _discountService.DeleteDiscountRequirementAsync(discountRequirement, false);
            }

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.DiscountRules.HasSpentAmount");

            await base.UninstallAsync();
        }


    }
}
