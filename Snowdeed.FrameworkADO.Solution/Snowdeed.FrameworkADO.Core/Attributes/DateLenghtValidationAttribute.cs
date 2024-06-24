using System.ComponentModel.DataAnnotations;

namespace Snowdeed.FrameworkADO.Core.Attributes;

public class DateLenghtValidationAttribute : ValidationAttribute
{
    public int MinNbrYear { get; set; }
    public int MaxNbrYear { get; set; }

    public override bool IsValid(object? value)
    {
        if (value == null) return true;

        var val = (DateTime)value;

        if (val.AddYears(MinNbrYear) > DateTime.Now) return false;

        return (val.AddYears(MaxNbrYear) > DateTime.Now);
    }
}