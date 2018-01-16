using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TouchRemote.Utils.ValidationRules
{
    internal class IntegerValueValidationRule : ValidationRule
    {
        public string ErrorMessage { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string str = value as string;
            int n;
            return !string.IsNullOrWhiteSpace(str) && int.TryParse(str, out n) ? ValidationResult.ValidResult : new NotAnIntegerValidationResult(ErrorMessage);
        }

        public class NotAnIntegerValidationResult : ValidationResult
        {
            public NotAnIntegerValidationResult(string errorMessage) : base(false, errorMessage) { }
        }
    }
}
