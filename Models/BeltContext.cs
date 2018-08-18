using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Test.Models
{
    public class Belt : DbContext
    {
        public Belt(DbContextOptions<Belt> options) : base(options) { }
        public DbSet<Users> Users { get; set; }
        public DbSet<Activities> Activities { get; set; }
        public DbSet<Participants> Participants { get; set; }
    }
}