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
        public IActionResult Patients()
        {
            var patients = _dataSourceService.GetData<RequestBase>("Patient");
            return View();
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
