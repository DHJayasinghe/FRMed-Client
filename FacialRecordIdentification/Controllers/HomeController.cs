using FacialRecordIdentification.Models;
using FacialRecordIdentification.Persistent;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FacialRecordIdentification.Controllers
{
    [RoutePrefix("Patient")]
    public class HomeController : Controller
    {
        private readonly string _FRMedEndpointAddress = "http://localhost:5001/frmed/api"; //FRMed Web API endpoint address
        Random rnd = new Random();

        private readonly IPatientRepository patientRepo; //hhims db repository
        private readonly ISampleDataRepository sampleDataRepo; //sample data repository

        public HomeController()
        {
            patientRepo = new PatientRepository();
            sampleDataRepo = new SampleDataRepository();
        }

        public ActionResult Index() => View();

        public ActionResult About() => View();
        
        /// <summary>
        /// New patient record registration using scanned patient image
        /// </summary>
        [HttpPost, Route("Record/Save")]
        public async Task<JsonResult> SavePatientRecord(RegisterPatientViewModel model)
        {
            try
            {
                MemoryStream newFileStream = new MemoryStream();
                model.WebCam.InputStream.CopyTo(newFileStream); //copy the input stream into newFileStream variable
                byte[] imageByteArray = newFileStream.ToArray();

                var referenceNo = Guid.NewGuid().ToString();
                string imageFileName = string.Concat(referenceNo, Path.GetExtension(model.WebCam.FileName)); //generate new file name

                var result = await FRMedRegister(imageByteArray, imageFileName, referenceNo); //calling FRMed API register method (request => FaceImage, Reference#)
                if (result.code != 200) //if not a success response
                    return Json(result);

                //if success => register details in HHIMS database with generated Reference#
                var patient = new Patient
                {
                    Full_Name_Registered = string.Format("{0} {1}", model.FirstName, model.LastName),
                    Gender = model.Gender,
                    Personal_Civil_Status = model.CivilStatus,
                    Personal_Title = model.Title,
                    DateOfBirth = DateTime.TryParse(model.DateOfBirth, out DateTime dob) ? dob : default(DateTime?),
                    NIC = model.NIC,
                    ReferenceNo = referenceNo
                };
                var patientId = patientRepo.Insert(patient);
                patient = patientRepo.Get((int)patientId);

                if (patient == default(Patient))
                    return Json(new { code = HttpStatusCode.OK, text = "Patient Registration Failed" });

                return Json(new { code = HttpStatusCode.OK, text = "Patient Profile Saved" });
            }
            catch (Exception ex)
            {
                return Json(new { code = HttpStatusCode.InternalServerError, text = ex.Message, log = ex });
            }
        }

        [NonAction]
        private async Task<FRMedResponseDAO> FRMedRegister(byte[] frontalFace, string fileName, string referenceNo)
        {
            using (var client = new HttpClient())
            {
                using (var formData = new MultipartFormDataContent())
                {
                    var content = new ByteArrayContent(frontalFace);
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                    formData.Add(content, "FrontalFace", fileName);
                    formData.Add(new StringContent(referenceNo), "ReferenceNo");

                    Task<HttpResponseMessage> faceEncodeTask = client.PostAsync(_FRMedEndpointAddress + "/register", formData);
                    var response = await faceEncodeTask.Result.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<FRMedResponseDAO>(response);
                }
            }
        }

        /// <summary>
        /// Search Patient Medical Record using scanned patient image
        /// </summary>
        [HttpPost, Route("Record/Search")]
        public async Task<JsonResult> SearchPatientRecord(HttpPostedFileBase webcam)
        {
            try
            {
                string newFileName = webcam.FileName;
                MemoryStream newFileStream = new MemoryStream();
                webcam.InputStream.CopyTo(newFileStream); // CopyTo method used to copy the input stream into newFileStream variable
                byte[] imageByteArray = newFileStream.ToArray();

                var result = await FRMedIdentify(imageByteArray, newFileName); //calling FRMed API identify method (response => Reference#, FaceImage)
                if (result.code != 200) //if not a success response
                    return Json(result);

                //if success => get details from HHIMS database by Reference#
                var patient = patientRepo.Get(result.data.ReferenceNo);
                if (patient == default(Patient))
                    return Json(new { code = HttpStatusCode.NotFound, text = "No Matching Record Found" });

                int randomRecordId = rnd.Next(1, 100); //select random record id from 1-100 -> for random sample data selection

                var labhistory = sampleDataRepo.GetLabDiagnosisDS().LabDiagnosis.Where(d => d.ID == randomRecordId)?
                    .OrderByDescending(d => d.LabDateTime).Take(100) //take 100 records
                    .Select(d => new LabDiagnosisDTO(d.LabDateTime) { Name = d.LabName, Value = d.LabValue, Unit = d.LabUnits }).ToList(); //select random lab history belong to random record id
                var medicalhistory = sampleDataRepo.GetPrimaryDiagnosisDS().PrimaryDiagnosis.Where(d => d.ID == randomRecordId)?
                    .Select(d => new MedicalDiagnosisDTO() { Code = d.Code, Description = d.Description }).ToList(); //select random medical history belong to random record id

                var response = new PatientProfileDTO()
                {
                    Id = result.data.ReferenceNo,
                    FaceProfile = result.data.FaceImage,
                    FullName = patient.Full_Name_Registered,
                    DateOfBirth = patient.DateOfBirth?.ToString("dd/MM/yyyy"),
                    Gender = patient.Gender,
                    PersonalTitle = patient.Personal_Title,
                    CivilStatus = patient.Personal_Civil_Status,
                    PHN = patient.HIN,
                    LabHistory = labhistory,
                    MedicalHistory = medicalhistory
                };

                return Json(new { code = HttpStatusCode.OK, text = "Matching Record Found", data = response });
            }
            catch (Exception ex)
            {
                return Json(new { code = HttpStatusCode.InternalServerError, text = "Internal Server Error Occured", data = ex }); ;
            }
        }

        [NonAction]
        private async Task<FRMedResponseDAO> FRMedIdentify(byte[] frontalFace, string fileName)
        {
            using (HttpClient client = new HttpClient())
            {
                using (var formData = new MultipartFormDataContent())
                {
                    var contentPart = new ByteArrayContent(frontalFace);
                    contentPart.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                    formData.Add(contentPart, "FrontalFace", fileName);

                    Task<HttpResponseMessage> faceEncodeTask = client.PostAsync(_FRMedEndpointAddress + "/identify", formData);
                    var response = await faceEncodeTask.Result.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<FRMedResponseDAO>(response);
                }
            }
        }
    }
}