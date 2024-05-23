using System.Reflection;

namespace gView.Razor.Extensions;

static internal class TypeExtensions
{
    static public bool IsFlagsEnum(this Type enumType)
    {
        if (!enumType.IsEnum)
        {
            return false;
        }

        return enumType.GetCustomAttribute<FlagsAttribute>() is not null;
    }

    static public HashSet<object> EnumsToHashset(this Type enumType, int? val)
    {
        var hashset = new HashSet<object>();

        if (val.HasValue && enumType.IsEnum) 
        {
            if(enumType.IsFlagsEnum())
            {
                foreach(var enumVal in Enum.GetValues(enumType))
                {
                    if ((int)enumVal == 0 && val.Value > 0)
                    {
                        // ignore enum value 0 (mostly "none" or "regular")
                        // if other flags are set
                        continue;
                    }

                    if((val.Value & (int)enumVal) == (int)enumVal)
                    {
                        hashset.Add(enumVal);
                    }
                }
            } 
            else
            {
                hashset.Add(val);
            }
        }

        return hashset;
    } 
}
