using Microsoft.AspNetCore.Mvc;
using Snowdeed.FrameworkADO.TestAPI.Core;
using Snowdeed.FrameworkADO.TestAPI.Entities;
using Snowdeed.FrameworkADO.TestAPI.Enums;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped(x => new TestDbContext(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.MapGet("/dbReady", async ([FromServices] TestDbContext context) =>
{
    await context.CreateDatabaseAsync();
    await context.CreateTableAsync();
});

app.MapPost("/addEmployee", async ([FromServices] TestDbContext context, CancellationToken cancellationToken) =>
{
    await context.Employee.AddAsync(new Employee() {Surname = "Ducoroy", Firstname = "Stéphane", Gender = GenderEnum.Male, Matricule = "0098", Position = "Développeur informatique" }, cancellationToken);
});

app.MapGet("/getEmployees", async ([FromServices] TestDbContext context, CancellationToken cancellationToken) =>
{
    return await context.Employee.GetAllAsync(cancellationToken);
});

app.MapGet("/getEmployee/{Id:guid}", async ([FromServices] TestDbContext context, [FromRoute] Guid Id, CancellationToken cancellationToken) =>
{
    return await context.Employee.GetAsync(Id, cancellationToken);
});

app.MapGet("getEmployee/{Surname}/{Firstname}", async ([FromServices] TestDbContext context, [FromRoute] string Surname, [FromRoute] string Firstname, CancellationToken cancellationToken) =>
{
    return await context.Employee.FindAsync(x => x.Surname == Surname && x.Firstname == Firstname, cancellationToken);
});

app.MapPut("/updateEmployee", async ([FromServices] TestDbContext context, [FromBody] Employee employeeUpdate, CancellationToken cancellationToken) =>
{
    return await context.Employee.UdpateAsync(employeeUpdate, cancellationToken);
});

app.MapPut("/updateEmployee/{id}", async ([FromServices] TestDbContext context, [FromRoute] Guid id, [FromBody] Employee employeeUpdate, CancellationToken cancellationToken) =>
{
    return await context.Employee.UdpateAsync(id, employeeUpdate, cancellationToken);
});

app.MapDelete("/deleteEmployee", async ([FromServices] TestDbContext context, Guid Id, CancellationToken cancellationToken) =>
{
    await context.Employee.DeleteAsync(Id, cancellationToken);
});

app.Run();
