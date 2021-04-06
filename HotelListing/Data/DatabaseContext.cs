using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelListing.Data
{
    public class DatabaseContext :DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Country> Countries { get; set; }
        public DbSet<Hotel> Hotels { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Country>().HasData(
                    new Country { Id = 1, Name = "Mexico", ShortName = "MX"},
                    new Country { Id = 2, Name = "Brazil", ShortName = "BR" },
                    new Country { Id = 3, Name = "United States", ShortName = "US" }
                );

            builder.Entity<Hotel>().HasData(
                    new Hotel { Id = 1, Name = "Hotel 1", Address = "Calle 1", Rating = 5.0, CountryId = 1 },
                    new Hotel { Id = 2, Name = "Hotel 2", Address = "Calle 2", Rating = 4.5, CountryId = 2 },
                    new Hotel { Id = 3, Name = "Hotel 3", Address = "Calle 3", Rating = 3.9, CountryId = 3 }
                );
        }

    }
}
