using System;

namespace Snowdeed.FrameworkADO.APITest.Dto.Extensions
{
    using Snowdeed.FrameworkADO.APITest.DbModels;

    public static class MovieDtoExtension
    {
        public static MovieDto ConvertToMovieDto(this Movie entity)
        {
            if (entity == null)
                return null;

            var artistDto = new MovieDto()
            {
                Title = entity.Title,
                TitleOriginal = entity.TitleOriginal,
                TheatricalRelease = entity.TheatricalRelease,
                CommercialRelease = entity.CommercialRelease,
                Origin = entity.Origin,
                GenreID = entity.GenreID
            };

            return artistDto;
        }

        public static Movie ConvertToMovie(this MovieDto dto)
        {
            if (dto == null)
                return null;

            var artist = new Movie()
            {
                ID = Guid.NewGuid(),
                Title = dto.Title,
                TitleOriginal = dto.TitleOriginal,
                TheatricalRelease = dto.TheatricalRelease,
                CommercialRelease = dto.CommercialRelease,
                Origin = dto.Origin,
                GenreID = dto.GenreID
            };

            return artist;
        }
    }
}
