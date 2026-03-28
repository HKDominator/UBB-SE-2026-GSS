using System;

namespace Events_GSS.Data.Models;

    public class Event
    {
	    public int EventId { get; set; }
        public string Name { get; set; } = null!;
        public double LocationLat { get; set; }
        public double LocationLng { get; set; }
        public DateTime StartDateTime {  get; set; }
        public DateTime EndDateTime { get; set; }
        public bool IsPublic { get; set; }
        public string? Description { get; set; }
        public int? MaximumPeople { get; set; }
        public string? EventBannerPath { get; set; }
        public Category? Category { get; set; }
        public User? CreatedBy { get; set; }
        public int? SlowModeSeconds {  get; set; }
       
        public int EnrolledCount { get; set; } = 0;
    }