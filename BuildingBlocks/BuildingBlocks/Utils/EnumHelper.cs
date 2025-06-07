using System.ComponentModel;
using System.Reflection;

namespace BuildingBlocks.Utils
{
    public static class EnumHelper
    {
        public static string? GetEnumDescription(this Enum value)
        {
            if (value == null) return null;

            var fieldInfo = value.GetType().GetField(value.ToString());
            var description = fieldInfo?
                .GetCustomAttribute<DescriptionAttribute>(false)?
                .Description;

            return description ?? value.ToString();
        }

        public static string? GetEnumDescriptionOrNull(this Enum? value)
        {
            return value == null ? null : GetEnumDescription(value);
        }
    }
}
