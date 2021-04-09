using HotelListing.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelListing.Configurations.Entities
{
    public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
    {
        public void Configure(EntityTypeBuilder<Hotel> builder)
        {
            builder.HasData(
                new Hotel { Id = 1, Name = "Hotel 1", Address = "Calle 1", Rating = 5.0, CountryId = 1 },
                new Hotel { Id = 2, Name = "Hotel 2", Address = "Calle 2", Rating = 4.5, CountryId = 2 },
                new Hotel { Id = 3, Name = "Hotel 3", Address = "Calle 3", Rating = 3.9, CountryId = 3 }
            );
        }
    }
}
