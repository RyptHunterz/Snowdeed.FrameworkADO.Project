using Snowdeed.FrameworkADO.Core.Attributes;
using Snowdeed.FrameworkADO.TestAPI.Enums;

namespace Snowdeed.FrameworkADO.TestAPI.Entities
{
    public class Employee
    {
        [Identity]
        public int EmployeeId { get; set; }
        [PrimaryKey]
        public Guid EmployeeGuid { get; set; }
        [NotNull]
        [MaxLenght(50)]
        public required string Surname { get; set; }
        [NotNull]
        [MaxLenght(50)]
        public required string Firstname { get; set; }
        public GenderEnum Gender { get; set; }
        public string? Matricule { get; set; }
        public string? Position { get; set; }
    }
}
