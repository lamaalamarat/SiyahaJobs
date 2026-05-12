namespace SiyahaJobs.Web.Helpers;

/// <summary>
/// Centralized role name constants used across Identity, authorization policies and UI.
/// </summary>
public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Employer = "Employer";
    public const string JobSeeker = "JobSeeker";

    public static readonly string[] All = { Admin, Employer, JobSeeker };
}
