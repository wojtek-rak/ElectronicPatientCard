using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElectronicPatientCard.Models
{
    public class ListPatient
    {
        public string resourceType { get; set; }
        public string id { get; set; }
        public Meta meta { get; set; }
        public Text text { get; set; }
        public IList<Identifier> identifier { get; set; }
        public IList<Name> name { get; set; }
        public string gender { get; set; }
        public string birthDate { get; set; }
        public IList<Telecom> telecom { get; set; }
        public BirthDate _birthDate { get; set; }
        public IList<Address> address { get; set; }
        public IList<Contact> contact { get; set; }
    }
}
