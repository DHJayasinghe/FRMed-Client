using FacialRecordIdentification.Models;
using FacialRecordIdentification.Persistent;
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
        private readonly FacialRecMgmtDBDataContext dc;
        private readonly string _ProfileImgDirectory = "";
        private readonly string _FRECApiURL = "http://localhost:5001/frec/api"; //face recognition module, Python FLASK web service url
        private readonly PatientRepository patientRepo;
        Random rnd = new Random();

        public HomeController()
        {
            dc = new FacialRecMgmtDBDataContext(ConfigurationManager.ConnectionStrings["FacialRecMgmtDBConnectionString"].ConnectionString);

            //set attachment save directory path for patient profile images
            _ProfileImgDirectory = AppDomain.CurrentDomain.BaseDirectory + "/App_Data/PatientProfileImgData";

            if (!Directory.Exists(_ProfileImgDirectory))
                Directory.CreateDirectory(_ProfileImgDirectory);

            patientRepo = new PatientRepository();
        }

        public ActionResult Index() => View();

        public ActionResult About() => View();

        /// <summary>
        /// Return random patient diagnosis record for Patient profile registration process (Mock data)
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("MockData/Generate")]
        public JsonResult RandomMockData()
        {
            try
            {
                //return dummy patient diagnosis profile ids which is not used yet, for registration purposes
                var _PatientIdArr = dc.PatientCorePopulateds.Where(d => !dc.PatientProfiles.Any(x => x.PatientID == d.PatientID)).Select(d => d.PatientID).ToArray();
                int _RandIndex = rnd.Next(0, _PatientIdArr.Count()); //select random patient diagnosis profile
                Guid _SelectedPatientId = _PatientIdArr[_RandIndex];

                //return patient profile data belong to selected profile id
                var record = dc.PatientCorePopulateds.Where(d => d.PatientID == _SelectedPatientId).ToList().Select(d => new
                {
                    PatientId = d.PatientID,
                    DOB = d.PatientDateOfBirth.ToString("yyyy-MM-dd"),
                    Gender = d.PatientGender,
                    Language = d.PatientLanguage,
                    MaritalStatus = d.PatientMaritalStatus,
                    Race = d.PatientRace,
                    PopPercBP = d.PatientPopulationPercentageBelowPoverty
                }).First();

                return Json(record);
            }
            catch (Exception ex)
            {
                return Json(new { code = 500, text = ex.Message, log = ex });
            }
        }

        /// <summary>
        /// New patient record registration using patient diagnosis mock data
        /// </summary>
        [HttpPost, Route("Record/Save")]
        public async Task<JsonResult> SavePatientRecord(CreatePatientRecordModel model)
        {
            try
            {
                MemoryStream newFileStream = new MemoryStream();
                model.WebCam.InputStream.CopyTo(newFileStream); //copy the input stream into newFileStream variable
                byte[] imageByteArray = newFileStream.ToArray();

                string _profileImageName = string.Concat(Guid.NewGuid().ToString(), Path.GetExtension(model.WebCam.FileName));
                model.WebCam.SaveAs(Path.Combine(_ProfileImgDirectory, _profileImageName)); //save uploaded new profile picture

                //var patientId = patientRepo //save new patient record
                //    .Insert(new Patient
                //    {
                //        LPID = 1,
                //        HID = 1,
                //        Personal_Title = model.Title,
                //        Full_Name_Registered = model.FirstName + " " + model.LastName,
                //        Personal_Used_Name = "",
                //        NIC = model.NIC,
                //        DateOfBirth = !string.IsNullOrEmpty(model.DateOfBirth) ? Convert.ToDateTime(model.DateOfBirth) : default(DateTime?),
                //        Gender = model.Gender,
                //        Personal_Civil_Status = model.CivilStatus,
                //        ProfileImage = _profileImageName
                //    });
                //var patient = patientRepo.Get(Convert.ToInt32(patientId));

                Task<string> taskFRecRegister = FRecRegister(Guid.NewGuid().ToString(), Convert.ToChar(model.Gender), imageByteArray, _profileImageName);
                //Task<string> taskFRecRegister = FRecRegister(patient.HIN, Convert.ToChar(patient.Gender), imageByteArray, _profileImageName);
                var result = await taskFRecRegister;

                return Json(new { code = HttpStatusCode.OK, text = "Patient Profile Saved" });
            }
            catch (Exception ex)
            {
                return Json(new { code = HttpStatusCode.InternalServerError, text = ex.Message, log = ex });
            }
        }

        [NonAction]
        private async Task<string> FRecRegister(string referenceNo, char gender, byte[] frontalFace, string fileName)
        {
            using (var client = new HttpClient())
            {
                using (var formData = new MultipartFormDataContent())
                {
                    var content = new ByteArrayContent(frontalFace);
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                    formData.Add(content, "FrontalFace", fileName);
                    formData.Add(new StringContent(referenceNo), "ReferencenNo");
                    formData.Add(new StringContent(gender.ToString()), "Gender");

                    return await client.PostAsync(_FRECApiURL + "/register", formData).Result.Content.ReadAsStringAsync();
                }
            }
        }

        [NonAction]
        private async Task<string> FRecIdentify(byte[] frontalFace, string fileName)
        {
            using (HttpClient client = new HttpClient())
            {
                using (var formData = new MultipartFormDataContent())
                {
                    var contentPart = new ByteArrayContent(frontalFace);
                    contentPart.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                    formData.Add(contentPart, "FrontalFace", fileName);

                    Task<HttpResponseMessage> faceEncodeTask = client.PostAsync(_FRECApiURL + "/identify", formData);
                    return await faceEncodeTask.Result.Content.ReadAsStringAsync();
                }
            }
        }

        /// <summary>
        /// Search Patient Medical Record using Uploaded Patient Image
        /// </summary>
        [HttpPost, Route("Record/Search")]
        public async Task<JsonResult> SearchPatientRecord(HttpPostedFileBase webcam, bool getMatchingResult = true)
        {
            try
            {
                string newFileName = webcam.FileName;
                MemoryStream newFileStream = new MemoryStream();
                webcam.InputStream.CopyTo(newFileStream); // CopyTo method used to copy the input stream into newFileStream variable
                byte[] imageByteArray = newFileStream.ToArray();

                var result = await FRecIdentify(imageByteArray, newFileName);

                if (result == "BAD_REQUEST")
                    return Json(new { code = HttpStatusCode.BadRequest, text = "Bad Request" });
                else if (result == "NA")
                    return Json(new { code = HttpStatusCode.NotFound, text = "No Matching Record Found" });

                Guid patientId = Guid.Parse(result);
                var diagnoseHistory = getMatchingResult ? PatientHistory(patientId) : null;

                return Json(new { code = HttpStatusCode.OK, text = "Matching Record Found", data = diagnoseHistory });
            }
            catch (Exception ex)
            {
                return Json(new { code = HttpStatusCode.InternalServerError, text = "Internal Server Error Occured", data = ex }); ;
            }
        }

        /// <summary>
        /// Retrieve Matiching Patient Profile Picture
        /// </summary>
        [HttpGet, Route("Profile/Image")]
        public ActionResult LoadProfilePic(string imageName)
        {
            string picturePath = Path.Combine(_ProfileImgDirectory, imageName);
            if (!System.IO.File.Exists(picturePath))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            return File(picturePath, "image/jpg");
        }

        /// <summary>
        /// Return Patient Profile Medical Diagnose History by Patient ID
        /// </summary>
        [NonAction]
        public PatientDiagnosisHistoryModel PatientHistory(Guid patientId)
        {
            try
            {
                var admissions = dc.AdmissionsDiagnosesCorePopulates
                    .Where(d => d.PatientID == patientId).AsEnumerable();
                //.Select(d => new PatientAdmissionDiagnoseModel(d.AdmissionID, d.PrimaryDiagnosisCode, d.PrimaryDiagnosisDescription));
                var labResults = dc.LabsCorePopulateds.Where(d => d.PatientID == patientId).AsEnumerable();

                var diagnoseHistory = dc.PatientProfiles.Where(d => d.PatientID == patientId).ToList()
                    .Join(dc.PatientCorePopulateds.Where(d => d.PatientID == patientId).ToList(), pp => pp.PatientID, pcp => pcp.PatientID, (pp, pcp) => new PatientDiagnosisHistoryModel(
                        new PatientprofileDetailsModel(pp.PatientID, pp.FirstName, pp.LastName, pcp.PatientDateOfBirth, pcp.PatientRace, pcp.PatientMaritalStatus, pcp.PatientLanguage, "/Patient/Profile/Image?imageName=" + pp.ProfilePicture),
                        admissions.Select(ad => new PatientAdmissionDiagnoseModel(ad.AdmissionID, ad.PrimaryDiagnosisCode, ad.PrimaryDiagnosisDescription,
                        labResults.Where(lr => lr.AdmissionID == ad.AdmissionID).Take(100).Select(lr => new PatientLabsResultModel(lr.LabName,
                            lr.LabValue,
                            lr.LabUnits,
                            lr.LabDateTime
                        )).ToList())).ToList().OrderBy(ad => ad.AdmissionId).ToList())).FirstOrDefault();

                return diagnoseHistory;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}