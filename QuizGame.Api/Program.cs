//using Microsoft.EntityFrameworkCore;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
//builder.Services.AddControllers();

//builder.Services.AddControllers();

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowFrontend", policy =>
//    {
//        policy.WithOrigins("http://localhost:4200", "http://localhost:5173").AllowAnyHeader().AllowAnyMethod();
//    });
//});

//builder.Services.AddDbContext<SleepTrackerDbContext>(opt => opt.UseSqlServer
//    (builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddScoped<ISleepRepository, SleepRepository>();
//builder.Services.AddScoped<ISleepService, SleepService>();

//builder.Services.AddOpenApi();

//var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<SleepTrackerDbContext>();
//    dbContext.Database.Migrate();
//}

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseCors("AllowFrontend");

//app.MapControllers();

//app.Run();
