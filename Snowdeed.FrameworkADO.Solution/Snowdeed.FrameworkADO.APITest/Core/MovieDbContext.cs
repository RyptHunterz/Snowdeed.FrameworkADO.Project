using System;

namespace Snowdeed.FrameworkADO.APITest.Core
{
    using Snowdeed.FrameworkADO.APITest.DbModels;
    using Snowdeed.FrameworkADO.Core.Core;

    public class MovieDbContext : DbContext
    {
        private DbSet<Movie> _movie;
        private DbSet<MovieGenre> _movieGenre;

        public MovieDbContext(string connectionString) : base(connectionString) { }

        // Old writing
        //public DbSet<Movie> Movie { get { return _movie ?? (_movie = new DbSet<Movie>(_connection, _database)); } }
        //public DbSet<MovieGenre> MovieGenre { get { return _movieGenre ?? (_movieGenre = new DbSet<MovieGenre>(_connection, _database)); } }

        // New writing
        public DbSet<Movie> Movie => _movie ??= new DbSet<Movie>(_connection, _database);
        public DbSet<MovieGenre> MovieGenre => _movieGenre ??= new DbSet<MovieGenre>(_connection, _database);
    }
}
