# SiyahaJobs — Jordan's Tourism & Hospitality Career Hub

A production-grade recruitment platform built with **ASP.NET Core MVC 8**, **Entity Framework Core**, **SQL Server**, and **ASP.NET Identity**.

Dedicated to Jordan's tourism and hospitality sector — hotels, restaurants, tour operators, coffee shops, transportation, and more.

---

## Highlights

- Clean Architecture folder layout
- Repository + Unit of Work pattern
- Service layer with DI
- ASP.NET Identity (roles: **Admin**, **Employer**, **JobSeeker**)
- Role-based authorization policies
- Three complete portals:
  - **Public site** — landing page with hero, categories slider, insights bar, featured jobs, partners, job search + details
  - **Job Seeker portal** — profile, CV upload, applications tracking, saved jobs, notifications
  - **Employer portal** — company profile, jobs CRUD (Pause/Resume/Close), applicants management, interview scheduling, dashboard analytics
  - **Admin portal** — platform analytics with Chart.js, user moderation, job approval workflow, category management, company verification
- Full email/SMTP support with dev-mode log fallback
- Secure file uploads (avatars, logos, CVs) with size + type validation
- CSRF tokens on every mutation (`AutoValidateAntiforgeryToken`)
- Responsive UI built with a custom CSS design system (brand colors `#1F9E78` green + `#194870` navy)

---

## Tech stack

| Layer          | Technology                                        |
|----------------|---------------------------------------------------|
| Runtime        | .NET 8                                            |
| Framework      | ASP.NET Core MVC                                  |
| ORM            | Entity Framework Core 8                           |
| Database       | SQL Server (Express or full)                      |
| Auth           | ASP.NET Identity                                  |
| Frontend       | Razor + Bootstrap-style custom CSS + Chart.js     |
| Validation     | DataAnnotations + jQuery-Validation (unobtrusive) |

---

## Getting started

### 1. Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server Express (or any SQL Server 2019+)
- Visual Studio 2022 / Rider / VS Code

### 2. Clone & restore

```bash
git clone https://github.com/lamaalamarat/SiyahaJobs.git
cd SiyahaJobs
dotnet restore
```

### 3. Configure the connection string

`src/SiyahaJobs.Web/appsettings.json` ships with:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=SiyahaJobs;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

Adjust if your SQL Server instance or authentication differs.

### 4. Create & migrate the database

```bash
cd src/SiyahaJobs.Web
dotnet tool install --global dotnet-ef      # once, if not installed
dotnet ef migrations add InitialCreate
dotnet ef database update
```

> The app also auto-creates the database on first run (`EnsureCreated`) if no migrations are present, and always seeds roles, admin, cities and categories.

### 5. Run

```bash
dotnet run --project src/SiyahaJobs.Web
```

Then open `https://localhost:7060` (or `http://localhost:5060`).

### 6. Default admin account

```
Email:    admin@siyahajobs.jo
Password: Admin@123456
```

You can change these in `appsettings.json` under `AdminUser` **before** the first run.

---

## Project structure

```
SiyahaJobs/
├── SiyahaJobs.sln
└── src/SiyahaJobs.Web/
    ├── Controllers/               HomeController, AccountController, JobsController,
    │                              CompaniesController, JobSeekerController,
    │                              EmployerController, AdminController
    ├── Data/
    │   ├── ApplicationDbContext.cs
    │   └── Seed/DatabaseSeeder.cs
    ├── Helpers/                   RoleNames, CurrentUserAccessor, OperationResult,
    │                              PagedResult, BrandColors
    ├── Models/
    │   ├── Entities/              Domain entities
    │   └── Enums/                 JobType, JobStatus, ApplicationStatus, …
    ├── Repositories/
    │   ├── Interfaces/            IGenericRepository, IJobRepository, etc.
    │   ├── GenericRepository.cs
    │   ├── JobRepository.cs
    │   ├── JobApplicationRepository.cs
    │   ├── CompanyRepository.cs
    │   └── UnitOfWork.cs
    ├── Services/
    │   ├── Interfaces/
    │   └── *Service.cs            JobService, ApplicationService, CompanyService,
    │                              AdminService, NotificationService,
    │                              FileUploadService, EmailService, LookupService
    ├── ViewModels/                Account, Admin, Company, Dashboard, Employer,
    │                              Home, JobSeeker, Jobs
    ├── Views/
    │   ├── Shared/                _Layout, _DashboardLayout, _JobCardPartial,
    │   │                          _ToastPartial, _ValidationScriptsPartial
    │   ├── Home/                  Index (landing), About, Contact, Error
    │   ├── Account/               Login, Register, ForgotPassword, ResetPassword
    │   ├── Jobs/                  Index, Details, Apply
    │   ├── Companies/             Index, Details
    │   ├── JobSeeker/             Index, Profile, Applications, SavedJobs,
    │   │                          Notifications
    │   ├── Employer/              Index, Company, Jobs, JobEditor, Applicants,
    │   │                          ScheduleInterview
    │   └── Admin/                 Index, Users, Jobs, RejectJob, Companies,
    │                              Categories, CategoryEditor
    ├── wwwroot/
    │   ├── css/                   site.css, dashboard.css
    │   ├── js/                    site.js, dashboard.js
    │   └── uploads/               avatars/, logos/, cvs/   (gitkeep + runtime)
    ├── appsettings.json
    ├── Program.cs
    └── SiyahaJobs.Web.csproj
```

