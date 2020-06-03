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

        public HomeController(IDataSourceService dataSourceService)
        {
            _dataSourceService = dataSourceService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Patients(string sortOrder)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            var patients = _dataSourceService.GetListData<ListPatient>("Patient");

            if(sortOrder == "name_desc")
            {
                patients = patients.OrderByDescending(x => x.name.First().family);
            }
            else
            {
                patients = patients.OrderBy(x => x.name.First().family);
            }

            return View(patients);
        }

        public IActionResult Observations()
        {
            return View();
        }

        public IActionResult Medications()
        {
            return View();
        }

        public IActionResult MedicationsStatements()
        {
            return View();
        }
    }
}
