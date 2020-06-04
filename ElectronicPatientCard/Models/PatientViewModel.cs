using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElectronicPatientCard.Models
{
    public class PatientViewModel
    {
        public string PatientId { get; set; }
        public List<ListObservation> Units { get; set; }
        public WeightGraph WeightGraphs { get; set; }
    }

    public class WeightGraph
    {
        public List<float> Weight { get; set; } = new List<float>();
        public List<DateTime> DateTime { get; set; } = new List<DateTime>();
        public string Max { get; set; }
    }
}
