// EnergyMeteringSystem.Services/DynamicForms/Builders/DynamicFormBuilder.cs
using EnergyMeteringSystem.Services.DynamicForms.Models;
using EnergyMeteringSystem.Services.DynamicForms.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;

namespace EnergyMeteringSystem.Services.DynamicForms.Builders
{
    public class DynamicFormBuilder : IFormBuilder
    {
        private readonly IDynamicRepository _repository;

        public DynamicFormBuilder(IDynamicRepository repository)
        {
            _repository = repository;
        }

        public async Task<FormResult> BuildFormAsync(TableMetadata metadata, Dictionary<string, object> data = null)
        {
            var result = new FormResult();
            var grid = new Grid
            {
                Margin = new Thickness(0, 0, 0, 10)
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(190) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            int row = 0;
            foreach (var column in metadata.Columns)
            {
                if (column.IsIdentity)
                    continue;

                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var labelText = string.IsNullOrWhiteSpace(column.RussianName)
                    ? column.ColumnName
                    : column.RussianName;

                if (!column.IsNullable)
                    labelText += " *";

                var label = new TextBlock
                {
                    Text = labelText,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5, 8, 12, 8),
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brushes.Black,
                    TextWrapping = TextWrapping.Wrap
                };
                Grid.SetRow(label, row);
                Grid.SetColumn(label, 0);

                var control = await CreateControlAsync(column);
                control.Margin = new Thickness(5, 8, 5, 8);
                control.MinHeight = 28;

                if (data != null)
                {
                    object cellValue = null;
                    if (!data.TryGetValue(column.ColumnName, out cellValue))
                    {
                        foreach (var kvp in data)
                        {
                            if (string.Equals(kvp.Key, column.ColumnName, StringComparison.OrdinalIgnoreCase))
                            {
                                cellValue = kvp.Value;
                                break;
                            }
                        }
                    }

                    SetControlValue(control, cellValue, column);
                }

                Grid.SetRow(control, row);
                Grid.SetColumn(control, 1);

                grid.Children.Add(label);
                grid.Children.Add(control);

                result.Controls[column.ColumnName] = control;
                if (!column.IsNullable && !column.IsIdentity)
                    result.RequiredFields.Add(column.ColumnName);

                row++;
            }

            result.FormGrid = grid;
            return result;
        }

        private async Task<FrameworkElement> CreateControlAsync(ColumnMetadata column)
        {
            if (column.ControlType == ControlType.ComboBox)
            {
                var comboBox = new ComboBox
                {
                    DisplayMemberPath = "DisplayName",
                    SelectedValuePath = "Id",
                    MinWidth = 220,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var refTable = column.ReferencedTable;
                if (string.IsNullOrWhiteSpace(refTable) && column.ColumnName.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                    refTable = column.ColumnName.Substring(0, column.ColumnName.Length - 2);

                if (!string.IsNullOrWhiteSpace(refTable))
                {
                    var items = await _repository.GetComboBoxDataAsync(refTable);
                    foreach (var item in items)
                        comboBox.Items.Add(item);
                }

                comboBox.SetValue(AutomationProperties.NameProperty, column.ColumnName);
                return comboBox;
            }

            return column.ControlType switch
            {
                ControlType.CheckBox => CreateCheckBox(column),
                ControlType.NumericTextBox => CreateNumericBox(column),
                ControlType.DatePicker => CreateDatePicker(column),
                _ => CreateTextBox(column)
            };
        }

        private FrameworkElement CreateTextBox(ColumnMetadata column)
        {
            var textBox = new TextBox
            {
                MinWidth = 220,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(6, 4, 6, 4)
            };

            if (column.MaxLength.HasValue && column.MaxLength.Value < 4000)
                textBox.MaxLength = column.MaxLength.Value;

            textBox.SetValue(AutomationProperties.NameProperty, column.ColumnName);

            if (column.DataType == "nvarchar" && column.ColumnName.Contains("Color"))
                textBox.ToolTip = "Формат: #RRGGBB";

            return textBox;
        }

        private FrameworkElement CreateNumericBox(ColumnMetadata column)
        {
            var textBox = new TextBox
            {
                MinWidth = 220,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(6, 4, 6, 4)
            };
            textBox.PreviewTextInput += (s, e) =>
                e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[0-9.,]$");

            textBox.SetValue(AutomationProperties.NameProperty, column.ColumnName);
            textBox.ToolTip = "Только цифры";
            return textBox;
        }

        private FrameworkElement CreateCheckBox(ColumnMetadata column)
        {
            var checkBox = new CheckBox
            {
                VerticalAlignment = VerticalAlignment.Center
            };
            checkBox.SetValue(AutomationProperties.NameProperty, column.ColumnName);
            checkBox.VerticalAlignment = VerticalAlignment.Center;
            return checkBox;
        }

        private FrameworkElement CreateDatePicker(ColumnMetadata column)
        {
            var datePicker = new DatePicker
            {
                MinWidth = 220,
                VerticalAlignment = VerticalAlignment.Center
            };
            datePicker.SetValue(AutomationProperties.NameProperty, column.ColumnName);
            return datePicker;
        }

        private void SetControlValue(FrameworkElement control, object value, ColumnMetadata column)
        {
            if (value == DBNull.Value || value == null) return;

            switch (control)
            {
                case TextBox textBox:
                    textBox.Text = value.ToString();
                    break;
                case CheckBox checkBox:
                    checkBox.IsChecked = Convert.ToBoolean(value);
                    break;
                case ComboBox comboBox:
                    comboBox.SelectedValue = Convert.ToInt32(value);
                    break;
                case DatePicker datePicker:
                    if (value is DateTime dt)
                        datePicker.SelectedDate = dt;
                    break;
            }
        }

        public Dictionary<string, object> CollectDataFromForm(FormResult formResult)
        {
            var values = new Dictionary<string, object>();

            foreach (var kvp in formResult.Controls)
            {
                var control = kvp.Value;
                var columnName = kvp.Key;

                object value = control switch
                {
                    TextBox tb => string.IsNullOrWhiteSpace(tb.Text) ? null : tb.Text,
                    CheckBox cb => cb.IsChecked ?? false,
                    ComboBox cmb => cmb.SelectedValue ?? (object)DBNull.Value,
                    DatePicker dp => dp.SelectedDate,
                    _ => null
                };

                if (value != null && value != DBNull.Value)
                {
                    values[columnName] = value;
                }
            }

            return values;
        }
    }
}