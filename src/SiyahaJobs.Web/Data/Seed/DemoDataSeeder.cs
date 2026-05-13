using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SiyahaJobs.Web.Helpers;
using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Models.Enums;

namespace SiyahaJobs.Web.Data.Seed;

/// <summary>
/// Creates a handful of demo employers, candidates, companies and jobs so that
/// a fresh install looks populated for screenshots / demos.
/// Runs only when no jobs exist in the database.
/// </summary>
public static class DemoDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        if (await db.Jobs.AnyAsync()) return;

        var amman = await db.Cities.FirstOrDefaultAsync(c => c.Name == "Amman");
        var aqaba = await db.Cities.FirstOrDefaultAsync(c => c.Name == "Aqaba");
        var petra = await db.Cities.FirstOrDefaultAsync(c => c.Name == "Petra");

        var hotels54 = await db.Categories.FirstOrDefaultAsync(c => c.Name == "Hotels 5-4 Stars");
        var formalRest = await db.Categories.FirstOrDefaultAsync(c => c.Name == "Formal Restaurants");
        var tourOps = await db.Categories.FirstOrDefaultAsync(c => c.Name == "Tour Operators");
        var coffee = await db.Categories.FirstOrDefaultAsync(c => c.Name == "Coffee Shops");

        if (hotels54 == null || formalRest == null || tourOps == null || coffee == null) return;

        // --- Demo employers + companies ---
        var employers = new[]
        {
            new { Email = "careers@petragrand.jo", Name = "Layla Haddad",  Company = "Petra Grand Hotel",        City = petra,  Cat = hotels54,   Industry = "Hotels" },
            new { Email = "hr@royalaqaba.jo",      Name = "Omar Al-Rashid", Company = "Royal Aqaba Resort",       City = aqaba,  Cat = hotels54,   Industry = "Resorts" },
            new { Email = "hire@levantbistro.jo",  Name = "Rania Sabbagh",  Company = "Levant Fine Dining",       City = amman,  Cat = formalRest, Industry = "Restaurants" },
            new { Email = "people@wadirumtours.jo",Name = "Yousef Nasser",  Company = "Wadi Rum Expeditions",     City = aqaba,  Cat = tourOps,    Industry = "Tour operators" },
            new { Email = "jobs@rainbowcafe.jo",   Name = "Mariam Khoury",  Company = "Rainbow Street Coffee",    City = amman,  Cat = coffee,     Industry = "Coffee shops" }
        };

        var createdCompanies = new List<(Company Company, string OwnerId)>();

        foreach (var emp in employers)
        {
            var user = new ApplicationUser
            {
                UserName = emp.Email,
                Email = emp.Email,
                EmailConfirmed = true,
                FullName = emp.Name,
                Status = AccountStatus.Active,
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(15, 90))
            };
            var result = await userManager.CreateAsync(user, "Demo@12345");
            if (!result.Succeeded) continue;
            await userManager.AddToRoleAsync(user, RoleNames.Employer);

            var company = new Company
            {
                Name = emp.Company,
                Description = $"{emp.Company} is a leading employer in Jordan's tourism and hospitality sector.",
                Industry = emp.Industry,
                CityId = emp.City?.Id,
                Email = emp.Email,
                IsVerified = true,
                Status = AccountStatus.Active,
                OwnerUserId = user.Id,
                CreatedAt = user.CreatedAt,
                CompanySize = "51-200"
            };
            db.Companies.Add(company);
            await db.SaveChangesAsync();
            createdCompanies.Add((company, user.Id));
        }

        // --- Demo jobs ---
        var titles = new (string Title, string Desc, ExperienceLevel Lvl, decimal Min, decimal Max, JobType Type, bool Urgent)[]
        {
            ("Front Office Manager",    "Lead our reception team and deliver world-class guest experiences.", ExperienceLevel.Senior,   900,  1300, JobType.FullTime,  false),
            ("Executive Chef",          "Design menus, manage kitchen brigade and maintain our 5-star standards.", ExperienceLevel.Lead,  1400, 2200, JobType.FullTime, true),
            ("Housekeeping Supervisor", "Oversee daily housekeeping operations across 120 rooms.", ExperienceLevel.MidLevel, 650,   900, JobType.FullTime, false),
            ("Tour Guide (English)",    "Guide small-group desert tours across Wadi Rum and Petra.", ExperienceLevel.Junior,   500,   800, JobType.FullTime, false),
            ("Barista",                 "Craft premium coffee drinks in a boutique Amman café.", ExperienceLevel.Entry,   380,   520, JobType.FullTime, false),
            ("Waitress / Waiter",       "Serve guests in a fine dining setting.", ExperienceLevel.Entry,  400,  600, JobType.FullTime, false),
            ("Reservations Agent",      "Handle inbound calls and online bookings.", ExperienceLevel.Junior, 500,  700, JobType.FullTime, false),
            ("Sous Chef",               "Support the Executive Chef in daily kitchen operations.", ExperienceLevel.MidLevel, 800,  1200, JobType.FullTime, true),
            ("Guest Relations Officer", "Greet VIP guests and coordinate their experience.", ExperienceLevel.MidLevel, 700,  950, JobType.FullTime, false),
            ("Sales & Marketing Intern","6-month internship in the marketing department.", ExperienceLevel.Entry,  250,  350, JobType.Internship, false)
        };

        var random = new Random(42);
        foreach (var t in titles)
        {
            var owner = createdCompanies[random.Next(createdCompanies.Count)];
            var catRow = new[] { hotels54, formalRest, tourOps, coffee }[random.Next(4)];
            var cityRow = new[] { amman, aqaba, petra }[random.Next(3)];

            db.Jobs.Add(new Job
            {
                Title = t.Title,
                Description = t.Desc,
                Responsibilities = $"· Deliver excellence daily\n· Work with a professional team\n· Maintain brand standards",
                Requirements = $"· {t.Lvl} experience\n· Fluent Arabic, good English\n· Customer-first mindset",
                Benefits = "· Competitive salary\n· Staff meals\n· Career growth",
                CompanyId = owner.Company.Id,
                CategoryId = catRow!.Id,
                CityId = cityRow?.Id,
                JobType = t.Type,
                ExperienceLevel = t.Lvl,
                SalaryMin = t.Min,
                SalaryMax = t.Max,
                Currency = "JOD",
                IsUrgent = t.Urgent,
                IsFeatured = random.Next(3) == 0,
                Vacancies = random.Next(1, 4),
                Status = JobStatus.Active,
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                PublishedAt = DateTime.UtcNow.AddDays(-random.Next(0, 15)),
                Deadline = DateTime.UtcNow.AddDays(random.Next(14, 60))
            });
        }
        await db.SaveChangesAsync();

        // --- Demo job seekers ---
        var seekers = new[]
        {
            new { Email = "salma@example.jo",   Name = "Salma Odeh",    Headline = "Front office agent · 3 yrs experience" },
            new { Email = "tariq@example.jo",   Name = "Tariq Mansour", Headline = "Experienced chef · Mediterranean cuisine" },
            new { Email = "nadia@example.jo",   Name = "Nadia Bishara", Headline = "Bilingual tour guide · Arabic/English/French" }
        };

        foreach (var s in seekers)
        {
            var user = new ApplicationUser
            {
                UserName = s.Email,
                Email = s.Email,
                EmailConfirmed = true,
                FullName = s.Name,
                Status = AccountStatus.Active,
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 45))
            };
            var r = await userManager.CreateAsync(user, "Demo@12345");
            if (!r.Succeeded) continue;
            await userManager.AddToRoleAsync(user, RoleNames.JobSeeker);

            db.JobSeekerProfiles.Add(new JobSeekerProfile
            {
                UserId = user.Id,
                Headline = s.Headline,
                Summary = $"{s.Name} is a passionate tourism professional.",
                YearsOfExperience = Random.Shared.Next(1, 8),
                ExperienceLevel = ExperienceLevel.MidLevel,
                CityId = amman?.Id,
                OpenToWork = true,
                UpdatedAt = DateTime.UtcNow
            });
        }
        await db.SaveChangesAsync();
    }
}
