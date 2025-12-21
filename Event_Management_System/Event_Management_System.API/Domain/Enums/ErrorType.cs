using System.ComponentModel;

namespace Event_Management_System.API.Domain.Enums
{
    public enum ErrorType
    {
        [Description("REQUEST_NOT_VALID")]
        RequestNotValid = 10400,
        [Description("UNAUTHORIZATION")]
        Unauthorized = 10401,
        [Description("FORBIDDEN")]
        Forbidden = 10403,
        [Description("RESOURCE_NOT_FOUND")]
        ResourceNotFound = 10404,
        [Description("RESOURCE_CONFLICT")]
        ResourceConflict = 10409,
        [Description("UNPROCESSABLE")]
        Unprocessable = 10422,
        [Description("INTERNAL_SERVER_ERROR")]
        InternalServerError = 10500
    }
}
