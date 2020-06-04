using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElectronicPatientCard.Models
{

    public class Meta
    {
        public DateTime lastUpdated { get; set; }
        public string versionId { get; set; }
    }

    public class Link
    {
        public string relation { get; set; }
        public string url { get; set; }
    }

    public class Text
    {
        public string status { get; set; }
        public string div { get; set; }
    }

    public class Assigner
    {
        public string display { get; set; }
    }

    public class Identifier
    {
        public string system { get; set; }
        public string value { get; set; }
        public Assigner assigner { get; set; }
    }

    public class Name
    {
        public string use { get; set; }
        public string text { get; set; }
        public string family { get; set; }
        public IList<string> given { get; set; }
    }

    public class Telecom
    {
        public string system { get; set; }
        public string use { get; set; }
        public string value { get; set; }
    }

    public class Extension
    {
        public string url { get; set; }
        public DateTime valueDateTime { get; set; }
    }

    public class BirthDate
    {
        public IList<Extension> extension { get; set; }
    }

    public class Address
    {
        public string use { get; set; }
    }

    public class Coding
    {
        public string system { get; set; }
        public string code { get; set; }
        public string display { get; set; }
    }

    public class Relationship
    {
        public IList<Coding> coding { get; set; }
    }

    public class Contact
    {
        public IList<Relationship> relationship { get; set; }
        public IList<Telecom> telecom { get; set; }
    }

 

    public class Search
    {
        public string mode { get; set; }
    }

    public class Entry
    {
        public string fullUrl { get; set; }
        public object resource { get; set; }
        public Search search { get; set; }
    }

    public class RequestBase
    {
        public string resourceType { get; set; }
        public string id { get; set; }
        public Meta meta { get; set; }
        public string type { get; set; }
        public IList<Link> link { get; set; }
        public IList<Entry> entry { get; set; }
    }
}





