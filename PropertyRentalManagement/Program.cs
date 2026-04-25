using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PropertyRentalManagement.Data;
using PropertyRentalManagement.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Home/AccessDenied";
    });
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
 
app.MapStaticAssets();
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

await EnsureSeedDataAsync(app);

app.Run();

static async Task EnsureSeedDataAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
    await context.Database.MigrateAsync();

    if (!await context.Users.AnyAsync(u => u.Role == UserRoles.Owner))
    {
        var owner = new User
        {
            Name = "System Owner",
            Email = "owner@propertyrental.local",
            Role = UserRoles.Owner
        };
        owner.Password = passwordHasher.HashPassword(owner, "Owner@123");
        context.Users.Add(owner);
    }

    if (!await context.Users.AnyAsync(u => u.Role == UserRoles.Manager))
    {
        var manager = new User
        {
            Name = "Default Manager",
            Email = "manager@propertyrental.local",
            Role = UserRoles.Manager
        };
        manager.Password = passwordHasher.HashPassword(manager, "Manager@123");
        context.Users.Add(manager);
    }

    if (!await context.Users.AnyAsync(u => u.Role == UserRoles.Tenant))
    {
        var tenant = new User
        {
            Name = "Default Tenant",
            Email = "tenant@propertyrental.local",
            Role = UserRoles.Tenant
        };
        tenant.Password = passwordHasher.HashPassword(tenant, "Tenant@123");
        context.Users.Add(tenant);
    }

    if (!await context.Buildings.AnyAsync())
    {
        context.Buildings.Add(new Building
        {
            Name = "Demo Building",
            Address = "100 Main St",
            City = "Sample City",
            State = "SC",
            ZipCode = "00000"
        });
        await context.SaveChangesAsync();
    }

    if (!await context.Apartments.AnyAsync())
    {
        var firstBuilding = await context.Buildings.OrderBy(b => b.Id).FirstAsync();
        context.Apartments.Add(new Apartment
        {
            AptNumber = "101",
            BuildingId = firstBuilding.Id,
            Rooms = 2,
            Bathrooms = 1,
            Rent = 1200,
            Status = "Available"
        });
    }

    await context.SaveChangesAsync();
}
