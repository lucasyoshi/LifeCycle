using eBikes.Areas.Identity;
using eBikes.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Receiving;
using Purchasing;
using SalesAndReturns;
using Servicing;
using MatBlazor;


var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var connectionStringBikes = builder.Configuration.GetConnectionString("eBikes");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
// Add services to the container.
builder.Services.ReceivingBackendDependencies(options =>
                    options.UseSqlServer(connectionStringBikes));
//Purchasing 
builder.Services.PurchasingBackendDependencies(options =>
                    options.UseSqlServer(connectionStringBikes));

//Sales and Returns
builder.Services.SalesAndReturnsBackendDependencies(options =>
                    options.UseSqlServer(connectionStringBikes));

//Servicing
builder.Services.ServicingBackendDependencies(options =>
                    options.UseSqlServer(connectionStringBikes));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddMatBlazor();
//  placeholder for db context
builder.Services.AddDbContext<ApplicationDbContext>(a =>
{
    // will add out connection string to chinook
    a.UseSqlServer();
}
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
