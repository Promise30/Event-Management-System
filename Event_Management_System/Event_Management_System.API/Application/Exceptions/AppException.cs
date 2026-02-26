using System.Net;

namespace Event_Management_System.API.Application.Exceptions
{
    public abstract class AppException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        protected AppException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
    public sealed class NotFoundException : AppException
    {
        public NotFoundException(string resourceName, object key)
            : base($"{resourceName} with identifier '{key}' was not found.", HttpStatusCode.NotFound)
        {
        }
    }

    public sealed class BadRequestException : AppException
    {
        public BadRequestException(string message)
            : base(message, HttpStatusCode.BadRequest)
        {
        }
    }

    public sealed class ConflictException : AppException
    {
        public ConflictException(string message)
            : base(message, HttpStatusCode.Conflict)
        {
        }
    }

    public sealed class ValidationException : AppException
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException(IDictionary<string, string[]> errors)
            : base("One or more validation errors occurred.", HttpStatusCode.BadRequest)
        {
            Errors = errors;
        }

        public ValidationException(string field, string error)
            : base("One or more validation errors occurred.", HttpStatusCode.BadRequest)
        {
            Errors = new Dictionary<string, string[]>
        {
            { field, [error] }
        };
        }
    }
}
