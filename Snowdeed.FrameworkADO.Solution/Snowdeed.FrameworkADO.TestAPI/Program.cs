using Microsoft.AspNetCore.Mvc;
using Snowdeed.FrameworkADO.TestAPI.Core;
using Snowdeed.FrameworkADO.TestAPI.Entities;
using Snowdeed.FrameworkADO.TestAPI.Enums;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped(x => new TestDbContext(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.MapGet("/dbReady", async ([FromServices] TestDbContext context) =>
{
    await context.CreateDatabaseAsync();
    await context.CreateTableAsync();
});

app.MapPost("/addEmployee", async ([FromServices] TestDbContext context) =>
{
    await context.Employee.AddAsync(new Employee() {Surname = "Ducoroy", Firstname = "Stéphane", Gender = GenderEnum.Male, Matricule = "0098", Position = "Développeur informatique" });
});

app.MapGet("/getEmployees", async ([FromServices] TestDbContext context) =>
{
    return await context.Employee.GetAllAsync();
});

app.MapGet("/getEmployee/{Id}", async ([FromServices] TestDbContext context, [FromRoute] Guid Id) =>
{
    return await context.Employee.GetAsync(Id);
});

app.MapPut("/updateEmployee", async ([FromServices] TestDbContext context, [FromBody] Employee employeeUpdate) =>
{
    return await context.Employee.UdpateAsync(employeeUpdate);
});

app.MapPut("/updateEmployee/{id}", async ([FromServices] TestDbContext context, [FromRoute] Guid id, [FromBody] Employee employeeUpdate) =>
{
    return await context.Employee.UdpateAsync(id, employeeUpdate);
});

app.MapDelete("/deleteEmployee", async ([FromServices] TestDbContext context, Guid Id) =>
{
    await context.Employee.DeleteAsync(Id);
});

app.Run();
