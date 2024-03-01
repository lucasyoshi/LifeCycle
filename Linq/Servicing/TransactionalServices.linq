<Query Kind="Program">
  <Connection>
    <ID>24a6a0ea-952c-46f2-aea5-75feeeb21df4</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <Database>eBike_DMIT2018</Database>
    <Server>KIERINLAPTOP\SQLSERVER</Server>
    <DisplayName>eBike</DisplayName>
    <DriverData>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Microsoft.EntityFrameworkCore.SqlServer</EFProvider>
    </DriverData>
  </Connection>
  <IncludeUncapsulator>false</IncludeUncapsulator>
</Query>

#load ".\ViewModels\*.cs"

using Servicing;

void Main()
{
	try
	{

	}
	catch (Exception ex)
	{

		throw;
	}
}

#region Method
private Exception GetInnerException(Exception ex)
{
	while (ex.InnerException != null)
	{
		ex = ex.InnerException;
	}
	return ex;
}
#endregion

public void RegisterJob(ServiceView serviceJob, List<ServiceView> serviceJobDetails)
{
	List<Exception> errorList = new List<Exception>();
	
	Jobs newJob = new Jobs
	{
		JobDateIn = DateTime.Now,
		EmployeeID = serviceJob.EmployeeID,
		VehicleIdentification = serviceJob.VIN,
	};
	
	foreach (var detail in serviceJobDetails)
	{
		JobDetails newJobDetails = new JobDetails
		{
			JobID = newJob.JobID,
			EmployeeID = serviceJob.EmployeeID,
			Description = detail.ServiceName,
			Comments = detail.ServiceComment,
			JobHours = detail.ServiceHours,
			StatusCode = "S",
			CouponID = detail.CouponID,
		};
		newJob.JobDetails.Add(newJobDetails);
	}
	Employees employee = Employees.Where(e => e.EmployeeID == serviceJob.EmployeeID).FirstOrDefault();
	employee.Jobs.Add(newJob);

	if (errorList.Count > 0)
	{
		throw new AggregateException("Unable to register job.", errorList.OrderBy(x => x.Message).ToList());
	}
	else
	{
		SaveChanges();
	}
}

public int GetCoupon(string couponVal)
{
	return Coupons
		.Where(c => c.CouponIDValue == couponVal)
		.Select(c => c.CouponDiscount).FirstOrDefault().Dump();
}