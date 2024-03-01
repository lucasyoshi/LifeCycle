using Microsoft.AspNetCore.Components;
using Receiving.BLL;
using Receiving.Entities;
using Receiving.ViewModels;

namespace eBikes.Pages.Receiving
{
    public partial class Receiving
    {
        #region Injection
        [Inject]
        protected ReceivingServices? ReceivingServices { get; set; }
        #endregion

        #region Fields
        private int employeeId { get; set; } = 5;
        private int purchaseOrderID { get; set; }
        private string feedback { get; set; }
        private string goodFeedback { get; set; }
        private string reasonForClosing { get; set; } = "";
        private bool hasPurchaseOrderID { get; set; } = true;
        private bool hasError => !string.IsNullOrWhiteSpace(feedback);
        private bool hasFeedBack => !string.IsNullOrWhiteSpace(goodFeedback);
        private List<string> errorDetails { get; set; } = new();

        private UnorderedItemView unorderedItem = new UnorderedItemView();
        #endregion

        protected List<OutstandingOrderView> outstandingOrderViews { get; set; } = new ();
        protected List<PurchaseOrderDetailsView> poDetails { get; set; } = new ();
        protected List<UnorderedItemView> unorderedItems { get; set; } = new ();
        
        #region Methods

        protected override async Task OnInitializedAsync()
        {
            try
            {
                outstandingOrderViews = ReceivingServices.GetOutstandingOrders();
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

        private async Task GetOrderDetails(int purchaseOrderId)
        {
            try
            {
                feedback = "";
                goodFeedback = "";
                errorDetails.Clear();
                poDetails = ReceivingServices.GetOrderDetails(purchaseOrderId);
                purchaseOrderID = purchaseOrderId;
                if (purchaseOrderID > 0 || purchaseOrderID != null)
                {
                    hasPurchaseOrderID = false;
                }
                await GetUnorderedItems();
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

        private async Task GetUnorderedItems()
        {
            try
            {
                unorderedItems = ReceivingServices.GetUnorderedItems();
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

        private async Task ForceClose()
        {
            try
            {
                feedback = "";
                goodFeedback = "";
                errorDetails.Clear();
                ReceivingServices.ForceClose(purchaseOrderID, reasonForClosing);
                goodFeedback = "Order was force closed with success";
                outstandingOrderViews = ReceivingServices.GetOutstandingOrders();
                await GetUnorderedItems();
            }
            #region catch all exceptions
            catch (AggregateException ex)
            {
                feedback = "Unable to force close";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
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

        private async Task Receive()
        {
            try
            {
                feedback = "";
                goodFeedback = "";
                errorDetails.Clear();
                int count = 0;

                foreach (var item in poDetails)
                {
                    if (item.ReceivedQty == 0 && item.ReturnQty == 0)
                    {
                        count++;
                    }
                }
                if (count == poDetails.Count())
                {
                    throw new ArgumentNullException("Can't receive order with no received quantity and return quantity.");
                }

                ReceivingServices.Receive(purchaseOrderID, employeeId, poDetails, unorderedItems);
                goodFeedback = "Order was received";
                outstandingOrderViews = ReceivingServices.GetOutstandingOrders();
                poDetails = ReceivingServices.GetOrderDetails(purchaseOrderID);
                await GetUnorderedItems();
                await InvokeAsync(StateHasChanged);

            }
            #region catch all exceptions
            catch (AggregateException ex)
            {
                feedback = "Unable to receive order.";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
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

        private async Task Add()
        {
            try
            {
                feedback = "";
                goodFeedback = "";
                errorDetails.Clear();
                ReceivingServices.AddUnorderedItem(unorderedItem);
                goodFeedback = "Unordered Item was ADDED to the Cart.";
                await GetUnorderedItems();
                Clear();

            }
            #region catch all exceptions
            catch (AggregateException ex)
            {
                feedback = "Unable to Add new unordered item";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
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
        private async Task Delete(int cartId)
        {
            try
            {
                feedback = "";
                goodFeedback = "";
                errorDetails.Clear();
                ReceivingServices.DeleteUnorderedItem(cartId);
                goodFeedback = "Unordered Item was DELETED to the Cart.";
                await GetUnorderedItems();
            }
            #region catch all exceptions
            catch (AggregateException ex)
            {
                feedback = "Unable to delete unordered item";
                foreach (var error in ex.InnerExceptions)
                {
                    errorDetails.Add(error.Message);
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
        private void Clear()
        {
            unorderedItem.ItemDescription = "";
            unorderedItem.VendorPartNumber = "";
            unorderedItem.Quantity = 0;
        }
        private Exception GetInnerException(Exception ex)
        {
            while (ex.InnerException != null)
                ex = ex.InnerException;
            return ex;
        }

        #endregion
    }
}
