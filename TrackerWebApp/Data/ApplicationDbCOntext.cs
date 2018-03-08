﻿
using Microsoft.EntityFrameworkCore;
using TrackerWebApp.Models;

namespace TrackerWebApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<ActivityType> ActivityType { get; set; }
        public DbSet<Note> Note { get; set; }
        public DbSet<Activity> Activity { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }



    }
}