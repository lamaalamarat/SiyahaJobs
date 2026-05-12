using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SiyahaJobs.Web.Helpers;
using SiyahaJobs.Web.Models.Entities;
using SiyahaJobs.Web.Models.Enums;
using SiyahaJobs.Web.Services.Interfaces;
using SiyahaJobs.Web.ViewModels.Account;

namespace SiyahaJobs.Web.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ICompanyService _companyService;
    private readonly IEmailService _emailService;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ICompanyService companyService,
        IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _companyService = companyService;
        _emailService = emailService;
    }

    // ---------------------------------------------------------------------
    // LOGIN
    // ---------------------------------------------------------------------
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) =>
        View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials.");
            return View(model);
        }

        if (user.Status != AccountStatus.Active)
        {
            ModelState.AddModelError(string.Empty, $"Your account is currently {user.Status}. Please contact support.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account locked. Try again later.");
            return View(model);
        }

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials.");
            return View(model);
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToRoleHome(user);
    }

    // ---------------------------------------------------------------------
    // REGISTER
    // ---------------------------------------------------------------------
    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // Only Employer and JobSeeker can self-register.
        if (model.Role != RoleNames.Employer && model.Role != RoleNames.JobSeeker)
        {
            ModelState.AddModelError(nameof(model.Role), "Invalid role selection.");
            return View(model);
        }

        if (model.Role == RoleNames.Employer && string.IsNullOrWhiteSpace(model.CompanyName))
        {
            ModelState.AddModelError(nameof(model.CompanyName), "Company name is required for employer accounts.");
            return View(model);
        }

        if (await _userManager.FindByEmailAsync(model.Email) != null)
        {
            ModelState.AddModelError(nameof(model.Email), "An account already exists with this email.");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            foreach (var err in createResult.Errors)
                ModelState.AddModelError(string.Empty, err.Description);
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, model.Role);

        // Create role-specific profile/company
        if (model.Role == RoleNames.JobSeeker)
        {
            // JobSeekerProfile will be auto-created on first profile edit; keep optional.
        }
        else if (model.Role == RoleNames.Employer)
        {
            await _companyService.CreateOrUpdateAsync(new Company
            {
                Name = model.CompanyName!.Trim(),
                Email = model.Email,
                Phone = model.PhoneNumber,
                Status = AccountStatus.Active
            }, user.Id);
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        TempData["Success"] = $"Welcome to SiyahaJobs, {user.FullName}!";

        return RedirectToRoleHome(user);
    }

    // ---------------------------------------------------------------------
    // LOGOUT
    // ---------------------------------------------------------------------
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login), "Account");
    }

    // ---------------------------------------------------------------------
    // FORGOT PASSWORD
    // ---------------------------------------------------------------------
    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callback = Url.Action(nameof(ResetPassword), "Account",
                new { email = model.Email, token }, Request.Scheme);

            await _emailService.SendAsync(model.Email,
                "Reset your SiyahaJobs password",
                $@"<p>Hi {user.FullName},</p>
                   <p>You requested a password reset. Click the link below to set a new password:</p>
                   <p><a href='{callback}' style='background:#1F9E78;color:#fff;padding:10px 16px;border-radius:8px;text-decoration:none'>Reset password</a></p>
                   <p>If you didn't request this, please ignore this email.</p>");
        }

        TempData["Success"] = "If the email exists in our system, we've sent reset instructions.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ResetPassword(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            return BadRequest("Invalid reset link.");
        return View(new ResetPasswordViewModel { Email = email, Token = token });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            TempData["Success"] = "Password reset successful. You can now log in.";
            return RedirectToAction(nameof(Login));
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError(string.Empty, err.Description);
            return View(model);
        }

        TempData["Success"] = "Password reset successful. You can now log in.";
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied() => View();

    // ---------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------
    private IActionResult RedirectToRoleHome(ApplicationUser user)
    {
        // Identity in-memory role check requires role claim; use UserManager for safety.
        var roles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult();
        if (roles.Contains(RoleNames.Admin))     return RedirectToAction("Index", "Admin");
        if (roles.Contains(RoleNames.Employer))  return RedirectToAction("Index", "Employer");
        return RedirectToAction("Index", "JobSeeker");
    }
}
