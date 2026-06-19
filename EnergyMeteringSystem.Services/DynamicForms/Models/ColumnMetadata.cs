namespace EnergyMeteringSystem.Services.DynamicForms.Models
{
    public class ColumnMetadata
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public int? MaxLength { get; set; }
        public bool IsNullable { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsForeignKey { get; set; }
        public string ReferencedTable { get; set; }
        public string RussianName { get; set; }
        public ControlType ControlType { get; set; }
    }

    public enum ControlType
    {
        TextBox,
        NumericTextBox,
        CheckBox,
        ComboBox,
        DatePicker
    }
}