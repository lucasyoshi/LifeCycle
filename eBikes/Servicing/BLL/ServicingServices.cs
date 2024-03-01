#nullable disable

using Servicing.DAL;
using Servicing.Entities;
using Servicing.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servicing.BLL
{
    public class ServicingServices
    {
        #region DAL Context
        private readonly ServicingContext _servicingContext;
        #endregion

        internal ServicingServices (ServicingContext servicingContext)
        {
            _servicingContext = servicingContext;
        }

        #region Query Methods
        public List<CustomerView> GetCustomers(string partialCustomerLastName)
        {
            return _servicingContext.Customers
                .Where(c => c.LastName.Contains(partialCustomerLastName))
                .Select(c => new CustomerView
                {
                    CustomerID = c.CustomerID,
                    CustomerName = c.FirstName + ' ' + c.LastName,
                    PhoneNumber = c.ContactPhone,
                    Address = c.Address + " " + c.City,
                }).ToList();
        }

        public List<VehicleView> GetVehicles(int customerID)
        {
            return _servicingContext.CustomerVehicles
                .Where(cv => cv.CustomerID == customerID)
                .Select(cv => new VehicleView
                {
                    VehicleName = cv.Make + ' ' + cv.Model,
                    VIN = cv.VehicleIdentification,
                    CustomerID = cv.CustomerID
                }).ToList();
        }

        public List<StandardServiceView> GetStandardServices()
        {
            return _servicingContext.StandardJobs
                .Select(sj => new StandardServiceView
                {
                    StandardServiceID = sj.StandardJobID,
                    Description = sj.Description,
                    StandardHours = sj.StandardHours
                }).ToList();
        }

        /*public List<CouponView> GetCoupons()
        {
            return _servicingContext.Coupons
                .Select(c => new CouponView
                {
                    CouponIDValue = c.CouponIDValue,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    CouponDiscount = c.CouponDiscount
                }).ToList();
        }*/

        public int CheckCoupon(string couponIDValue)
        {
            int couponDiscount = _servicingContext.Coupons
                .Where(c => c.CouponIDValue == couponIDValue)
                .Where(c => c.StartDate < DateTime.Now)
                .Where(c => c.EndDate > DateTime.Now)
                .Select(c => c.CouponDiscount).FirstOrDefault();

            return couponDiscount;
        }
        #endregion

        #region Transactional Method
        public void RegisterJob(JobView serviceJob, List<ServiceView> serviceJobDetails)
        {
            List<Exception> errorList = new List<Exception>();

            if (serviceJob.EmployeeID == 0)
            {
                errorList.Add(new Exception("Missing employee ID."));
            }

            if (serviceJob.VIN == null)
            {
                errorList.Add(new Exception("Missing VIN."));
            }

            Job newJob = new()
            {
                JobDateIn = DateTime.Now,
                EmployeeID = serviceJob.EmployeeID,
                VehicleIdentification = serviceJob.VIN
            };

            if (serviceJobDetails == null)
            {
                errorList.Add(new Exception("Job Details is null."));
            };

            foreach (var detail in serviceJobDetails)
            {
                JobDetail newJobDetails = new JobDetail
                {
                    JobID = newJob.JobID,
                    EmployeeID = serviceJob.EmployeeID,
                    Description = detail.ServiceName,
                    Comments = detail.ServiceComment,
                    JobHours = detail.ServiceHours,
                    StatusCode = "S",
                    CouponID = _servicingContext.Coupons
                        .Where(c => c.CouponIDValue == serviceJob.CouponIDValue)
                        .Select(c => c.CouponID).FirstOrDefault(),
                };
                newJob.JobDetails.Add(newJobDetails);
            }
            Employee employee = _servicingContext.Employees.Where(e => e.EmployeeID == serviceJob.EmployeeID).FirstOrDefault();
            employee.Jobs.Add(newJob);

            if (errorList.Count > 0)
            {
                throw new AggregateException("Unable to register job.", errorList.OrderBy(x => x.Message).ToList());
            }
            else
            {
                _servicingContext.SaveChanges();
            }
        }
        #endregion
    }
}
