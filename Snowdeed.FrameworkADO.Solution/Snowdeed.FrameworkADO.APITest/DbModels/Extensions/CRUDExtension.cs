using System;

namespace Snowdeed.FrameworkADO.APITest.DbModels.Extensions
{
    using Snowdeed.FrameworkADO.Core.Attributes;

    public class CRUDExtension
    {
        [NotNull]
        public DateTime Create_At { get; set; }

        [NotNull]
        public string Create_By { get; set; }

        public DateTime Delete_At { get; set; }

        public string Delete_By { get; set; }

        public DateTime Update_At { get; set; }

        public string Update_By { get; set; }
    }
}