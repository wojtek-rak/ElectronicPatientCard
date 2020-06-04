using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElectronicPatientCard.Models
{
    public class UpdateModel
    {
        public float Value { get; set; }
        public DateTime CarriedOut { get; set; }
        public string Text { get; set; }
        public string Id { get; set; }
    }
}
