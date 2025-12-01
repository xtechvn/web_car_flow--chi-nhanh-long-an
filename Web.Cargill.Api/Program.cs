using Web.Cargill.Api.Model;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepositories;
using Repositories.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// 🔹 Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration["DataBaseConfig:SqlServer:ConnectionString"])
);
ConfigurationManager configuration = builder.Configuration; // allows both to access and to set up the config

builder.Services.Configure<Entities.ConfigModels.DataBaseConfig>(configuration.GetSection("DataBaseConfig"));
builder.Services.Configure<Entities.ConfigModels.DomainConfig>(configuration.GetSection("DomainConfig"));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IAllCodeRepository, AllCodeRepository>();
builder.Services.AddSingleton<IVehicleInspectionRepository, VehicleInspectionRepository>();
builder.Services.AddSingleton<IRoleRepository, RoleRepository>();
builder.Services.AddSingleton<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddSingleton<IMenuRepository, MenuRepository>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
// Cho phép truy cập file trong wwwroot
app.UseStaticFiles();

app.MapControllers();

app.Run();
