namespace HVIP.Models
{
    public class Banner
    {
        public int    Id                { get; set; }
        public string Title             { get; set; }
        public string Subtitle          { get; set; }
        public string BadgeText         { get; set; }
        public string Icon              { get; set; }
        public string BgGradient        { get; set; }
        public string PrimaryLink       { get; set; }
        public string PrimaryLinkText   { get; set; }
        public string SecondaryLink     { get; set; }
        public string SecondaryLinkText { get; set; }
        public int    SortOrder         { get; set; }
        public bool   IsActive          { get; set; }
    }
}
