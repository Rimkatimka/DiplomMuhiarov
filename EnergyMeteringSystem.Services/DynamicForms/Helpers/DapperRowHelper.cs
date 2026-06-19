using System;
using System.Collections.Generic;
using System.Linq;

namespace EnergyMeteringSystem.Services.DynamicForms.Helpers
{
    internal static class DapperRowHelper
    {
        public static Dictionary<string, object> ToDictionary(object row)
        {
            if (row == null)
                return null;

            if (row is Dictionary<string, object> dictionary)
                return new Dictionary<string, object>(dictionary, StringComparer.OrdinalIgnoreCase);

            if (row is IDictionary<string, object> idictionary)
            {
                return idictionary.ToDictionary(
                    kvp => kvp.Key,
                    kvp => NormalizeValue(kvp.Value),
                    StringComparer.OrdinalIgnoreCase);
            }

            if (row is IEnumerable<KeyValuePair<string, object>> pairs)
            {
                return pairs.ToDictionary(
                    kvp => kvp.Key,
                    kvp => NormalizeValue(kvp.Value),
                    StringComparer.OrdinalIgnoreCase);
            }

            var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in row.GetType().GetProperties())
            {
                if (!property.CanRead || property.GetIndexParameters().Length > 0)
                    continue;

                result[property.Name] = NormalizeValue(property.GetValue(row));
            }

            return result;
        }

        public static List<Dictionary<string, object>> ToDictionaryList(IEnumerable<dynamic> rows)
        {
            if (rows == null)
                return new List<Dictionary<string, object>>();

            return rows
                .Select(row => (object)row)
                .Select(ToDictionary)
                .Where(d => d != null && d.Count > 0)
                .ToList();
        }

        private static object NormalizeValue(object value)
        {
            return value == null || value is DBNull ? null : value;
        }
    }
}
