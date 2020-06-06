using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElectronicPatientCard.Models
{
    public class PatientFilter
    {
        public DateTime DatePick { get; set; }
        public string Filtering { get; set; }
        public string Id { get; set; }
    }
}
