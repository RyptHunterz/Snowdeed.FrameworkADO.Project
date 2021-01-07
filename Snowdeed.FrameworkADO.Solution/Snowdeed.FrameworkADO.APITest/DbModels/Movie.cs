using System;

namespace Snowdeed.FrameworkADO.APITest.DbModels
{
    using Snowdeed.FrameworkADO.APITest.DbModels.Extensions;
    using Snowdeed.FrameworkADO.Core.Attributes;

    public class Movie : CRUDExtension
    {
        [Identity]
        public Guid ID { get; set; }

        [NotNull]
        [MaxLenght(50)]
        public string Title { get; set; }

        [NotNull]
        [MaxLenght(50)]
        public string TitleOriginal { get; set; }

        [NotNull]
        public DateTime TheatricalRelease { get; set; }

        [NotNull]
        public DateTime CommercialRelease { get; set; }

        [NotNull]
        [MaxLenght(50)]
        public string Origin { get; set; }

        public int GenreID { get; set; }
    }
}