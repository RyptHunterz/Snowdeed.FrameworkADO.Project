using System;

namespace Snowdeed.FrameworkADO.APITest.DbModels
{
    public class MovieGenre
    {
        public int ID { get; set; }

        public string Genre_Name { get; set; }



        public DateTime Create_At { get; set; }

        public string Create_By { get; set; }

        public DateTime? Delete_At { get; set; }

        public string Delete_By { get; set; }

        public DateTime? Update_At { get; set; }

        public string Update_By { get; set; }
    }
}