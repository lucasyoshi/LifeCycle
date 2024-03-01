#nullable disable
using SalesAndReturns.DAL;
using SalesAndReturns.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesAndReturns.Entities;

namespace SalesAndReturns.BLL
{
    public class SalesAndReturnsServices
    {
        #region Fields
        private readonly SalesAndReturnsContext _context;
        #endregion
        internal SalesAndReturnsServices(SalesAndReturnsContext context)
        {
            _context = context;
        }

        public int Refund(SaleRefundView saleRefund, List<SaleRefundDetailView> saleRefundDetails)
        {
            //a container to hold x number of Exception messages
            List<Exception> errorList = new List<Exception>();

            #region Validation
            if (saleRefund.SaleId < 0)
            {
                throw new ArgumentNullException("Missing SaleID");
            }
            foreach (var item in saleRefundDetails)
            {
                if (item.Quantity > item.OriginalQuantity)
                {
                    errorList.Add(new Exception($"Cannot return greater quantity ({item.Quantity}) than the original "
                            + $"quantity ({item.OriginalQuantity}) for item: {item.Description}"));
                }
                if (item.ReturnQuantity != 0 && item.Quantity > (item.OriginalQuantity - item.ReturnQuantity))
                {
                    errorList.Add(new Exception($"Cannot return quantity {item.Quantity} for item: {item.Description}. "
                            + $"Previously returned quantity of {item.ReturnQuantity} means that maximum quantity that "
                            + $"can be returned is: {item.OriginalQuantity - item.ReturnQuantity}."));
                }
                if (String.IsNullOrWhiteSpace(item.Reason) && item.Quantity > 0)
                {
                    errorList.Add(new Exception($"{item.Description} must have a reason for return"));
                }
            }
            #endregion

            SaleRefund thisSaleRefund = new()
            {
                SaleRefundDate = DateTime.Now,
                SaleID = saleRefund.SaleId,
                EmployeeID = saleRefund.EmployeeID,
                TaxAmount = saleRefund.TaxAmount,
                SubTotal = saleRefund.SubTotal
            };


            foreach (var detail in saleRefundDetails)
            {
                SaleRefundDetail thisSaleRefundDetails = new()
                {
                    PartID = detail.PartID,
                    Quantity = detail.Quantity,
                    SellingPrice = detail.SellingPrice,
                    Reason = detail.Reason,
                    Part = _context.Parts.Where(p => p.PartID == detail.PartID).FirstOrDefault()
                };

                //var discount = Sales.Where(s => s.SaleID == saleRefund.SaleId ).FirstOrDefault().Coupon.CouponDiscount;
                var itemPrice = thisSaleRefundDetails.SellingPrice;
                if (saleRefund.DiscountPercent > 0)
                {
                    itemPrice -= itemPrice * ((decimal)saleRefund.DiscountPercent / 100);
                }
                thisSaleRefund.TaxAmount += itemPrice * (decimal)0.05;
                thisSaleRefund.SubTotal += itemPrice;

                thisSaleRefund.SaleRefundDetails.Add(thisSaleRefundDetails);
                _context.Parts.Where(p => p.PartID == thisSaleRefundDetails.PartID).FirstOrDefault().QuantityOnHand += thisSaleRefundDetails.Quantity;

            }
            var employee = _context.Employees.Where(e => e.EmployeeID == thisSaleRefund.EmployeeID).FirstOrDefault();
            employee.SaleRefunds.Add(thisSaleRefund);

            if (errorList.Count > 0)
            {
                //  throw the list of business processing error(s)
                throw new AggregateException("Unable to Refund sale. Check concerns", errorList.OrderBy(x => x.Message).ToList());
            }
            else
            {
                //  consider data valid
                //	has passed business processing rules
                _context.SaveChanges();
                return thisSaleRefund.SaleRefundID;
            };


        }

