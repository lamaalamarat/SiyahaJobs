namespace SiyahaJobs.Web.Models.Enums;

public enum JobType
{
    FullTime = 1,
    PartTime = 2,
    Contract = 3,
    Internship = 4,
    Temporary = 5,
    Seasonal = 6
}

public enum ExperienceLevel
{
    Entry = 1,
    Junior = 2,
    MidLevel = 3,
    Senior = 4,
    Lead = 5,
    Executive = 6
}

public enum JobStatus
{
    /// <summary>Job was created but awaits admin approval before being public.</summary>
    PendingApproval = 0,
    Active = 1,
    Paused = 2,
    Closed = 3,
    Rejected = 4
}

public enum ApplicationStatus
{
    Submitted = 0,
    UnderReview = 1,
    Shortlisted = 2,
    InterviewScheduled = 3,
    Accepted = 4,
    Rejected = 5,
    Withdrawn = 6
}

public enum InterviewStatus
{
    Scheduled = 0,
    Completed = 1,
    Cancelled = 2,
    Rescheduled = 3
}

public enum AccountStatus
{
    Active = 1,
    Suspended = 2,
    Banned = 3
}

public enum NotificationType
{
    General = 0,
    ApplicationUpdate = 1,
    NewApplicant = 2,
    InterviewScheduled = 3,
    Message = 4,
    JobApproved = 5,
    JobRejected = 6,
    AccountAlert = 7
}

public enum Gender
{
    NotSpecified = 0,
    Male = 1,
    Female = 2
}