---

## Seeded data

On first run, the seeder populates:

- 3 **roles** — `Admin`, `Employer`, `JobSeeker`
- 1 default **admin user** (from `appsettings.json`)
- 1 country — **Jordan**
- 13 Jordanian cities — Amman, Zarqa, Irbid, Aqaba, Madaba, Karak, Salt, Jerash, Ajloun, Ma'an, Tafilah, Mafraq, Petra
- 12 tourism categories — Formal Restaurants, Casual Restaurants, Coffee Shops, Hotels 5-4 Stars, Hotels 1-3 Stars, Fast Food, Entertainment & Leisure, Hotel Apartments, Tour Operators, Bazaars & Handcrafts, Tourism Transportation, Tourism Offices
- 3 partner placeholder rows (admin manages them)

---

## Key workflows

### Job seeker
1. Register as **Job Seeker**
2. Complete the profile (CV, skills, headline)
3. Browse or search jobs → Apply → Track status in **My Applications**
4. Save interesting jobs, receive notifications about application updates

### Employer
1. Register as **Employer** (company name is required)
2. Complete the **Company** profile (logo, description, contact)
3. Post jobs → **submitted for admin approval**
4. Once approved, jobs are live; view **Applicants** per job, change their status, **schedule interviews**
5. Pause / resume / close jobs anytime

### Admin
1. Sign in with seeded credentials
2. Moderate **pending jobs** (approve or reject with reason)
3. Manage **users** (suspend, ban, delete)
4. Verify **companies**, delete if needed
5. Manage tourism **categories** shown on the homepage
6. Monitor platform via Chart.js analytics (jobs, users, applications per month; top categories)

---

## Security

- ASP.NET Identity with hashed passwords, lockout after 5 failed attempts
- CSRF protection on all form POSTs via `AutoValidateAntiforgeryToken`
- Role-based authorization at the controller level (`[Authorize(Roles = ...)]`)
- All mutating endpoints scope resources to the current user (employer can only edit their own jobs; job seekers can only withdraw their own applications)
- Secure file upload: random GUID filenames, type + size validation from configuration
- HTTPS redirection & HSTS enabled in production

---

## Configuration reference (`appsettings.json`)

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=SiyahaJobs;..."
  },
  "AdminUser": {
    "Email": "admin@siyahajobs.jo",
    "Password": "Admin@123456",
    "FullName": "Platform Administrator"
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "no-reply@siyahajobs.jo",
    "SenderName": "SiyahaJobs",
    "Username": "",
    "Password": "",
    "EnableSsl": true
  },
  "FileUpload": {
    "MaxAvatarSizeMB": 2,
    "MaxLogoSizeMB": 3,
    "MaxCvSizeMB": 5,
    "AllowedImageExtensions": [ ".jpg", ".jpeg", ".png", ".webp" ],
    "AllowedCvExtensions": [ ".pdf", ".doc", ".docx" ]
  }
}
```

> Leave SMTP credentials empty in development — emails (including password-reset links) will be written to the application log instead of being sent.

---

## Brand & design

- **Primary green**: `#1F9E78` (CTAs, active states, accents)
- **Primary navy**: `#194870` (headings, logos, dashboard stat numbers)
- **Typography**: Plus Jakarta Sans (via Google Fonts)
- Soft shadows, rounded cards, generous white space, responsive grids

---

## License

Built as a production template. Adapt freely for your use case.
