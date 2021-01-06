using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Snowdeed.FrameworkADO.APITest.Controllers
{
    using Snowdeed.FrameworkADO.APITest.Core;
    using Snowdeed.FrameworkADO.APITest.Dto;
    using Snowdeed.FrameworkADO.APITest.Dto.Extensions;

    [Route("api/music")]
    public class MovieController : Controller
    {
        public IConfiguration Configuration { get; }

        public MovieController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [HttpGet]
        public IEnumerable<MovieDto> Get()
        {
            using (MovieDbContext context = new MovieDbContext(Configuration.GetConnectionString("DefaultConnection")))
            {
                return context.Movie.GetAll().Select(s => s.ConvertToMovieDto());
            }
        }
    }
}