#nullable disable

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

using Purchasing.BLL;
using Purchasing.ViewModels;
using Receiving.BLL;
using Receiving.Entities;
using Receiving.ViewModels;

namespace eBikes.Pages.Purchasing
{
    public partial class Purchasing
    {
        #region Inject

        [Inject]
        protected PurchasingServices PurchasingServices { get; set; }
        #endregion

        #region Fields

        private PurchaseOrderView purchaseOrder = new();

        protected List<VendorView> vendorsList{ get; set; } = new();

        private int vendorID { get; set; } = 0;
        private bool hasPurchaseOrder { get; set; } = false;

        private string feedBackMessage { get; set; }

        private string errorMessage { get; set; }
        private bool hasError => !string.IsNullOrWhiteSpace(errorMessage) || errorDetails.Count()>0;
        private bool hasFeedBack => !string.IsNullOrWhiteSpace(feedBackMessage);
        private List<string> errorDetails { get; set; } = new();

        protected VendorView selectedVendor => vendorsList.FirstOrDefault(v => v.VendorID == vendorID);
        #endregion
        protected List<ItemView> inventoryView { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                vendorsList = PurchasingServices.GetVendors();
                purchaseOrder.VendorID = 0;

                await InvokeAsync(StateHasChanged);
            }
            #region catch all exceptions
            catch (AggregateException ex)
            {

                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add( error.Message);
                }
            }

            catch (ArgumentNullException ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }

            catch (Exception ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
            #endregion
        }

        private async Task RemoveItem(int partID)
        {
            await PurchasingServices.RemoveItem(partID, purchaseOrder, inventoryView, "Remove");
        }
        private async Task AddItem(int partID)
        {
            await PurchasingServices.RemoveItem(partID, purchaseOrder, inventoryView, "Add");
            await InvokeAsync(StateHasChanged);

        }

        private async Task DisplayOrderAndInventory()
        {
            ClearFeedback();
            await GetPurchaseOrder();
            await GetInventory();
        }
        private async Task GetPurchaseOrder()
        {
            try
            {
                purchaseOrder = await PurchasingServices.GetPurchaseOrder(vendorID, purchaseOrder);
                await InvokeAsync(StateHasChanged);
                feedBackMessage = "PO loaded";
                if (purchaseOrder.PurchaseOrderNum > 0)
                {
                    hasPurchaseOrder = true;
                }
                else
                {
                    hasPurchaseOrder = false;
                }
            }
            #region catch all exceptions
            catch (AggregateException ex)
            {
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
                }
            }

            catch (ArgumentNullException ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }

            catch (Exception ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
            #endregion
        }
        private async Task GetInventory()
        {
            try
            {
                inventoryView = await PurchasingServices.GetInventory(vendorID, purchaseOrder.PurchaseOrderDetails);
                await InvokeAsync(StateHasChanged);

            }
            #region catch all exceptions
            catch (AggregateException ex)
            {
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);

                }
            }

            catch (ArgumentNullException ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }

            catch (Exception ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
            #endregion
        }

        private async Task SaveOrder()
        {
            try
            {
                ClearFeedback();

                PurchasingServices.SaveOrder(purchaseOrder, "save");
                await DisplayOrderAndInventory();

                feedBackMessage = "Order Saved";

            }

            #region catch all exceptions
            catch (AggregateException ex)
            {
                foreach (var error in ex.InnerExceptions)
                {
                   errorDetails.Add(error.Message);

                }
            }

            catch (ArgumentNullException ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }

            catch (Exception ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
            #endregion

        }

        private async Task PlaceOrder()
        {
            try
            {
                ClearFeedback();
                PurchasingServices.SaveOrder(purchaseOrder, "place");
                await DisplayOrderAndInventory();
                feedBackMessage = "Order Placed";
            }

            #region catch all exceptions
            catch (AggregateException ex)
            {
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);


                }
            }

            catch (ArgumentNullException ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }

            catch (Exception ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
            #endregion

        }

        private async Task DeleteOrder()
        {
            try
            {
                ClearFeedback();
                PurchasingServices.DeleteOrder(purchaseOrder);
                await DisplayOrderAndInventory();

                feedBackMessage = "Order Deleted";
            }

            #region catch all exceptions
            catch (AggregateException ex)
            {
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);

                }
            }

            catch (ArgumentNullException ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }

            catch (Exception ex)
            {
                errorMessage = GetInnerException(ex).Message;
            }
            #endregion

        }
       
        //FORM METHODS
        public async Task Refresh()
        {
            purchaseOrder.SubTotal = purchaseOrder.PurchaseOrderDetails.Sum(x => x.Price * x.QTO);
            purchaseOrder.GST = purchaseOrder.PurchaseOrderDetails.Sum(x => x.Price * x.QTO) * (decimal)0.05;
            purchaseOrder.Total = (purchaseOrder.PurchaseOrderDetails.Sum(x => x.Price * x.QTO) * (decimal)0.05) + purchaseOrder.PurchaseOrderDetails.Sum(x => x.Price * x.QTO);
        }
        public void ClearForm()
        {
            purchaseOrder = new();
            vendorID = 0;
            purchaseOrder.VendorID = 0;
            inventoryView = new();
            hasPurchaseOrder = false;
            ClearFeedback();    
            feedBackMessage = "Form Cleared";
        }
        private void ClearFeedback()
        {
            feedBackMessage = "";
            errorMessage = "";
            errorDetails.Clear();
        }

        #region ExceptionMethod
        private Exception GetInnerException(Exception ex)
        {
            while (ex.InnerException != null)
                ex = ex.InnerException;
            return ex;
        }
        #endregion
    }
}
