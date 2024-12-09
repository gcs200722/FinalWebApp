using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using FinalWebApp.Commons;
using FinalWebApp.Data;
using FinalWebApp.Data.Entities;
using FinalWebApp.Session;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<FinalWebDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Yêu cầu xác nhận tài khoản
    options.Password.RequireDigit = true; // Yêu cầu mật khẩu có ít nhất một chữ số
    options.Password.RequiredLength = 6; // Mật khẩu tối thiểu 6 ký tự
    options.Password.RequireLowercase = true; // Yêu cầu có ít nhất một chữ cái viết thường
    options.Password.RequireNonAlphanumeric = false; // Không yêu cầu ký tự đặc biệt
    options.Password.RequireUppercase = true; // Yêu cầu có ít nhất một chữ cái viết hoa
    options.Password.RequiredUniqueChars = 1; // Mật khẩu phải có ít nhất một ký tự duy nhất
}).AddEntityFrameworkStores<FinalWebDbContext>()
  .AddDefaultTokenProviders();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddSignalR();
builder.Services.AddScoped<ReportGenerator>();
builder.Services.AddControllersWithViews();
var app = builder.Build();
// Configure the HTTP request pipeline.
var scope = app.Services.CreateScope();
await SeedRolesAsync(scope.ServiceProvider);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.MapHub<OrderHub>("/orderHub");
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
static async Task SeedRolesAsync(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roleNames = { "CUSTOMER", "ADMIN", "STAFF" , "MANAGER" }; // Thêm các vai trò cần thiết
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
    string adminEmail = "Admin@test123";
    string adminPassword = "Admin123";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true, // Xác nhận email mặc định
            Fullname = "Administrator",
            NumberPhone="12345678"
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "ADMIN");
            Console.WriteLine("Admin account created successfully.");
        }
        else
        {
            Console.WriteLine("Failed to create Admin account:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"- {error.Description}");
            }
        }
    }
    else
    {
        Console.WriteLine("Admin account already exists.");
    }
}