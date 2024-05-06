using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using PatientManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace _Practice2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Patients_Controller : ControllerBase
    {
        private readonly string _dataFilePath;
        private readonly IConfiguration _configuration;
        private List<Patient> _patients;

        public Patients_Controller(IConfiguration configuration)
        {
            _configuration = configuration;
            _dataFilePath = _configuration["AppSettings:DataFilePath"];
            _patients = LoadPatientsFromFile();  // Carga los pacientes al iniciar
        }

        // Método para crear un paciente (HTTP POST)
        [HttpPost]
        public IActionResult Create(Patient patient)
        {
            var bloodGroups = new string[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
            patient.BloodType = bloodGroups[new Random().Next(bloodGroups.Length)];  // Assign a random blood group
            _patients.Add(patient);
            SavePatientsToFile(_patients);  // Guarda los cambios en el archivo
            return CreatedAtAction(nameof(GetByCI), new { ci = patient.CI }, patient);
        }

        // Método para actualizar un paciente por su CI (HTTP PUT)
        [HttpPut("{ci}")]
        public IActionResult Update(string ci, Patient updatedPatient)
        {
            var patient = _patients.FirstOrDefault(p => p.CI == ci);
            if (patient == null)
            {
                return NotFound("Patient not found");
            }
            patient.Name = updatedPatient.Name;
            patient.LastName = updatedPatient.LastName;
            SavePatientsToFile(_patients);  // Guarda los cambios en el archivo
            return Ok(patient);
        }

        // Método para eliminar un paciente por su CI (HTTP DELETE)
        [HttpDelete("{ci}")]
        public IActionResult Delete(string ci)
        {
            var patient = _patients.FirstOrDefault(p => p.CI == ci);
            if (patient == null)
            {
                return NotFound("Patient not found");
            }
            _patients.Remove(patient);
            SavePatientsToFile(_patients);  // Guarda los cambios en el archivo
            return NoContent();
        }

        // Método para obtener todos los pacientes (HTTP GET)
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_patients);
        }

        // Método para obtener un paciente por su CI (HTTP GET)
        [HttpGet("{ci}")]
        public IActionResult GetByCI(string ci)
        {
            var patient = _patients.FirstOrDefault(p => p.CI == ci);
            if (patient == null)
            {
                return NotFound("Patient not found");
            }
            return Ok(patient);
        }

        // Método para cargar pacientes desde el archivo
        private List<Patient> LoadPatientsFromFile()
        {
            var patients = new List<Patient>();
            if (System.IO.File.Exists(_dataFilePath))
            {
                using (StreamReader reader = new StreamReader(_dataFilePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] parts = line.Split(',');
                        if (parts.Length == 5)
                        {
                            patients.Add(new Patient
                            {
                                Id = int.Parse(parts[0]),
                                Name = parts[1],
                                LastName = parts[2],
                                CI = parts[3],
                                BloodType = parts[4]
                            });
                        }
                    }
                }
            }
            return patients;
        }

        // Método para guardar pacientes en el archivo
        private void SavePatientsToFile(List<Patient> patients)
        {
            using (StreamWriter writer = new StreamWriter(_dataFilePath))
            {
                foreach (var patient in patients)
                {
                    writer.WriteLine($"{patient.Id},{patient.Name},{patient.LastName},{patient.CI},{patient.BloodType}");
                }
            }
        }

        // Método para obtener el documento JSON de Swagger
        [HttpGet("swagger")]
        public IActionResult GetSwaggerJson()
        {
            var environment = _configuration["AppSettings:Environment"];
            var appName = _configuration[$"AppInfo:{environment}:Name"];
            var appVersion = "v1";
            var swaggerUrl = $"{Request.Scheme}://{Request.Host.Value}/swagger/v1/swagger.json";

            var swaggerDoc = new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Title = appName,
                    Version = appVersion,
                    Description = "Patients Manager"
                },
                Servers = new List<OpenApiServer>
                {
                    new OpenApiServer { Url = swaggerUrl }
                }
            };

            return Ok(swaggerDoc);
        }
    }
}
