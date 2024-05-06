using PatientManager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace _2p_Practice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Patients_Controller : ControllerBase
    {
        private List<Patient> _patients;
        private readonly string _dataFilePath;
        private readonly IConfiguration _configuration;

        public Patients_Controller()
        {
            _patients = new List<Patient>
            {
                new Patient { Id = 1, Name = "Mauricio", LastName = "Terceros", CI = "123456", BloodType = "O+" },
                new Patient { Id = 2, Name = "John", LastName = "Smith", CI = "666666", BloodType = "A-" }
            };
        }

        // Método para crear un paciente (HTTP POST)
        [HttpPost]
        public ActionResult Post([FromBody] Patient patient)
        {
            if (patient == null)
            {
                return BadRequest("Datos del paciente inválidos"); // Devuelve un error si los datos del paciente son inválidos
            }
            patient.Id = GenerateNewId();
            AssignRandomBloodType(patient); // Asigna un grupo sanguíneo aleatorio al paciente
            _patients.Add(patient);
            return CreatedAtAction(nameof(Get), new { id = patient.Id }, patient);
        }

        // Método para actualizar un paciente por su ID (HTTP PUT)
        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody] Patient updatedPatient)
        {
            var patient = _patients.Find(p => p.Id == id);
            if (patient == null)
            {
                return NotFound("Paciente no encontrado"); // Devuelve un error si el paciente no existe
            }
            patient.Name = updatedPatient.Name;
            patient.LastName = updatedPatient.LastName;
            return NoContent();
        }

        // Método para eliminar un paciente por su ID (HTTP DELETE)
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var patient = _patients.Find(p => p.Id == id);
            if (patient == null)
            {
                return NotFound("Paciente no encontrado"); // Devuelve un error si el paciente no existe
            }
            _patients.Remove(patient);
            return NoContent();
        }

        // Método para obtener todos los pacientes (HTTP GET)
        [HttpGet]
        public ActionResult<IEnumerable<Patient>> Get()
        {
            return Ok(_patients);
        }

        // Método para obtener un paciente por su ID (HTTP GET)
        [HttpGet("{id}")]
        public ActionResult<Patient> Get(int id)
        {
            var patient = _patients.Find(p => p.Id == id);
            if (patient == null)
            {
                return NotFound("Paciente no encontrado"); // Devuelve un error si el paciente no existe
            }
            return Ok(patient);
        }

        // Método para generar un nuevo ID para pacientes
        private int GenerateNewId()
        {
            return _patients.Count + 1;
        }

        // Método para asignar un grupo sanguíneo aleatorio al paciente
        private void AssignRandomBloodType(Patient patient)
        {
            var bloodtypes = new string[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
            patient.BloodType = bloodtypes[new Random().Next(bloodtypes.Length)];
        }

        // Método para escribir un paciente en el archivo
        private void WritePatientToFile(Patient patient)
        {
            using (StreamWriter writer = new StreamWriter(_dataFilePath, true))
            {
                writer.WriteLine($"{patient.Id},{patient.Name},{patient.LastName},{patient.CI},{patient.BloodType}");
            }
        }

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


