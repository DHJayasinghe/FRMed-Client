using FacialRecordIdentification.Core;
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

        // GET api/<controller>/5
        public IHttpActionResult Get(string refno)
        {
            //select random record id from 1-100 -> for random sample data selection
            int randomId = rnd.Next(0, 100); //sample lab and primary diagnosis records related to this Id retrieved 

            try
            {
                patientRepo = new PatientRepository();
                var patient = patientRepo.Get(refno);

                if (patient == default(Patient))
                    return NotFound();

                var labhistory = GetLabDiagnosisDS().LabDiagnosis.Where(d => d.ID == randomId)?
                    .OrderByDescending(d=>d.LabDateTime)
                    .Take(100) //take 100 records
                    .Select(d => new LabDiagnosisDTO(d.LabDateTime) { Name = d.LabName, Value = d.LabValue, Unit = d.LabUnits })
                    .ToList(); //select random lab history
                var medicalhistory = GetPrimaryDiagnosisDS().PrimaryDiagnosis.Where(d => d.ID == randomId)?
                    .Select(d => new MedicalDiagnosisDTO() { Code = d.Code, Description = d.Description }).ToList(); //select random medical history

                var result = new PatientProfileDTO()
                {
                    Id = patient.ReferenceNo,
                    FullName = patient.Full_Name_Registered,
                    DateOfBirth = patient.DateOfBirth?.ToString("dd/MM/yyyy"),
                    Gender = patient.Gender,
                    PersonalTitle = patient.Personal_Title,
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

        /// <summary>
        /// Get sample lab diagnosis dataset
        /// </summary>
        private LabDiagnoses GetLabDiagnosisDS()
        {
            var filepath = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Database/lab_diagnosis_sample_data.xml");
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(LabDiagnoses));

            LabDiagnoses list; // Declare an object variable of the type to be deserialized.

            using (var reader = new System.IO.FileStream(filepath, System.IO.FileMode.Open))
            {
                list = (LabDiagnoses)serializer.Deserialize(reader);  // Call the Deserialize method to restore the object's state.
                return list;
            }
        }

        /// <summary>
        /// Get sample primary diagnosis dataset
        /// </summary>
        private PrimaryDiagnoses GetPrimaryDiagnosisDS()
        {
            var filepath = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Database/primary_diagnosis_sample_data.xml");
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(PrimaryDiagnoses));

            PrimaryDiagnoses list; // Declare an object variable of the type to be deserialized.

            using (var reader = new System.IO.FileStream(filepath, System.IO.FileMode.Open))
            {
                list = (PrimaryDiagnoses)serializer.Deserialize(reader);  // Call the Deserialize method to restore the object's state.
                return list;
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

                return Ok(patient.ReferenceNo);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}