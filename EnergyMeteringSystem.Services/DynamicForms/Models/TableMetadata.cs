using System.Collections.Generic;

namespace EnergyMeteringSystem.Services.DynamicForms.Models
{
    public class TableMetadata
    {
        public string TableName { get; set; }
        public string RussianName { get; set; }
        public List<ColumnMetadata> Columns { get; set; } = new();
        public ColumnMetadata PrimaryKey { get; set; }
    }
}
