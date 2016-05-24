using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace OpenShare.Net.Library.Common
{
    public static class IListExtensions
    {
        /// <summary>
        /// Converts IList to a DataTable.
        /// http://stackoverflow.com/questions/27738238/convert-dbcontext-to-datatable-in-code-first-entity-framework
        /// </summary>
        /// <typeparam name="T">Generic type T.</typeparam>
        /// <param name="data">Data to fill in DataTable.</param>
        /// <returns>A DataTable from the IList of type T.</returns>
        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            var properties = TypeDescriptor.GetProperties(typeof(T));
            var table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(
                    prop.Name,
                    Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

            foreach (var item in data)
            {
                var row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }

            return table;
        }
    }
}
