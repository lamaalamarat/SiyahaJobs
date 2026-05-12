namespace SiyahaJobs.Web.Helpers;

public class OperationResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public IList<string> Errors { get; set; } = new List<string>();

    public static OperationResult Ok(string? message = null) =>
        new() { Success = true, Message = message };

    public static OperationResult Fail(string message) =>
        new() { Success = false, Message = message, Errors = { message } };

    public static OperationResult Fail(IEnumerable<string> errors) =>
        new() { Success = false, Errors = errors.ToList(), Message = string.Join("; ", errors) };
}

public class OperationResult<T> : OperationResult
{
    public T? Data { get; set; }

    public static OperationResult<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public new static OperationResult<T> Fail(string message) =>
        new() { Success = false, Message = message, Errors = { message } };
}
