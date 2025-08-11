namespace FLIP.Application.Models;

public class ResponseVM<T>
{
    public string Status { get; set; } = "success";
    public ApiError? Error { get; set; }
    public T Data { get; set; } = default!;
}

public class ApiError
{
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorDescription { get; set; } = string.Empty;
    public string? SubError { get; set; }
    public List<ValidationError> Validations { get; set; } = [];
}

public class ValidationError
{
    public string PropertyName { get; set; } = string.Empty;
    public List<string> ValidationMessage { get; set; } = new();
}
