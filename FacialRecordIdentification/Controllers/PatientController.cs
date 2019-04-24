using FacialRecordIdentification.Models;
using FacialRecordIdentification.Persistent;
using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;

namespace FacialRecordIdentification.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "get,post")]
    public class PatientController : ApiController
    {
        private Random rnd = new Random();
        private IPatientRepository patientRepo;
        private ISampleDataRepository sampleDataRepo;

        // GET api/<controller>/5
        public IHttpActionResult Get(string refno)
        {
            try
            {
                patientRepo = new PatientRepository();
                var patient = patientRepo.Get(refno);

                if (patient == default(Patient))
                    return NotFound();

                int randomRecordId = rnd.Next(1, 100); //select random record id from 1-100 -> for random sample data selection
                sampleDataRepo = new SampleDataRepository();

                var labhistory = sampleDataRepo.GetLabDiagnosisDS().LabDiagnosis.Where(d => d.ID == randomRecordId)?
                    .OrderByDescending(d => d.LabDateTime).Take(100) //take 100 records
                    .Select(d => new LabDiagnosisDTO(d.LabDateTime) { Name = d.LabName, Value = d.LabValue, Unit = d.LabUnits }).ToList(); //select random lab history belong to random record id
                var medicalhistory = sampleDataRepo.GetPrimaryDiagnosisDS().PrimaryDiagnosis.Where(d => d.ID == randomRecordId)?
                    .Select(d => new MedicalDiagnosisDTO() { Code = d.Code, Description = d.Description }).ToList(); //select random medical history belong to random record id

                var result = new PatientProfileDTO()
                {
                    Id = patient.ReferenceNo,
                    FullName = patient.Full_Name_Registered,
                    DateOfBirth = patient.DateOfBirth?.ToString("dd/MM/yyyy"),
                    Gender = patient.Gender,
                    PersonalTitle = patient.Personal_Title,
                    CivilStatus=patient.Personal_Civil_Status,
                    PHN = patient.HIN,
                    LabHistory = labhistory,
                    MedicalHistory = medicalhistory
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST api/<controller>
        public IHttpActionResult Post(CreatePatientViewModel model)
        {
            try
            {
                patientRepo = new PatientRepository();
                var patient = new Patient
                {
                    Full_Name_Registered = string.Format("{0} {1}", model.FirstName, model.LastName),
                    Gender = model.Gender,
                    Personal_Civil_Status = model.CivilStatus,
                    Personal_Title = model.Title,
                    DateOfBirth = DateTime.TryParse(model.DateOfBirth, out DateTime dob) ? dob : default(DateTime?),
                    NIC = model.NIC
                };
                var patientId = patientRepo.Insert(patient);
                patient = patientRepo.Get((int)patientId);

                if (patient == default(Patient))
                    return NotFound();

                return Ok(patient.ReferenceNo); //return generated Reference# to the FRMed API
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}