namespace MichaelPage.Common.Models;

public class Result
{
    public bool Success { get; }
    public string Message { get; set; }
    public string Details { get; set; } 
    public string Code { get; set; }
    public IDictionary<string, string[]> ValidationErrors { get; set; }

    protected Result(bool success, string message, string details, string code = null,
        IDictionary<string, string[]> errors = null)
    {
        Success = success;
        Message = message;
        Details = details;
        Code = code;
        ValidationErrors = errors;
    }

    public static Result Fail()
    {
        return new Result(false, null, null);
    }
    
    public static Result Fail(string message, string description = null, string code = null)
    {
        return new Result(false, message, description, code);
    }
  
    public static Result<T> Fail<T>(string message, string description = null, string code = null)
    {
        return new Result<T>(default, false, message, description, code);
    }

    public static Result Fail(IDictionary<string, string[]> errors)
    {
        return new Result(false, null, null, null, errors);
    }

    public static Result Ok()
    {
        return new Result(true, null, null);
    }
   
    public static Result Ok(string message, string description = null, string code = null)
    {
        return new Result(true, message, description, code);
    }
    
    public static Result<T> Ok<T>(T value)
    {
        return new Result<T>(value, true, null, null, null);
    }
    
    public static Result<T> Ok<T>(string message, string description = null, string code = null)
    {
        return new Result<T>(default, true, message, description, code);
    }
}

public class Result<T> : Result
{
    protected internal Result(T data, bool success, string message, string details, string code) : base(success,
        message, details, code)
    {
        Data = data;
    }

    public T Data { get; set; }
}