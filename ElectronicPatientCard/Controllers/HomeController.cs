using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ElectronicPatientCard.Models;
using ElectronicPatientCard.Services;

namespace ElectronicPatientCard.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDataSourceService _dataSourceService;
        private readonly IwmdbContext _iwmdbContext;

        public static WeightGraph WeightGraphs { get; set; }

        public HomeController(IDataSourceService dataSourceService, IwmdbContext iwmdbContext)
        {
            _dataSourceService = dataSourceService;
            _iwmdbContext = iwmdbContext;
        }

        public IActionResult Index()
        {
            return View();
        }



        public IActionResult Patients(string sortOrder)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            var patientsObs = _dataSourceService.GetListData<ListObservation>("Observation", 0).ToList();


            var patientsObsEnu = (IEnumerable<ListObservation>)patientsObs;
            patientsObsEnu = patientsObsEnu.Where(x => x.subject != null);
            patientsObsEnu = patientsObsEnu.Where(x => x.subject.reference != null);
            patientsObsEnu = patientsObsEnu.Where(x => x.subject.reference.Split('/').Count() == 2);
            var ids = patientsObsEnu.Select(x => x.subject.reference.Split('/')[1]).ToList();
            ids.AddRange(TempValid);
            var uniqIds = ids.ToHashSet();

            var patients = new List<ListPatient>();

            foreach (var id in uniqIds)
            {
                patients.Add(_dataSourceService.GetData<ListPatient>($"Patient/{id}"));
                Console.Write(@$"""{id}"",");
            }

            patients = patients.Where(x => x.name != null).ToList();
            patients = patients.Where(x => x.name.Count >= 1).ToList();
            patients = patients.Where(x => x.name.First().given != null).ToList();

            if (sortOrder == "name_desc")
            {
                patients = patients.OrderByDescending(x => x.name.First().family).ToList();
            }
            else
            {
                patients = patients.OrderBy(x => x.name.First().family).ToList();
            }

            return View(patients);
        }

        [HttpGet("Patient/{id}")]
        public IActionResult Patient(string id)
        {
            var observation = GetObservations(id);

            WeightGraphs = new WeightGraph();
            var values = observation
                .Where(y => y.resourceType == "Observation")
                .Where(x => x.code.coding.First().code == "3141-9")
                .Select(t => (float)t.valueQuantity.value)
                .ToList();

            var dateTimes = observation
                .Where(y => y.resourceType == "Observation")
                .Where(x => x.code.coding.First().code == "3141-9")
                .Select(t => t.meta.lastUpdated)
                .ToList();

            WeightGraphs.DateTime = dateTimes;
            WeightGraphs.Weight = values;

            var dbObservation = new List<ListObservation>();
            foreach (var unit in observation)
            {
                if(unit.resourceType == "Observation")
                {
                    var baseUnit =  _iwmdbContext.Observation.Where(x => x.Id == unit.id && x.ResourceType == unit.resourceType);
                    if(baseUnit.Count() > 1)
                    {

                    }
                    if(baseUnit.Count() == 1)
                    {
                        var baseUnitFirst = baseUnit.First();
                        var newUnit = unit;

                        if(baseUnitFirst.LastUpdated != null)
                        {
                            newUnit.meta.lastUpdated = (DateTime)baseUnitFirst.LastUpdated;
                        }
                        if (baseUnitFirst.Text != null)
                        {
                            newUnit.code.text = baseUnitFirst.Text;
                        }
                        if (baseUnitFirst.Value != null)
                        {
                            newUnit.valueQuantity.value = (double)baseUnitFirst.Value;
                        }
                        dbObservation.Add(newUnit);
                    }
                    if(baseUnit.Count() == 0)
                    {
                        int ver = 1;
                        int.TryParse(unit.meta.versionId, out ver);
                        var newObservation = new Observation
                        {
                            Id = unit.id,
                            LastUpdated = unit.meta.lastUpdated,
                            LastChanged = unit.meta.lastUpdated,
                            ResourceType = unit.resourceType,
                            Text = unit.code.text,
                            Value = (decimal)unit.valueQuantity.value,
                            VersionId = ver
                        };
                        _iwmdbContext.Observation.Add(newObservation);
                        _iwmdbContext.SaveChanges();
                        dbObservation.Add(unit);
                    }
                }
                else if (unit.resourceType == "MedicationStatement")
                {
                    var baseUnit = _iwmdbContext.Observation.Where(x => x.Id == unit.id && x.ResourceType == unit.resourceType);
                    if (baseUnit.Count() > 1)
                    {

                    }
                    if (baseUnit.Count() == 1)
                    {
                        var baseUnitFirst = baseUnit.First();
                        var newUnit = unit;

                        if (baseUnitFirst.LastUpdated != null)
                        {
                            newUnit.meta.lastUpdated = (DateTime)baseUnitFirst.LastUpdated;
                        }
                        if (baseUnitFirst.Text != null)
                        {
                            newUnit.medicationCodeableConcept.text = baseUnitFirst.Text;
                        }
                        if (baseUnitFirst.Value != null)
                        {
                            newUnit.dosage.First().doseQuantity.value = (int)baseUnitFirst.Value;
                        }
                        dbObservation.Add(newUnit);
                    }
                    if (baseUnit.Count() == 0)
                    {
                        int ver = 1;
                        int.TryParse(unit.meta.versionId, out ver);
                        var newObservation = new Observation
                        {
                            Id = unit.id,
                            LastUpdated = unit.meta.lastUpdated,
                            LastChanged = unit.meta.lastUpdated,
                            ResourceType = unit.resourceType,
                            Text = unit.medicationCodeableConcept.text,
                            Value = unit.dosage.First().doseQuantity.value,
                            VersionId = ver
                        };
                        _iwmdbContext.Observation.Add(newObservation);
                        _iwmdbContext.SaveChanges();
                        dbObservation.Add(unit);
                    }
                }
            }









            var viewModel = new PatientViewModel() { Units = observation, PatientId = id };

            return View(viewModel);
        }

        [HttpGet("Observation/{id}/{patientId}")]
        public IActionResult Observation(string id, string patientId)
        {
            var observation = GetObservations(patientId);
            var specyfic = observation.FirstOrDefault(x => x.id == id);

            if(specyfic.category != null 
                && specyfic.category.Count >= 1
                && specyfic.category.First().coding != null
                && specyfic.category.First().coding.Count >= 1)
            { }
            else
            {
                specyfic.category = new List<Category>() { new Category() { coding = new List<Coding>() { new Coding { } } } };
            }

            if (specyfic.code != null
                && specyfic.code.coding != null
                && specyfic.code.coding.Count >= 1)
            { }
            else
            {
                specyfic.code = new Code() { coding = new List<Coding>() { new Coding()} };
            }



            return View(specyfic);
        }

        [HttpGet("MedicationStatement/{id}/{patientId}")]
        public IActionResult MedicationStatement(string id, string patientId)
        {
            var observation = GetObservations(patientId);
            var specyfic = observation.FirstOrDefault(x => x.id == id);

            if (specyfic.medicationCodeableConcept != null
                && specyfic.medicationCodeableConcept.coding != null
                && specyfic.medicationCodeableConcept.coding.Count >= 1)
            { }
            else
            {
                specyfic.medicationCodeableConcept = new MedicationCodeableConcept() {  coding = new List<Coding>() { new Coding { } } };
            }

            if (specyfic.dosage != null
                && specyfic.dosage.Count >= 1
                && specyfic.dosage.First().doseQuantity != null)
            { }
            else
            {
                specyfic.dosage = new List<Dosage>() { new Dosage() { doseQuantity = new DoseQuantity() } };
            }

            return View(specyfic);
        }

        public ActionResult GetValues()
        {

            return Json(new { weights = WeightGraphs.Weight, dataTime = WeightGraphs.DateTime });
        }

        private List<ListObservation> GetObservations(string patientId)
        {
            var observation = _dataSourceService.GetListData<ListObservation>($"Observation?subject={patientId}", 200).ToList();

            var medicationStatement = _dataSourceService.GetListData<ListObservation>($"MedicationStatement?subject={patientId}", 200);
            medicationStatement = medicationStatement.Where(x => x.medicationCodeableConcept != null);
            medicationStatement = medicationStatement.Where(x => x.medicationCodeableConcept.coding.Count >= 1);
            medicationStatement = medicationStatement.Where(x => x.medicationCodeableConcept.coding.First().display != null);

            observation.AddRange(medicationStatement);

            observation = observation.OrderByDescending(x => x.meta.lastUpdated).ToList();

            return observation;
        }

        public static List<string> TempValid = new List<string> { "368307", "359417", "435927", "1326480" };



    }
}