        public int Checkout(SalesView sale, List<SaleDetailView> saleDetails)
        {
            //a container to hold x number of Exception messages
            List<Exception> errorList = new List<Exception>();

            #region Validation
            if (saleDetails.Count() < 0)
            {
                throw new ArgumentNullException("sale details (cart) is empty. No parts to checkout.");
            }
            foreach (var item in saleDetails)
            {
                if (item.Quantity <= 0)
                {
                    errorList.Add(new Exception($"Quantity for {item.Description} must be a positive non zero number."));
                }
                if (item.Quantity > _context.Parts.Where(p => p.PartID == item.PartID).FirstOrDefault().QuantityOnHand)
                {
                    errorList.Add(new Exception($"Quantity requested for {item.Description} ({item.Quantity}) can't be greater than "
                        + $"quantity on hand ({_context.Parts.Where(p => p.PartID == item.PartID).FirstOrDefault().QuantityOnHand})."));
                }
            }
            if (String.IsNullOrWhiteSpace(sale.PaymentType))
            {
                errorList.Add(new Exception("Payment type must be selected."));
            }
            #endregion


            Sale thisSale = new()
            {
                SaleDate = DateTime.Now,
                EmployeeID = sale.EmployeeID,
                PaymentType = sale.PaymentType,
                Coupon = _context.Coupons.Where(c => c.CouponID == sale.CouponId).FirstOrDefault(),
                Employee = _context.Employees.Where(e => e.EmployeeID == sale.EmployeeID).FirstOrDefault()
            };

            foreach (var detail in saleDetails)
            {
                SaleDetail thisSaleDetails = new()
                {
                    SaleID = thisSale.SaleID,
                    PartID = detail.PartID,
                    Quantity = detail.Quantity,
                    SellingPrice = detail.SellingPrice,
                    Part = _context.Parts.Where(p => p.PartID == detail.PartID).FirstOrDefault()
                };
                var discount = 0;
                if (thisSale.Coupon != null)
                { 
                    discount = thisSale.Coupon.CouponDiscount;
                
                }
                

                var itemPrice = thisSaleDetails.SellingPrice;
                if (discount > 0)
                {
                    itemPrice -= itemPrice * ((decimal)discount / 100);
                }
                thisSale.TaxAmount += itemPrice * (decimal)0.05;
                thisSale.SubTotal += itemPrice;

                thisSale.SaleDetails.Add(thisSaleDetails);
                _context.Parts.Where(p => p.PartID == thisSaleDetails.PartID).FirstOrDefault().QuantityOnHand -= thisSaleDetails.Quantity;

            }
            Employee employee = _context.Employees.Where(e => e.EmployeeID == sale.EmployeeID).FirstOrDefault();
            employee.Sales.Add(thisSale);

            if (errorList.Count > 0)
            {
                //  throw the list of business processing error(s)

                throw new AggregateException("Unable checkout sale. Check concerns", errorList.OrderBy(x => x.Message).ToList());
            }
            else
            {
                //  consider data valid
                //	has passed business processing rules

                _context.SaveChanges();
                return thisSale.SaleID;
            }
        }


        public List<CategoryView> GetCategoryList()
        {
            return _context.Categories.Select(x => new CategoryView
            {
                CategoryID = x.CategoryID,
                Description = x.Description
            }).ToList();
        }

        public List<PartView> GetPartList(int CategoryID)
        {
            return _context.Parts
                .Where(p => p.CategoryID == CategoryID && p.Discontinued == false)
                .Select(p => new PartView
                {
                    PartID = p.PartID,
                    Description = p.Description,
                    SellingPrice = p.SellingPrice
                }).ToList();
        }


        public int GetCoupon(string coupons)
        {
            if (_context.Coupons.Where(c => c.CouponIDValue == coupons).Any())
            {
                return _context.Coupons.Where(c => c.CouponIDValue == coupons)
                            .Select(c => c.CouponDiscount).FirstOrDefault();
            }
            else
            {
                return 0;
            }
        }

        public SaleRefundView GetSaleRefund(int saleID)
        {
            return _context.SaleRefunds.Where(sr => sr.SaleID == saleID)
                        .Select(sr => new SaleRefundView
                        {
                            SaleId = sr.SaleID,
                            EmployeeID = sr.EmployeeID,
                            TaxAmount = sr.TaxAmount,
                            SubTotal = sr.SubTotal,
                            DiscountPercent = sr.Sale.Coupon.CouponDiscount
                        }).FirstOrDefault();
        }

        public List<SaleRefundDetailView> GetSaleDetailsRefund(int saleID)
        {
            return _context.SaleRefundDetails.Where(srd => srd.SaleRefund.SaleID == saleID)
                                    .Select(srd => new SaleRefundDetailView
                                    {
                                        PartID = srd.PartID,
                                        Description = srd.Part.Description,
                                        OriginalQuantity = srd.SaleRefund.Sale.SaleDetails
                                                            .Where(sd => sd.SaleID == saleID && sd.PartID == srd.PartID)
                                                            .Select(sd => sd.Quantity).FirstOrDefault(),
                                        SellingPrice = srd.SellingPrice,
                                        ReturnQuantity = srd.Quantity,
                                        Refundable = srd.Part.Refundable == "Y" ? true : false,
                                        Quantity = srd.Quantity,
                                        Reason = srd.Reason
                                    }).ToList();

        }
    }
}
