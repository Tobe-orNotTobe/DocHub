using DochubSystem.Common.Helper;
using DochubSystem.Data.Entities;
using DochubSystem.Data.Models;
using DochubSystem.Service;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký các Service khác
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddIdentity<User, IdentityRole>()
	.AddEntityFrameworkStores<DochubDbContext>()
	.AddDefaultTokenProviders();

builder.Services.AddServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
