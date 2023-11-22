using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SerilogSlim.Capturing
{
    static class GetablePropertyFinder
    {
        internal static IEnumerable<PropertyInfo> GetPropertiesRecursive([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] this Type type)
        {
            var seenNames = new HashSet<string>();

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                if (!property.CanRead)
                {
                    continue;
                }

                if (seenNames.Contains(property.Name))
                {
                    continue;
                }

                if (property.Name == "Item" &&
                    property.GetIndexParameters().Length != 0)
                {
                    continue;
                }

                seenNames.Add(property.Name);
                yield return property;
            }
        }
    }
}
