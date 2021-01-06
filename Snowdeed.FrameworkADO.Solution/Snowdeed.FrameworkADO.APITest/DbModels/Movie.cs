using System;

namespace Snowdeed.FrameworkADO.APITest.DbModels
{
    public class Movie
    {
        public Guid ID { get; set; }

        public string Title { get; set; }

        public string TitleOriginal { get; set; }

        public DateTime TheatricalRelease { get; set; }

        public DateTime CommercialRelease { get; set; }

        public string Origin { get; set; }

        public int GenreID { get; set; }



        public DateTime Create_At { get; set; }

        public string Create_By { get; set; }

        public DateTime? Delete_At { get; set; }

        public string Delete_By { get; set; }

        public DateTime? Update_At { get; set; }

        public string Update_By { get; set; }
    }
}