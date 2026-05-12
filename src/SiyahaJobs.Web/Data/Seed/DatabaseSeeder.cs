using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SiyahaJobs.Web.Helpers;
using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Models.Enums;

namespace SiyahaJobs.Web.Data.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Apply pending migrations; if none, ensure created.
        if ((await db.Database.GetPendingMigrationsAsync()).Any())
        {
            await db.Database.MigrateAsync();
        }
        else
        {
            await db.Database.EnsureCreatedAsync();
        }

        await SeedRolesAsync(roleManager);
        await SeedAdminAsync(userManager, configuration);
        await SeedCountriesAndCitiesAsync(db);
        await SeedCategoriesAsync(db);
        await SeedPartnersAsync(db);
    }

    // ---------------------------------------------------------------------
    // Roles
    // ---------------------------------------------------------------------
    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var roleName in RoleNames.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    // ---------------------------------------------------------------------
    // Default admin user
    // ---------------------------------------------------------------------
    private static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        var email = configuration["AdminUser:Email"] ?? "admin@siyahajobs.jo";
        var password = configuration["AdminUser:Password"] ?? "Admin@123456";
        var fullName = configuration["AdminUser:FullName"] ?? "Platform Administrator";

        var existing = await userManager.FindByEmailAsync(email);
        if (existing != null) return;

        var admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            EmailConfirmed = true,
            Status = AccountStatus.Active
        };

        var result = await userManager.CreateAsync(admin, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, RoleNames.Admin);
        }
    }

    // ---------------------------------------------------------------------
    // Jordan cities (based on provided design defaults)
    // ---------------------------------------------------------------------
    private static async Task SeedCountriesAndCitiesAsync(ApplicationDbContext db)
    {
        if (!await db.Countries.AnyAsync())
        {
            var jordan = new Country { Name = "Jordan", IsoCode = "JO" };
            db.Countries.Add(jordan);
            await db.SaveChangesAsync();

            var cities = new[]
            {
                "Amman", "Zarqa", "Irbid", "Aqaba", "Madaba",
                "Karak", "Salt", "Jerash", "Ajloun", "Ma'an",
                "Tafilah", "Mafraq", "Petra"
            };

            foreach (var name in cities)
            {
                db.Cities.Add(new City { Name = name, CountryId = jordan.Id });
            }
            await db.SaveChangesAsync();
        }
    }

    // ---------------------------------------------------------------------
    // Tourism &amp; hospitality categories (from homepage design)
    // ---------------------------------------------------------------------
    private static async Task SeedCategoriesAsync(ApplicationDbContext db)
    {
        if (await db.Categories.AnyAsync()) return;

        var categories = new List<Category>
        {
            new() { Name = "Formal Restaurants",        ArabicName = "مطاعم فاخرة",          IconName = "utensils",   Description = "4-5 Stars dining establishments" },
            new() { Name = "Casual Restaurants",        ArabicName = "مطاعم عائلية",         IconName = "utensils",   Description = "1-3 Stars dining establishments" },
            new() { Name = "Coffee Shops",              ArabicName = "مقاهي",                IconName = "coffee",     Description = "Coffee shops & coffee houses" },
            new() { Name = "Hotels 5-4 Stars",          ArabicName = "فنادق 5-4 نجوم",       IconName = "building",   Description = "Luxury and premium hotels" },
            new() { Name = "Hotels 1-3 Stars",          ArabicName = "فنادق 1-3 نجوم",       IconName = "building",   Description = "Mid-range and budget hotels" },
            new() { Name = "Fast Food / Quick Service", ArabicName = "مطاعم الوجبات السريعة", IconName = "utensils",  Description = "Quick service restaurants" },
            new() { Name = "Entertainment & Leisure",   ArabicName = "مدن ترفيهية",          IconName = "sparkles",   Description = "Entertainment and leisure cities" },
            new() { Name = "Hotel Apartments",          ArabicName = "شقق فندقية",           IconName = "home",       Description = "Hotel apartments & hotel suites" },
            new() { Name = "Tour Operators",            ArabicName = "شركات سياحية",         IconName = "map",        Description = "Travel agencies and tour operators" },
            new() { Name = "Bazaars & Handcrafts",      ArabicName = "بازارات وحرف يدوية",   IconName = "gift",       Description = "Craft shops and souvenir bazaars" },
            new() { Name = "Tourism Transportation",    ArabicName = "نقل سياحي",            IconName = "car",        Description = "Tourism transportation services" },
            new() { Name = "Tourism Offices",           ArabicName = "مكاتب سياحية",         IconName = "briefcase",  Description = "Tourism offices and information centres" }
        };

        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();
    }

    // ---------------------------------------------------------------------
    // Partners (optional, empty until uploaded by admin)
    // ---------------------------------------------------------------------
    private static async Task SeedPartnersAsync(ApplicationDbContext db)
    {
        if (await db.Partners.AnyAsync()) return;

        var partners = new List<Partner>
        {
            new() { Name = "Jordan Restaurant Association", LogoPath = "/uploads/logos/partner-placeholder.png", DisplayOrder = 1 },
            new() { Name = "Jordan Tourism Board",          LogoPath = "/uploads/logos/partner-placeholder.png", DisplayOrder = 2 },
            new() { Name = "TVSDC",                         LogoPath = "/uploads/logos/partner-placeholder.png", DisplayOrder = 3 }
        };

        db.Partners.AddRange(partners);
        await db.SaveChangesAsync();
    }
}
