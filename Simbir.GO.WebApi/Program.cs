using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Simbir.GO.BLL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<SimbirGoDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration["ConnectionString"]); // Replace with your PostgreSQL connection string
});

// For Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<SimbirGoDbContext>()
                .AddDefaultTokenProviders();

// Adding Authentication
builder.Services.AddAuthentication(cfg =>
            {
                cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWTKey:ValidAudience"],
                    ValidIssuer = builder.Configuration["JWTKey:ValidIssuer"],
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWTKey:SecretKey"]))
                };
            });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts => {
    opts.AddSecurityDefinition("jwt_auth", new OpenApiSecurityScheme
    {
        Name = "Bearer",
        BearerFormat = "JWT",
        Scheme = "bearer",
        Description = "Specify the authorization token.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
    });
  opts.AddSecurityRequirement(new OpenApiSecurityRequirement {
   { 
     new OpenApiSecurityScheme 
     {
        Reference = new OpenApiReference()
        {
            Id = "jwt_auth",
            Type = ReferenceType.SecurityScheme
        }
      },
      new string[] { } 
    }});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//initialize data
//cringe tho, have to move it to migrations or smth like this
using(var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SimbirGoDbContext>();

    dbContext.Database.EnsureCreated();

    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        
        async Task createRoleIfDoesntExist(string name)
        {
            var userRole = await roleManager.FindByNameAsync(name);
            if (userRole is null)
            {
                userRole = new IdentityRole
                {
                    Name = name
                };

                await roleManager.CreateAsync(userRole);
            }
        }

        await createRoleIfDoesntExist("user");
        await createRoleIfDoesntExist("admin");
    }

    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        
        if (await userManager.FindByNameAsync("admin") is null)
        {
            IdentityUser user = new()
            {
                UserName = "admin"
            };

            user.PasswordHash = userManager.PasswordHasher.HashPassword(user, "admin");

            await userManager.CreateAsync(user);

            await userManager.AddToRoleAsync(user, "admin");
        }
    }
}

app.UseAuthorization();

app.MapControllers();

app.Run();
