using EnergyMeteringSystem.Services.DynamicForms.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace EnergyMeteringSystem.Services.DynamicForms.Services
{
    public interface IFormBuilder
    {
        Task<FormResult> BuildFormAsync(TableMetadata metadata, Dictionary<string, object> data = null);
        Dictionary<string, object> CollectDataFromForm(FormResult formResult);
    }

    public class FormResult
    {
        public Grid FormGrid { get; set; }
        public Dictionary<string, FrameworkElement> Controls { get; set; } = new();
        public List<string> RequiredFields { get; set; } = new();
    }
}