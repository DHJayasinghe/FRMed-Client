using FacialRecordIdentification.Core;
using FacialRecordIdentification.Persistent;
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Cors;

namespace FacialRecordIdentification.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "get,post")]
    public class PatientController : ApiController
    {
        private IPatientRepository patientRepo;

        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public IHttpActionResult Get(string refno)
        {
            try
            {
                patientRepo = new PatientRepository();
                var patient = patientRepo.Get(refno);

                if (patient == default(Patient))
                    return NotFound();

                var result = new PatientProfileDTO()
                {
                    Id = patient.ReferenceNo,
                    FullName = patient.Full_Name_Registered,
                    DateOfBirth = patient.DateOfBirth?.ToString("dd/MM/yyyy"),
                    Gender = patient.Gender,
                    PersonalTitle = patient.Personal_Title,
                    PHN = patient.HIN
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }
    }
}