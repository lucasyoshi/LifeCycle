#nullable disable
using SalesAndReturns.BLL;
using SalesAndReturns.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

namespace eBikes.Pages.Sales
{
    public partial class Returns
    {
        #region Injections
        [Inject]
        protected SalesAndReturnsServices? SalesAndReturnService { get; set; }
        #endregion
        #region Fields
        private string feedback { get; set; }
        private int? SaleID { get; set; } 

        protected List<SaleRefundDetailView> returnDetails { get; set; }
        #endregion

        private async Task GetSaleToReturn()
        {
            try
            {
                returnDetails = SalesAndReturnService.GetSaleDetailsRefund((int)SaleID);

                await InvokeAsync(StateHasChanged);
            }
            #region catch all exceptions
            catch (AggregateException ex)
            {
                foreach (var error in ex.InnerExceptions)
                {
                    feedback = error.Message;
                }
            }

            catch (ArgumentNullException ex)
            {
                feedback = GetInnerException(ex).Message;
            }

            catch (Exception ex)
            {
                feedback = GetInnerException(ex).Message;
            }
            #endregion
        }
        #region Methods
        private Exception GetInnerException(Exception ex)
        {
            while (ex.InnerException != null)
                ex = ex.InnerException;
            return ex;
        }
        #endregion
    }
}
