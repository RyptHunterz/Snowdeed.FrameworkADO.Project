using System;

namespace Snowdeed.FrameworkADO.APITest.Dto
{
    public class MovieDto
    {
        public string Title { get; set; }

        public string TitleOriginal { get; set; }

        public DateTime TheatricalRelease { get; set; }

        public DateTime CommercialRelease { get; set; }

        public string Origin { get; set; }

        public int GenreID { get; set; }

        public string Genre { get; set; }
    }
}