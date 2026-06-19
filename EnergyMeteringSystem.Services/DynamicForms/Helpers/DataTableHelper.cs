using System;
using System.Collections.Generic;
using System.Data;

namespace EnergyMeteringSystem.Services.DynamicForms.Helpers
{
    internal static class DataTableHelper
    {
        public static Dictionary<string, object> ToDictionary(DataRow row)
        {
            if (row == null)
                return null;

            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (DataColumn column in row.Table.Columns)
            {
                var value = row[column];
                dict[column.ColumnName] = value == DBNull.Value ? null : value;
            }

            return dict;
        }

        public static List<Dictionary<string, object>> ToDictionaryList(DataTable table)
        {
            var list = new List<Dictionary<string, object>>();
            if (table == null)
                return list;

            foreach (DataRow row in table.Rows)
            {
                var dict = ToDictionary(row);
                if (dict != null && dict.Count > 0)
                    list.Add(dict);
            }

            return list;
        }
    }
}
