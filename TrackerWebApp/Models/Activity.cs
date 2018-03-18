﻿
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrackerWebApp.Models
{
    public class Activity
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public double Distance { get; set; }

        [Required]
        public double Pace { get; set; }

        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MMM d yy}")]
        public DateTime StartTime { get; set; }

        [Required]
        public string FirebaseLocation { get; set; }

        public int? ActivityTypeId { get; set; }
        public ActivityType ActivityType { get; set; }

        //Reference to many to many relationships via join tables
        public ICollection<Note> Notes { get; set; }

    }
}
