using System.Text;
using Dormio.Api.Data;
using Dormio.Api.Dtos;
using Dormio.Api.Entities;
using Dormio.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentityCore<AppUser>(o =>
    {
        o.User.RequireUniqueEmail = true;
        o.Password.RequiredLength = 8;
        o.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<AppRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    });
builder.Services.AddAuthorization();

var edm = new ODataConventionModelBuilder();
edm.EntitySet<RoomDto>("Rooms");

builder.Services.AddControllers()
    .AddOData(o => o.Select().Filter().OrderBy().Count().SetMaxTop(100)
        .AddRouteComponents("odata", edm.GetEdmModel()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "Dormio API", Version = "v1" });
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT", In = ParameterLocation.Header
    });
    o.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = Array.Empty<string>()
    });
});

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

// Apply migrations and seed roles + a default admin on startup (dev convenience)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
    foreach (var role in Roles.All)
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new AppRole(role));

    var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    const string adminEmail = "admin@dormio.local";
    if (await users.FindByEmailAsync(adminEmail) is null)
    {
        var admin = new AppUser { UserName = adminEmail, Email = adminEmail, FullName = "System Admin", EmailConfirmed = true };
        if ((await users.CreateAsync(admin, builder.Configuration["Seed:AdminPassword"] ?? "Admin@12345")).Succeeded)
            await users.AddToRoleAsync(admin, Roles.Admin);
    }
}

app.Run();
