using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MoogleEngine;
using System.Diagnostics;


Stopwatch stopWatch = new Stopwatch();
stopWatch.Start();

var builder = WebApplication.CreateBuilder(args);

Moogle.StartIndex();


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}


app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

System.Console.WriteLine(stopWatch.Elapsed);
app.Run();