#nullable disable
using System.Security.Cryptography.X509Certificates;

using Purchasing.ViewModels;
using Purchasing.DAL;
using Purchasing.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Purchasing.BLL
{
    public class PurchasingServices
    {
        #region Fields

        private readonly PurchasingContext _purchasingContext;
        #endregion

        internal PurchasingServices(PurchasingContext purchasingContext)
        {
            _purchasingContext = purchasingContext;
        }

        //Query Services
        public List<VendorView> GetVendors()
        {
            return _purchasingContext.Vendors
                        .OrderBy(x=>x.VendorID)
                        .Select(x => new VendorView
                        {
                            VendorID = x.VendorID,
                            VendorName = x.VendorName,
                            HomePhone = x.Phone,
                            City = x.City

                        }).ToList();
        } 

        public async Task< List<ItemView>> GetInventory(int vendorID, List<PurchaseOrderDetailView> purchaseOrderDetails)
        {
            bool vendorExists = false;

            if (vendorID == 0)
            {
                throw new ArgumentNullException("Vendor ID is invalid");
            }

            vendorExists = _purchasingContext.Vendors
                            .Where(x => x.VendorID == vendorID).Any();
            var activeOrder = _purchasingContext.PurchaseOrders
                            .Where(x => x.VendorID == vendorID && x.OrderDate == null).FirstOrDefault();
           List<ItemView> updatedInventory = new();

            if (vendorExists)
            {
                if (activeOrder != null)
                { 
                    foreach (var item in _purchasingContext.Parts)
                    {
                        bool partInPO = true;

                        if (item.VendorID == vendorID)
                        {
                            foreach (var part in purchaseOrderDetails)
                            {
                                if (item.PartID == part.PartID)
                                {
                                    partInPO = true;
                                    break;
                                }
                                else
                                {
                                    partInPO = false;
                                }
                            }
                            if (!partInPO)
                            {
                                updatedInventory.Add(new ItemView()
                                {
                                    PurchaseOrderID = activeOrder.PurchaseOrderID,
                                    PurchaseOrderNum = activeOrder.PurchaseOrderNumber,
                                    PartID = item.PartID,
                                    PartDescription = item.Description,
                                    QOH = item.QuantityOnHand,
                                    ROL = item.ReorderLevel,
                                    QOO = item.QuantityOnOrder,
                                    Buffer = item.ReorderLevel - (item.QuantityOnHand + item.QuantityOnOrder),
                                    Price = item.PurchasePrice
                                });
                            }

                        }
                    }

                    return updatedInventory.OrderBy(x => x.PartID).ToList();
                }
                else
                {
                    foreach (var item in _purchasingContext.Parts)
                    {
                        if (item.VendorID == vendorID && item.ReorderLevel - (item.QuantityOnHand + item.QuantityOnOrder) <= 0)
                        {
                            updatedInventory.Add(new ItemView()
                            {
                                PartID = item.PartID,
                                PartDescription = item.Description,
                                QOH = item.QuantityOnHand,
                                ROL = item.ReorderLevel,
                                QOO = item.QuantityOnOrder,
                                Buffer = item.ReorderLevel - (item.QuantityOnHand + item.QuantityOnOrder),
                                Price = item.PurchasePrice
                            });
                        }
                    }

                    return updatedInventory.OrderBy(x=>x.PartID).ToList();
                }
            }

            else
            {
                throw new Exception(" Vendor does not exist!");
            }
        }

        public async Task< PurchaseOrderView> GetPurchaseOrder(int vendorID, PurchaseOrderView purchaseOrder)
        {
            bool activeOrderExist = false;
            bool vendorExsits = false;
            if (vendorID == 0)
            {
                throw new ArgumentNullException("Vendor ID provided is invalid");
            }

            vendorExsits = _purchasingContext.Vendors
                            .Where(x => x.VendorID == vendorID).Any();

            //Business Rule 
            //Check if an active order exits where order date is null
            //if yes then return the order
            //if no then return an empty order list with vendorId

            activeOrderExist = _purchasingContext.PurchaseOrders
                    .Where(x => x.VendorID == vendorID && x.OrderDate == null).Any();

            if (vendorExsits)
            {
                if (activeOrderExist)
                {
                    Console.WriteLine($"Active order exists");
                    return _purchasingContext.PurchaseOrders
                    .Where(x => x.VendorID == vendorID && x.OrderDate == null)
                    .Select(x => new PurchaseOrderView
                    {
                        PurchaseOrderID = x.PurchaseOrderID,
                        PurchaseOrderNum = x.PurchaseOrderNumber,
                        VendorID = x.VendorID,
                        SubTotal = x.SubTotal,
                        GST = x.TaxAmount,
                        Total = x.SubTotal + x.TaxAmount,
                        EmployeeID = 7,
                        PurchaseOrderDetails = x.PurchaseOrderDetails
                                    .OrderBy(x=>x.PartID)
                                    .Select(p => new PurchaseOrderDetailView
                                    {
                                        PartID = p.PartID,
                                        PartDescription = p.Part.Description,
                                        QOH = p.Part.QuantityOnHand,
                                        ROL = p.Part.ReorderLevel,
                                        QOO = p.Part.QuantityOnOrder,
                                        QTO = p.Quantity,
                                        Price = p.PurchasePrice,
                                    }).ToList()
                    }).FirstOrDefault();
                }
                else
                {

                    var suggestedParts = _purchasingContext.Parts
                                        .OrderBy(x => x.PartID)
                                        .Where(x => x.ReorderLevel - (x.QuantityOnHand + x.QuantityOnOrder) > 0 && x.VendorID == vendorID)
                                    .Select(x => new PurchaseOrderDetailView
                                    {
                                        PartID = x.PartID,
                                        PartDescription = x.Description,
                                        QOH = x.QuantityOnHand,
                                        ROL = x.ReorderLevel,
                                        QOO = x.QuantityOnOrder,
                                        QTO = x.ReorderLevel - (x.QuantityOnHand + x.QuantityOnOrder), //Suggested quantity to purchase based on the buffer
                                        Price = x.PurchasePrice
                                    }).ToList();
                    return new PurchaseOrderView
                    {
                        VendorID = vendorID,
                        PurchaseOrderDetails = suggestedParts,
                        EmployeeID = 7,
                        SubTotal = suggestedParts.Sum(x => x.Price * x.QTO),
                        GST = suggestedParts.Sum(x => x.Price * x.QTO) * (decimal)0.05,
                        Total = (suggestedParts.Sum(x => x.Price * x.QTO) * (decimal)0.05) + suggestedParts.Sum(x => x.Price * x.QTO)


                    };
                }
            }
            else
            {
                throw new Exception("Vendor does not exist!");
            }
        }


        //Form Methods
        public async Task RemoveItem(int partID, PurchaseOrderView purchaseOrder, List<ItemView> inventoryView, string button)
        {
            if(button == "Remove")
            {
                var itemToRemove = purchaseOrder.PurchaseOrderDetails.Where(x => x.PartID == partID).FirstOrDefault();
                if (itemToRemove != null)
                {
                    purchaseOrder.PurchaseOrderDetails.Remove(itemToRemove);

                    if (inventoryView.Where(x => x.PartID == partID).Any() == false)
                    {

                        inventoryView.Add(new ItemView()
                        {
                            PurchaseOrderID = purchaseOrder.PurchaseOrderID,
                            PurchaseOrderNum = purchaseOrder.PurchaseOrderNum,
                            PartID = itemToRemove.PartID,
                            PartDescription = itemToRemove.PartDescription,
                            QOH = itemToRemove.QOH,
                            ROL = itemToRemove.ROL,
                            QOO = itemToRemove.QOO,
                            Buffer = itemToRemove.ROL - (itemToRemove.QOO + itemToRemove.QOO),
                            Price = _purchasingContext.Parts.Where(x => x.PartID == partID).Select(x => x.PurchasePrice).FirstOrDefault()
                        });
                    }
                }
            }
            else
            {
                var itemToRemove = inventoryView.Where(x => x.PartID == partID).FirstOrDefault();
                if (itemToRemove != null)
                {
                    inventoryView.Remove(itemToRemove);

                    if (purchaseOrder.PurchaseOrderDetails.Where(x => x.PartID == partID).Any() == false)
                    {
                        purchaseOrder.PurchaseOrderDetails.Add(new PurchaseOrderDetailView
                        {
                            PartDescription = itemToRemove.PartDescription, 
                            PartID=itemToRemove.PartID,
                            QOH=itemToRemove.QOH,
                            ROL=itemToRemove.ROL,
                            QOO=itemToRemove.QOO,
                            QTO=0,
                            Price=itemToRemove.Price
                        });
                    }
                }
            }

        }

        //Transactional Services
        public void SaveOrder(PurchaseOrderView purchaseOrder, string button)
        {
            bool changesMade = false;
            List<Exception> errorlist = new List<Exception>();
            PurchaseOrder order = new PurchaseOrder();
        
            List<PurchaseOrderDetail> poDetailsInDatabase = _purchasingContext.PurchaseOrderDetails
                   .Where(x => x.PurchaseOrderID == purchaseOrder.PurchaseOrderID)
                   .ToList();


            //VALIDATIONS
            //----Parameter has no value - throw error---//
            if (purchaseOrder == null || purchaseOrder.PurchaseOrderDetails.Count == 0 || purchaseOrder.VendorID == 0)
            {
                throw new ArgumentNullException("There is no order details");
            }

            //------Check for order date---//
            PurchaseOrder purchaseOrderData = _purchasingContext.PurchaseOrders
                                         .FirstOrDefault(x => x.PurchaseOrderID == purchaseOrder.PurchaseOrderID);
        
            if (purchaseOrderData != null && purchaseOrderData.OrderDate != null)
            {
                throw new ArgumentNullException("Order is closed and cannot be edited or placed");
            }


            bool employeeExists = _purchasingContext.Employees.Where(x => x.EmployeeID == purchaseOrder.EmployeeID).Any();

            if (!employeeExists)
            {
                errorlist.Add(new Exception($"Employee with the id {purchaseOrder.EmployeeID} does not exist in the database."));
            }

       

            //Validate Purchase Order Details 
            foreach (var orderDetail in purchaseOrder.PurchaseOrderDetails)
            {
                //Part must exist 
                if (orderDetail.PartID == 0)
                {
                    throw new ArgumentNullException($"Part ID cannot be 0");
                }
                Part part = _purchasingContext.Parts
                                .Where(x => x.PartID == orderDetail.PartID && x.VendorID == purchaseOrder.VendorID)
                                .FirstOrDefault();
                if (part == null)
                {
                    //throw new ArgumentNullException($"Item {orderDetail.PartDescription} does not exist in vendor { purchaseOrder.VendorID} database");

                    errorlist.Add(new Exception($"Item {orderDetail.PartDescription} does not exist in vendor {purchaseOrder.VendorID} database"));
                }

                //Positive price
                if (orderDetail.Price <= 0)
                {
                    errorlist.Add(new Exception($"Item {orderDetail.PartDescription} price must be greater than zero"));
                }

                //Postive Quantity
                if (orderDetail.QTO <= 0)
                {
                    errorlist.Add(new Exception($"Item {orderDetail.PartDescription} quantity must be greater than zero"));
                }


                if (errorlist.Count == 0)
                {
                    //we checking if the vendor already has an active order open, if yes then we change the data directly
                    if (purchaseOrder.PurchaseOrderNum > 0)
                    {
                        PurchaseOrderDetail activeOrderDetail = _purchasingContext.PurchaseOrderDetails
                                                                .Where(x => x.PartID == orderDetail.PartID && purchaseOrder.PurchaseOrderID == x.PurchaseOrderID)
                                                                .FirstOrDefault();
                        
                        if (activeOrderDetail != null)
                        {
                                activeOrderDetail.Quantity = orderDetail.QTO;
                                activeOrderDetail.PurchasePrice = orderDetail.Price;
                                _purchasingContext.PurchaseOrderDetails.Update(activeOrderDetail);
                        }
                        else
                        {
                            Part newPart = _purchasingContext.Parts
                                                .Where(x => x.PartID == orderDetail.PartID && x.VendorID == purchaseOrder.VendorID)
                                                .FirstOrDefault();
                            PurchaseOrderDetail newOrderItem = new PurchaseOrderDetail();
                            if (newPart != null)
                            {
                                newOrderItem.PartID = newPart.PartID;
                                newOrderItem.Part = newPart;
                                newOrderItem.Quantity = orderDetail.QTO;
                                newOrderItem.PurchasePrice = orderDetail.Price;
                                newOrderItem.PurchaseOrderID= purchaseOrder.PurchaseOrderID;
                                _purchasingContext.PurchaseOrderDetails.Add(newOrderItem);
                            }
                            else
                            {
                                throw new ArgumentNullException($"Part {orderDetail.PartDescription} does not exist");
                            }

                        }
                    }
                    else
                    {
                        PurchaseOrderDetail newOrderDetails = new PurchaseOrderDetail();
                        newOrderDetails.Part = _purchasingContext.Parts.Where(x => x.PartID == orderDetail.PartID).FirstOrDefault();
                        newOrderDetails.PartID = orderDetail.PartID;
                        newOrderDetails.PurchasePrice = orderDetail.Price;
                        newOrderDetails.Quantity = orderDetail.QTO;
                        order.PurchaseOrderDetails.Add(newOrderDetails);
                    }
                    if(button == "place")
                    {
                        part.QuantityOnOrder += orderDetail.QTO;//updating the quantity on order for the part we ordering

                    }

                }
            }


            if (errorlist.Count == 0)
            {
                if (purchaseOrder.PurchaseOrderNum == 0)
                {
                    order.PurchaseOrderNumber = SetNewOrderNum();
                    order.VendorID = purchaseOrder.VendorID;
                    order.SubTotal = order.PurchaseOrderDetails.Sum(x => x.Quantity * x.PurchasePrice);
                    order.TaxAmount = order.SubTotal * (decimal)0.05;
                    order.EmployeeID = purchaseOrder.EmployeeID;
                    order.OrderDate = button == "place" ? DateTime.Now : null;
                    _purchasingContext.PurchaseOrders.Add(order);
                }
                else
                {
                    Console.WriteLine("Updated exisitng active order");

                    purchaseOrderData.SubTotal = purchaseOrderData.PurchaseOrderDetails.Sum(x => x.Quantity * x.PurchasePrice);
                    purchaseOrderData.TaxAmount = purchaseOrderData.SubTotal * (decimal)0.05;
                    purchaseOrderData.OrderDate = button == "place" ? DateTime.Now : null;
                    _purchasingContext.PurchaseOrders.Update(purchaseOrderData);

                }
            }
            //Remove items from database that are not on current purchaseOrder

            foreach (var item in poDetailsInDatabase)
            {
                PurchaseOrderDetailView itemInPO = purchaseOrder.PurchaseOrderDetails.FirstOrDefault(x => x.PartID == item.PartID);
                if (itemInPO == null)
                {

                    _purchasingContext.PurchaseOrderDetails.Remove(item);
                }
            }

            if (errorlist.Count > 0)
            {
                _purchasingContext.ChangeTracker.Clear();

                throw new AggregateException("Unable to save the order. Check concerns", errorlist.OrderBy(x => x.Message).ToList());
            }
            else
            {
                _purchasingContext.SaveChanges();

            }
        }

        public int SetNewOrderNum()
        {
            int newOrderNum = _purchasingContext.PurchaseOrders
                                .Select(x => x.PurchaseOrderNumber).Max();
            newOrderNum++;
            return newOrderNum;
        }
        public void DeleteOrder(PurchaseOrderView purchaseOrder)
        {
            List<Exception> errorlist = new List<Exception>();
            PurchaseOrder order = new PurchaseOrder();
            bool employeeExists = _purchasingContext.Employees.Where(x => x.EmployeeID == purchaseOrder.EmployeeID).Any();
            bool orderExists = _purchasingContext.PurchaseOrders.Where(x => x.VendorID == purchaseOrder.VendorID && x.PurchaseOrderID == purchaseOrder.PurchaseOrderID).Any();
            bool vendorExists = _purchasingContext.Vendors.Where(x => x.VendorID == purchaseOrder.VendorID).Any();

            //VALIDATIONS
            //----Parameter has no value - throw error---//
            if (purchaseOrder == null || purchaseOrder.VendorID == 0)
            {
                throw new ArgumentNullException("Order is null");
            }

            //------Check for order date---//
            PurchaseOrder purchaseOrderData = _purchasingContext.PurchaseOrders
                            .Where(x => x.VendorID == purchaseOrder.VendorID
                                                && x.PurchaseOrderID == purchaseOrder.PurchaseOrderID).FirstOrDefault();
            if (purchaseOrderData != null && purchaseOrderData.OrderDate != null)
            {
                throw new ArgumentNullException("Order is closed and cannot be deleted");
            }

            if (!orderExists)
            {
                errorlist.Add(new Exception($"PO with the id {purchaseOrder.PurchaseOrderID} does not exist in the database."));
            }
            if (!employeeExists)
            {
                errorlist.Add(new Exception($"Employee with the id {purchaseOrder.EmployeeID} does not exist in the database."));
            }
            if (!vendorExists)
            {
                errorlist.Add(new Exception($"Vendor with the id {purchaseOrder.VendorID} does not exist in the database."));
            }


            if (errorlist.Count == 0)
            {
                List<PurchaseOrderDetail> orderDetailsToRemove = _purchasingContext.PurchaseOrderDetails
                                                            .Where(x => x.PurchaseOrderID == purchaseOrder.PurchaseOrderID).ToList();
                var orderToRemove = _purchasingContext.PurchaseOrders.Where(x => x.PurchaseOrderID == purchaseOrder.PurchaseOrderID).FirstOrDefault();

                foreach (var item in orderDetailsToRemove)
                {
                    _purchasingContext.PurchaseOrderDetails.Remove(item);
                }
                _purchasingContext.PurchaseOrders.Remove(orderToRemove);
            }

            if (errorlist.Count > 0)
            {
                _purchasingContext.ChangeTracker.Clear();

                throw new AggregateException("Unable to delete the order. Check concerns", errorlist.OrderBy(x => x.Message).ToList());
            }
            else
            {
                _purchasingContext.SaveChanges();
            }
        }
    }
}
