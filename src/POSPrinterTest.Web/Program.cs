using Microsoft.EntityFrameworkCore;
using POSPrinterTest.Web.Data;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = GetProjectDirectory()
});

static string GetProjectDirectory([System.Runtime.CompilerServices.CallerFilePath] string path = "")
    => System.IO.Path.GetDirectoryName(path)!;

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IPrinterService, PrinterService>();
builder.Services.AddScoped<IPrintService, PrintService>();

builder.Services.AddRazorPages(options =>
{
    options.RootDirectory = "/";
});

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

app.UseAuthorization();

app.MapRazorPages();

app.Run();
