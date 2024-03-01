using Microsoft.AspNetCore.Components;
using Servicing.ViewModels;
using Servicing.BLL;
using Servicing.Entities;
using Receiving.BLL;
using Receiving.ViewModels;
using Microsoft.AspNetCore.Components.Forms;

namespace eBikes.Pages.Servicing
{
    public partial class Servicing
    {
        #region Injection
        [Inject]
        protected ServicingServices? ServicingServices { get; set; }
        #endregion

        #region Fields
        private int customerID { get; set; } = 0;
        private string customerName { get; set; } = "";
        private string partialCustomerName { get; set; } = "";
        private string vin { get; set; } = "";
        private string serviceDescription { get; set; } = "";
        private decimal serviceHours { get; set; } = 0;
        private string serviceComment { get; set; } = "";
        private int couponDiscount { get; set; } = 0;
        private string feedback { get; set; } = "";

        private ServiceView serviceItem = new ServiceView();
        private JobView jobView = new JobView();

        private EditContext? editContext;
        #endregion

        protected List<CustomerView> customerList { get; set; }
        protected List<VehicleView> vehicleList { get; set; } = new List<VehicleView>();
        protected List<StandardServiceView> standardServiceList { get; set; }
        protected List<ServiceView> serviceList { get; set; }
        protected CouponView coupon { get; set; } = new CouponView();

        #region Methods

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await InvokeAsync(StateHasChanged);
                GetStandardJobs();
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

        private async Task GetCustomers(string partialCustomerLastName)
        {
            try
            {
                customerList = ServicingServices.GetCustomers(partialCustomerLastName);
                
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

        private async Task GetCustomerVehicles(int customerid)
        {
            try
            {
                vehicleList = new();
                vin = "";
                vehicleList = ServicingServices.GetVehicles(customerid);
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

        private async Task GetStandardJobs()
        {
            try
            {
                standardServiceList = ServicingServices.GetStandardServices();
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

        private async Task CheckCoupon(string couponIDValue)
        {
            try
            {
                couponDiscount = ServicingServices.CheckCoupon(couponIDValue);
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

        private async Task RegisterJob()
        {
            try
            {
                ServicingServices.RegisterJob(jobView, serviceList);
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

        public void SelectCustomer(int customerid, string customername)
        {
            customerID = customerid;
            customerName = customername;
            GetCustomerVehicles(customerID);
        }

        public void Add()
        {
            serviceItem.ServiceHours = serviceHours;
            serviceItem.ServiceName = serviceDescription;
            serviceItem.ServiceComment = serviceComment;

            jobView.SubTotal += (serviceItem.ServiceHours * jobView.ShopRate);

            serviceList.Add(serviceItem);

            Reset();
        }

        public void Reset()
        {
            serviceItem = new();
            serviceHours = 0;
            serviceComment = "";
            serviceDescription = "";
        }

        public void Remove(string servicename)
        {
            foreach (var service in serviceList)
            {
                if (service.ServiceName == servicename)
                {
                    serviceList.Remove(service);
                    return;
                }
            }
        }

        public void Clear()
        {
            customerID = 0;
            customerName = "";
            partialCustomerName = "";
            vin = "";
            couponDiscount = 0;
            Reset();
            serviceList = new();
            vehicleList = new();
            customerList = new();
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