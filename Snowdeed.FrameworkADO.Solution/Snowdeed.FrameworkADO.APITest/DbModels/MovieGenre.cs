using System;

namespace Snowdeed.FrameworkADO.APITest.DbModels
{
    using Snowdeed.FrameworkADO.APITest.DbModels.Extensions;
    using Snowdeed.FrameworkADO.Core.Attributes;

    public class MovieGenre : CRUDExtension
    {
        [Identity]
        public int ID { get; set; }

        [NotNull]
        [MaxLenght(50)]
        public string Genre_Name { get; set; }
    }
}