using DataLabeling.BLL.Services;
using DataLabeling.Core.Interfaces;
using DataLabeling.DAL;
using DataLabeling.DAL.Repositories;
using DataLabeling.DAL.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CẤU HÌNH SERVICES & DB
// ==========================================

// Lấy chuỗi kết nối
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Đăng ký DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Đăng ký UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// --- ĐĂNG KÝ CÁC SERVICE (QUAN TRỌNG) ---
builder.Services.AddScoped<IUserService, UserService>();       // <--- Dòng này bị thiếu trước đó
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();
// ----------------------------------------

builder.Services.AddControllers();

// Cấu hình Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ==========================================
// 2. CẤU HÌNH PIPELINE
// ==========================================

// Luôn bật Swagger để test (bất kể môi trường)
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();