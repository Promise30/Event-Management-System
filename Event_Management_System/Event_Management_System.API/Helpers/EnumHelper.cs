using System.ComponentModel;

namespace Event_Management_System.API.Helpers
{
    public static class EnumHelper
    {
        public static string GetEnumDescription(this Enum value)
        {
            if (Attribute.GetCustomAttribute(value.GetType().GetField(value.ToString()), typeof(DescriptionAttribute)) is DescriptionAttribute descriptionAttribute)
            {
                return descriptionAttribute.Description;
            }

            return value.ToString();
        }
    }
}
