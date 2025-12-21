using System.ComponentModel;

namespace Event_Management_System.API.Domain.Enums
{
    public enum ActionType
    {
        [Description("Create")]
        Create = 0,
        [Description("Update")]
        Update = 1,
        [Description("Delete")]
        Delete = 2,
    }
}
