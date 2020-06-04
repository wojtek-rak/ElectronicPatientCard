using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElectronicPatientCard.Models
{
    public class ListObservation : WasEdited
    {
        public string resourceType { get; set; }
        public string id { get; set; }
        public Meta meta { get; set; }
        public Code code { get; set; } 
        public Subject subject { get; set; }
        public ValueQuantity valueQuantity { get; set; }
        public Specimen specimen { get; set; }
        public IList<Identifier> identifier { get; set; }
        public string status { get; set; }
        public IList<Category> category { get; set; } 
        public Context context { get; set; }
        public DateTime? effectiveDateTime { get; set; }
        public DateTime? issued { get; set; }

        public Text text { get; set; }
        public IList<Extension> extension { get; set; }
        public MedicationCodeableConcept medicationCodeableConcept { get; set; }
        public string taken { get; set; }
        public IList<Dosage> dosage { get; set; }

        public List<ListObservation> OldListObservations = new List<ListObservation>();
    }

    public class WasEdited
    {
        public bool ValueWasEdited { get; set; } = false;
        public bool DataWasEdited { get; set; } = false;
        public bool TextWasEdited { get; set; } = false;
    }

    public class Dosage
    {
        public string text { get; set; }
        public Timing timing { get; set; }
        public DoseQuantity doseQuantity { get; set; }
    }

    public class Timing
    {
        public Repeat repeat { get; set; }
    }

    public class Repeat
    {
        public int frequency { get; set; }
        public int period { get; set; }
        public string periodUnit { get; set; }
    }

    public class DoseQuantity
    {
        public int value { get; set; }
        public string unit { get; set; }
        public string system { get; set; }
        public string code { get; set; }
    }

    public class MedicationCodeableConcept
    {
        public IList<Coding> coding { get; set; }
        public string text { get; set; }
    }

    public class Code
    {
        public IList<Coding> coding { get; set; }
        public string text { get; set; }
    }
    

    public class Subject
    {
        public string reference { get; set; }
        public string display { get; set; }
    }

    public class ValueQuantity
    {
        public double value { get; set; }
        public string unit { get; set; }
        public string system { get; set; }
        public string code { get; set; }
    }

    public class Specimen
    {
        public string reference { get; set; }
    }

    public class Context
    {
        public string reference { get; set; }
    }

    public class Category
    {
        public IList<Coding> coding { get; set; }
    }
}
