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
        public IActionResult Patient(string id, DateTime? dataTime = null, string filtering = null)
        {
            var observation = GetObservations(id);



            var dbObservation = new List<ListObservation>();
            foreach (var unit in observation)
            {
                if(unit.resourceType == "Observation")
                {
                    var baseUnit =  _iwmdbContext.Observation.Where(x => x.Id == unit.id && x.ResourceType == unit.resourceType);
                    if(baseUnit.Count() > 1)
                    {
                        var maxUnit = baseUnit.Max(x => x.VersionId);
                        var HighestUnit = baseUnit.First(x => x.VersionId == maxUnit);

                        var newUnit = unit;

                        if (HighestUnit.LastUpdated != null)
                        {
                            newUnit.meta.lastUpdated = (DateTime)HighestUnit.LastUpdated;
                            newUnit.DataWasEdited = true;
                        }
                        if (HighestUnit.Text != null)
                        {
                            newUnit.code.text = HighestUnit.Text;
                            newUnit.TextWasEdited = true;
                        }
                        if (HighestUnit.Value != null)
                        {
                            newUnit.valueQuantity.value = (double)HighestUnit.Value;
                            newUnit.ValueWasEdited = true;
                        }

                        newUnit.OldListObservations = baseUnit.OrderByDescending(x => x.VersionId).Skip(1).Select(y =>
                        new ListObservation
                        {
                            code = new Code { text = y.Text },
                            meta = new Meta { lastUpdated = (DateTime)y.LastUpdated },
                            valueQuantity = new ValueQuantity { value = (double)y.Value }
                        }).ToList();

                        dbObservation.Add(newUnit);
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
                        var maxUnit = baseUnit.Max(x => x.VersionId);
                        var HighestUnit = baseUnit.First(x => x.VersionId == maxUnit);

                        var newUnit = unit;

                        if (HighestUnit.LastUpdated != null)
                        {
                            newUnit.meta.lastUpdated = (DateTime)HighestUnit.LastUpdated;
                            newUnit.DataWasEdited = true;
                        }
                        if (HighestUnit.Text != null)
                        {
                            newUnit.medicationCodeableConcept.text = HighestUnit.Text;
                            newUnit.TextWasEdited = true;

                        }
                        if (HighestUnit.Value != null)
                        {
                            newUnit.dosage.First().doseQuantity.value = (int)HighestUnit.Value;
                            newUnit.ValueWasEdited = true;

                        }
                        newUnit.OldListObservations = baseUnit.OrderByDescending(x => x.VersionId).Skip(1).Select(y =>
                        new ListObservation
                        {
                            medicationCodeableConcept = new MedicationCodeableConcept { text = y.Text },
                            meta = new Meta { lastUpdated = (DateTime)y.LastUpdated },
                            dosage = new List<Dosage> { new Dosage {doseQuantity = new DoseQuantity { value = (int)y.Value } }  }
                        }).ToList();

                        dbObservation.Add(newUnit);
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

            dbObservation = dbObservation.OrderBy(x => x.meta.lastUpdated).ToList();

            DateTime last = DateTime.Now;

            if (filtering == "Last Week")
            {
                last = dataTime.Value.AddDays(-7);
            }
            if (filtering == "Last Year")
            {
                last = dataTime.Value.AddDays(-365);
            }
            if (filtering == "Last Month")
            {
                last = dataTime.Value.AddDays(-31);
            }
            if (filtering == "All")
            {
                last = dataTime.Value.AddDays(-5000);
            }

            if (filtering != null)
            {
                WeightGraphs = new WeightGraph();
                var values = dbObservation
                    .Where(y => y.resourceType == "Observation")
                    .Where(x => x.code.coding.First().code == "3141-9")
                    .Where(x => x.meta.lastUpdated > last && x.meta.lastUpdated < dataTime)
                    .Select(t => (float)t.valueQuantity.value)
                    .ToList();

                var dateTimes = dbObservation
                    .Where(y => y.resourceType == "Observation")
                    .Where(x => x.code.coding.First().code == "3141-9")
                    .Where(x => x.meta.lastUpdated > last && x.meta.lastUpdated < dataTime)
                    .Select(t => t.meta.lastUpdated)
                    .ToList();

                WeightGraphs.DateTime = dateTimes;
                WeightGraphs.Weight = values;
                WeightGraphs.Max = filtering;
            }
            else
            {
                WeightGraphs = new WeightGraph();
                var values = dbObservation
                    .Where(y => y.resourceType == "Observation")
                    .Where(x => x.code.coding.First().code == "3141-9")
                    .Select(t => (float)t.valueQuantity.value)
                    .ToList();

                var dateTimes = dbObservation
                    .Where(y => y.resourceType == "Observation")
                    .Where(x => x.code.coding.First().code == "3141-9")
                    .Select(t => t.meta.lastUpdated)
                    .ToList();

                WeightGraphs.DateTime = dateTimes;
                WeightGraphs.Weight = values;
            }

            

            dbObservation = dbObservation.OrderByDescending(x => x.meta.lastUpdated).ToList();

            var viewModel = new PatientViewModel() { Units = dbObservation, PatientId = id };

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


            var baseUnit = _iwmdbContext.Observation.Where(x => x.Id == specyfic.id && x.ResourceType == specyfic.resourceType);
            if (baseUnit.Count() > 1)
            {
                var maxUnit = baseUnit.Max(x => x.VersionId);
                var HighestUnit = baseUnit.First(x => x.VersionId == maxUnit);

                var newUnit = specyfic;

                if (HighestUnit.LastUpdated != null)
                {
                    newUnit.meta.lastUpdated = (DateTime)HighestUnit.LastUpdated;
                    newUnit.DataWasEdited = true;
                }
                if (HighestUnit.Text != null)
                {
                    newUnit.code.text = HighestUnit.Text;
                    newUnit.TextWasEdited = true;
                }
                if (HighestUnit.Value != null)
                {
                    newUnit.valueQuantity.value = (double)HighestUnit.Value;
                    newUnit.ValueWasEdited = true;
                }

                newUnit.OldListObservations = baseUnit.OrderByDescending(x => x.VersionId).Skip(1).Select(y =>
                new ListObservation
                {
                    code = new Code { text = y.Text },
                    meta = new Meta { lastUpdated = (DateTime)y.LastUpdated },
                    valueQuantity = new ValueQuantity { value = (double)y.Value }
                }).ToList();

                specyfic = newUnit;
            }
            if (baseUnit.Count() == 1)
            {
                var baseUnitFirst = baseUnit.First();
                var newUnit = specyfic;

                if (baseUnitFirst.LastUpdated != null)
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
                specyfic = newUnit;
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

            var baseUnit = _iwmdbContext.Observation.Where(x => x.Id == specyfic.id && x.ResourceType == specyfic.resourceType);
            if (baseUnit.Count() > 1)
            {
                var maxUnit = baseUnit.Max(x => x.VersionId);
                var HighestUnit = baseUnit.First(x => x.VersionId == maxUnit);

                var newUnit = specyfic;

                if (HighestUnit.LastUpdated != null)
                {
                    newUnit.meta.lastUpdated = (DateTime)HighestUnit.LastUpdated;
                    newUnit.DataWasEdited = true;
                }
                if (HighestUnit.Text != null)
                {
                    newUnit.medicationCodeableConcept.text = HighestUnit.Text;
                    newUnit.TextWasEdited = true;

                }
                if (HighestUnit.Value != null)
                {
                    newUnit.dosage.First().doseQuantity.value = (int)HighestUnit.Value;
                    newUnit.ValueWasEdited = true;

                }
                newUnit.OldListObservations = baseUnit.OrderByDescending(x => x.VersionId).Skip(1).Select(y =>
                new ListObservation
                {
                    medicationCodeableConcept = new MedicationCodeableConcept { text = y.Text },
                    meta = new Meta { lastUpdated = (DateTime)y.LastUpdated },
                    dosage = new List<Dosage> { new Dosage { doseQuantity = new DoseQuantity { value = (int)y.Value } } }
                }).ToList();

                specyfic = newUnit;
            }
            if (baseUnit.Count() == 1)
            {
                var baseUnitFirst = baseUnit.First();
                var newUnit = specyfic;

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
                specyfic = newUnit;
            }

            return View(specyfic);
        }

        [HttpPost]
        public IActionResult EditObservation(UpdateModel updateModel)
        {
            UpdateEntity(updateModel, "Observation");
            return RedirectToAction("Patients");

        }

        [HttpPost]
        public IActionResult EditMedicationStatement(UpdateModel updateModel)
        {
            UpdateEntity(updateModel, "MedicationStatement");
            return RedirectToAction("Patients");
        }

        [HttpPost]
        public IActionResult PatientFiltering(PatientFilter patientFIlter)
        {
            return RedirectToAction("Patient", "Home", new { id = patientFIlter .Id, filtering = patientFIlter.Filtering, dataTime = patientFIlter.DatePick });
        }

        private void UpdateEntity(UpdateModel updateModel, string resourceType)
        {
            var baseUnit = _iwmdbContext.Observation.Where(x => x.Id == updateModel.Id && x.ResourceType == resourceType);
            var maxUnit = baseUnit.Max(x => x.VersionId );
            var unit = baseUnit.First(x => x.VersionId == maxUnit);

            int ver = maxUnit + 1;
            var newObservation = new Observation
            {
                Id = unit.Id,
                LastUpdated = updateModel.CarriedOut,
                LastChanged = DateTime.Now,
                ResourceType = resourceType,
                Text = updateModel.Text,
                Value = (decimal)updateModel.Value,
                VersionId = ver
            };
            _iwmdbContext.Observation.Add(newObservation);
            _iwmdbContext.SaveChanges();
            //"MedicationStatement"
        }

        public ActionResult GetValues()
        {

            return Json(new { weights = WeightGraphs.Weight, dataTime = WeightGraphs.DateTime, unit = WeightGraphs.Max });
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

        public static List<string> TempValid = new List<string> { "368307", "359417", "435927"};



    }
}
