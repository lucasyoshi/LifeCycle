#nullable disable

using Receiving.DAL;
using Receiving.Entities;
using Receiving.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Receiving.BLL
{
    public class ReceivingServices
    {
        #region DAL Context
        private readonly ReceivingContext _receivingContext;
        #endregion

        internal ReceivingServices (ReceivingContext receivingContext)
        {
            _receivingContext = receivingContext;
        }
        #region Query Methods
        public List<OutstandingOrderView> GetOutstandingOrders()
        {
            return _receivingContext.PurchaseOrders
            .Where(x => x.Closed == false)
            .Select(x => new OutstandingOrderView
            {
                PurchaseOrderID = x.PurchaseOrderID,
                OrderDate = x.OrderDate,
                VendorName = x.Vendor.VendorName,
                VendorContact = x.Vendor.Phone
            }).ToList();
        }

        public List<PurchaseOrderDetailsView> GetOrderDetails(int PurchaseOrderID)
        {
            return _receivingContext.PurchaseOrderDetails
            .Where(x => x.PurchaseOrderID == PurchaseOrderID)
            .Select(x => new PurchaseOrderDetailsView
            {
                PurchaseOrderDetailID = x.PurchaseOrderDetailID,
                PartID = x.PartID,
                PartDescription = x.Part.Description,
                OriginalQty = x.Quantity,
                OutstandingQty = x.Quantity - x.ReceiveOrderDetails.Sum(r => r.QuantityReceived)
            }).ToList();
        }

        public List<UnorderedItemView> GetUnorderedItems()
        {
            return _receivingContext.UnorderedPurchaseItemCarts
            .Select(x => new UnorderedItemView
            {
                CartId = x.CartID,
                ItemDescription = x.Description,
                VendorPartNumber = x.VendorPartNumber,
                Quantity = x.Quantity
            }).ToList();
        }
        #endregion

        public void Receive(int purchaseOrderID, int employeeID, List<PurchaseOrderDetailsView> outstandingItems,
 List<UnorderedItemView> unorderedCart)
        {
            List<Exception> errorList = new();
            ReceiveOrder receive = new();

            // Receive Order
            if (purchaseOrderID <= 0)
            {
                throw new ArgumentNullException($"Purchase Order ID cannot be 0. ID: {purchaseOrderID}");
            }
            if (employeeID <= 0)
            {
                throw new ArgumentNullException($"Employee ID cannot be 0. ID: {employeeID}");
            }

            var poExists = _receivingContext.PurchaseOrders.FirstOrDefault(x => x.PurchaseOrderID == purchaseOrderID);
            var employeeExists = _receivingContext.Employees.FirstOrDefault(e => e.EmployeeID == employeeID);

            if (employeeExists == null)
            {
                errorList.Add(new Exception($"Employee does not exist in the database. Employee ID: {employeeID}"));
            }

            if (poExists != null)
            {
                receive.PurchaseOrderID = purchaseOrderID;
                receive.ReceiveDate = DateTime.Now;
                receive.EmployeeID = employeeID;

                _receivingContext.ReceiveOrders.Add(receive);
            }
            else
            {
                errorList.Add(new Exception($"Purchase Order ID does not exist in the database. PO ID: {purchaseOrderID}"));
            }

            // Receive Order Details
            var receiveOrderID = _receivingContext.ReceiveOrders.OrderByDescending(x => x.ReceiveOrderID).Where(x => x.PurchaseOrderID == purchaseOrderID).Select(x => x.ReceiveOrderID).FirstOrDefault();

            if (outstandingItems.Count() <= 0)
            {
                throw new ArgumentNullException($"List of items are empty.");
            }

            if (outstandingItems.Sum(x => x.ReceivedQty) == 0)
            {
                errorList.Add(new Exception("No record was added to the Received Order Details"));
            }
            foreach (var item in outstandingItems)
            {
                ReceiveOrderDetail roDetails = new();

                if (item.ReceivedQty > 0)
                {
                    roDetails.ReceiveOrderID = receiveOrderID;
                    roDetails.PurchaseOrderDetailID = item.PurchaseOrderDetailID;
                    roDetails.QuantityReceived = item.ReceivedQty;


                    receive.ReceiveOrderDetails.Add(roDetails);
                }
                else if (item.ReceivedQty < 0)
                {
                    errorList.Add(new Exception($"{item.PartDescription} cannot be less than zero."));
                }

            }

            // Returned Order Details
            if (unorderedCart.Any())
            {
                foreach (var item in unorderedCart)
                {
                    ReturnedOrderDetail returnItem = new();

                    if (string.IsNullOrEmpty(item.ItemDescription))
                    {
                        throw new ArgumentNullException($"Item Description must not be empty.");
                    }
                    if (item.Quantity <= 0)
                    {
                        throw new ArgumentNullException($"Quantity must non-zero positive value. Qty: {item.Quantity}");
                    }
                    if (string.IsNullOrEmpty(item.VendorPartNumber))
                    {
                        throw new ArgumentNullException("Vendor Part Number must not be empty.");
                    }

                    returnItem.ReceiveOrderID = receiveOrderID;
                    returnItem.ItemDescription = item.ItemDescription;
                    returnItem.Quantity = item.Quantity;
                    returnItem.Reason = "Not requested";
                    returnItem.VendorPartNumber = item.VendorPartNumber;

                    receive.ReturnedOrderDetails.Add(returnItem);
                }
            }
            if (outstandingItems.Any())
            {
                foreach (var item in outstandingItems)
                {
                    ReturnedOrderDetail returnItem = new();
                    if (item.ReturnQty < 0)
                    {
                        errorList.Add(new Exception($"Return quantity {item.PartDescription} cannot be less than zero {item.ReturnQty}"));
                    }
                    else if (item.ReturnQty > 0)
                    {
                        if (string.IsNullOrWhiteSpace(item.ReturnReason))
                        {
                            errorList.Add(new Exception($"Record must contain a commented reason on why it's being returned. {item.PartDescription}"));
                        }
                        else
                        {
                            returnItem.ReceiveOrderID = receiveOrderID;
                            returnItem.ItemDescription = item.PartDescription;
                            returnItem.PurchaseOrderDetailID = item.PurchaseOrderDetailID;
                            returnItem.Quantity = item.ReturnQty;
                            returnItem.Reason = item.ReturnReason;
                        }
                        receive.ReturnedOrderDetails.Add(returnItem);
                    }
                }
            }

            // Update Parts
            if (outstandingItems.Count() == 0)
            {
                throw new ArgumentNullException("There are no items to be updated in the Parts table.");
            }

            foreach (var item in outstandingItems)
            {

                if (item.ReceivedQty > _receivingContext.Parts.Where(x => x.PartID == item.PartID).Select(x => x.QuantityOnOrder).FirstOrDefault())
                {
                    errorList.Add(new Exception("Received Quantity cannot be greater than the Quantity on Order."));
                }
                if (item.ReceivedQty != 0)
                {
                    var updatedPart = _receivingContext.Parts.FirstOrDefault(x => x.PartID == item.PartID);

                    updatedPart.QuantityOnHand = updatedPart.QuantityOnHand + item.ReceivedQty;
                    updatedPart.QuantityOnOrder = updatedPart.QuantityOnOrder - item.ReceivedQty;
                }
            }

            // Close Order
            int count = 0;

            foreach (var item in outstandingItems)
            {
                if (item.OutstandingQty - item.ReceivedQty == 0)
                {
                    count++;
                }
            }

            if (count == outstandingItems.Count())
            {
                var closeOrder = _receivingContext.PurchaseOrders
                                .Where(x => x.PurchaseOrderID == purchaseOrderID)
                                .FirstOrDefault();
                closeOrder.Closed = true;
                closeOrder.Notes = "All items received";
            }

            var cart = _receivingContext.UnorderedPurchaseItemCarts.Select(x => x).ToList();
            _receivingContext.UnorderedPurchaseItemCarts.RemoveRange(cart);

            if (errorList.Count() > 0)
            {
                throw new AggregateException("Unable to add the Receive Order, check concerns", errorList);
            }
            else
            {
                _receivingContext.SaveChanges();
            }
        }
        public void ForceClose(int purchaseOrderId, string closeReason)
        {
            List<Exception> errorList = new List<Exception>();


            if (string.IsNullOrWhiteSpace(closeReason))
            {
                throw new ArgumentNullException("Reason for closing is missing");
            }

            var closingPO = _receivingContext.PurchaseOrders
                            .Where(po => po.PurchaseOrderID == purchaseOrderId)
                            .FirstOrDefault();
            if (closingPO == null)
            {
                errorList.Add(new Exception("There is no purchase order for this ID."));
            }

            var poDetails = _receivingContext.PurchaseOrderDetails
                            .Where(pod => pod.PurchaseOrderID == purchaseOrderId)
                            .Select(pod => pod)
                            .ToList();


            foreach (var item in poDetails)
            {
                var part = _receivingContext.Parts.Where(x => x.PartID == item.PartID).FirstOrDefault();

                var partqty = part.QuantityOnOrder;

                if (part.QuantityOnOrder < (item.Quantity - item.ReceiveOrderDetails.Sum(x => x.QuantityReceived)))
                {

                    errorList.Add(new Exception($"Quantity on Order (Parts) can not be lower than the Outstanding Quantity ({item.Part.Description})"));
                }
                else
                {
                    part.QuantityOnOrder = part.QuantityOnOrder - (item.Quantity - item.ReceiveOrderDetails.Sum(x => x.QuantityReceived));
                }
            }


            closingPO.Closed = true;
            closingPO.Notes = closeReason;

            if (errorList.Count() > 0)
            {
                throw new AggregateException("Unable to add the Force Close, check concerns", errorList);
            }
            else
            {
                var cart = _receivingContext.UnorderedPurchaseItemCarts.Select(x => x).ToList();
                _receivingContext.UnorderedPurchaseItemCarts.RemoveRange(cart);
                _receivingContext.SaveChanges();
            }
        }
        public void AddUnorderedItem(UnorderedItemView unorderedItem)
        {
            List<Exception> errorList = new List<Exception>();

            UnorderedPurchaseItemCart newItems = new();

            if (string.IsNullOrWhiteSpace(unorderedItem.ItemDescription))
            {
                throw new ArgumentNullException("Item Description is missing");
            }
            if (string.IsNullOrWhiteSpace(unorderedItem.VendorPartNumber))
            {
                throw new ArgumentNullException("Vendor Part Number is missing");
            }
            if (unorderedItem.Quantity == 0)
            {
                throw new ArgumentNullException("Quantity is missing");
            }
            if (unorderedItem.Quantity < 0)
            {
                throw new ArgumentNullException("Quantity cannot be less than zero");
            }

            newItems.Description = unorderedItem.ItemDescription.Trim();
            newItems.VendorPartNumber = unorderedItem.VendorPartNumber;
            newItems.Quantity = unorderedItem.Quantity;

            _receivingContext.UnorderedPurchaseItemCarts.Add(newItems);

            _receivingContext.SaveChanges();
        }
        public void DeleteUnorderedItem(int cartId)
        {
            var deleteItem = _receivingContext.UnorderedPurchaseItemCarts.Where(x => x.CartID == cartId).FirstOrDefault();

            _receivingContext.UnorderedPurchaseItemCarts.Remove(deleteItem);

            _receivingContext.SaveChanges();
        }
    }
}
