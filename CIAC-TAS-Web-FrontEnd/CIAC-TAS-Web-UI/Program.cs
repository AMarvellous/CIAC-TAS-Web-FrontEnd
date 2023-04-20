using CIAC_TAS_Web_UI.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CIAC_TAS_Web_UI.Data;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using CIAC_TAS_Web_UI.Helper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<CIAC_TAS_Web_UIContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CIAC_TAS_Web_UIContext") ?? throw new InvalidOperationException("Connection string 'CIAC_TAS_Web_UIContext' not found.")));
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ExceptionHandlerFilter>();
    options.Filters.Add<LoginFilter>();
    options.Filters.Add<WebMenuFilter>();
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddScoped<ICuestionarioASAHelper, CuestionarioASAHelper>();

builder.Services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");
builder.Services.AddAntiforgery(o => o.SuppressXFrameOptionsHeader = true);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      builder =>
                      {
                          builder.WithOrigins("http://ciac-tas.com");
                      });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//app.UseExceptionHandler("/Error/Errors");
app.UseExceptionHandler("/Error");
// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
app.UseHsts();
//}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.UseSession();

app.MapRazorPages();

app.Run();
