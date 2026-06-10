using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;

namespace EnergyMeteringSystem.App.Controls
{
    public class SimpleEmailMaskedTextBox : TextBox
    {
        public SimpleEmailMaskedTextBox()
        {
            PreviewTextInput += OnPreviewTextInput;
            DataObject.AddPastingHandler(this, OnPaste);
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только буквы, цифры и спецсимволы для email
            string allowedChars = @"[a-zA-Z0-9@._%-]";
            e.Handled = !Regex.IsMatch(e.Text, allowedChars);
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                string text = (string)e.DataObject.GetData(DataFormats.Text);
                // Очищаем вставляемый текст от недопустимых символов
                string allowedChars = Regex.Replace(text, @"[^a-zA-Z0-9@._%-]", "");

                if (string.IsNullOrEmpty(allowedChars))
                {
                    e.CancelCommand();
                }
                else
                {
                    e.DataObject.SetData(DataFormats.Text, allowedChars);
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true; // Запрещаем пробел
            }
            base.OnKeyDown(e);
        }
    }
}
