using System;
using System.Collections.Generic;

namespace ElectronicPatientCard
{
    public partial class Observation
    {
        public string Id { get; set; }
        public string ResourceType { get; set; }
        public int VersionId { get; set; }
        public DateTime LastChanged { get; set; }
        public decimal? Value { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string Text { get; set; }
        public int PrimaryId { get; set; }
    }
}
