#nullable disable
using SalesAndReturns.BLL;
using SalesAndReturns.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Forms;
using Servicing.Entities;

namespace eBikes.Pages.Sales
{
    public partial class Sales
    {
        #region Injections
        [Inject]
        protected SalesAndReturnsServices? SalesAndReturnService { get; set; }
        #endregion
        #region Fields

        private const decimal GST = (decimal)0.05;

        private int InvoiceNumber { get; set; }

        private int CategoryID { get; set; }

        private int ItemID { get; set; }

        private int Quantity { get; set; }

        private string Coupon { get; set; }

        private bool CouponAttempted { get; set; } = false;

        private int CouponDiscount { get; set; }

        private decimal totalDiscount { get; set; }

        private List<CategoryView> categoriesList { get; set; }
        private List<PartView> itemsList { get; set; }

        private SalesView sale { get; set; } = new();

        protected List<SaleDetailView> cart { get; set; } = new();

        private string feedBackMessage { get; set; }

        private string errorMessage { get; set; }
        private bool hasError => !string.IsNullOrWhiteSpace(errorMessage) || errorDetails.Count() > 0;
        private bool hasFeedBack => !string.IsNullOrWhiteSpace(feedBackMessage);
        private List<string> errorDetails { get; set; } = new();


        #endregion
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            categoriesList = SalesAndReturnService.GetCategoryList();
            itemsList = SalesAndReturnService.GetPartList(CategoryID);
            
        }
        private async Task addItem()
        {
            try
            {
                if (CategoryID > 0 && ItemID > 0 && Quantity > 0)
                {
                    foreach (var item in itemsList)
                    {

                        if (ItemID == item.PartID)
                        {
                            SaleDetailView itemToAdd = new();
                            itemToAdd.PartID = item.PartID;
                            itemToAdd.SellingPrice = item.SellingPrice;
                            itemToAdd.Quantity = Quantity;
                            itemToAdd.Description = item.Description;
                            itemToAdd.Total = item.SellingPrice * Quantity - (item.SellingPrice * Quantity) * ((decimal)CouponDiscount / (decimal)100.00);

                            cart.Add(itemToAdd);

                            sale.SubTotal += itemToAdd.Total;
                            sale.TaxAmount += itemToAdd.Total * GST;

                            totalDiscount += (item.SellingPrice * Quantity) * ((decimal)CouponDiscount / (decimal)100.0);

                        }
                    }
                    CategoryID = 0;
                    ItemID = 0;
                    Quantity = 0;
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

        private async Task removeItem(SaleDetailView itemToRemove)
        {
            try
            {
                cart.Remove(itemToRemove);
                updateItems();
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

        private async Task updateItems()
        {
            try
            {
                sale = new();
                foreach(var item in cart)
                {
                    item.Total = (item.SellingPrice * item.Quantity) - ((item.SellingPrice * item.Quantity)) * (CouponDiscount / (decimal)100.0);
                    
                    sale.SubTotal += item.Total;
                    sale.TaxAmount += item.Total * GST;

                    totalDiscount += (item.SellingPrice * item.Quantity) * (CouponDiscount / (decimal)100.0);
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
        private async Task clear()
        {
            try
            {
                sale = new();
                cart = new();
                CategoryID = 0;
                ItemID = 0;
                Quantity = 0;
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

        private async Task checkCoupon()
        {
            try
            {
                CouponDiscount = SalesAndReturnService.GetCoupon(Coupon);
                CouponAttempted = true;
                updateItems();
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

        private async Task checkout()
        {
            try
            {
                sale.EmployeeID = 1;
                InvoiceNumber = SalesAndReturnService.Checkout(sale, cart);
                feedBackMessage = "Order Succesfully Checked Out";
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

        private void getItemList()
        {
            try
            {
                itemsList = SalesAndReturnService.GetPartList(CategoryID);
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
        private bool isCategoryChosen()
        {
            if (CategoryID > 0)
            {
                getItemList();
                return true;
            }
            else
            {
                return false;
            }
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
