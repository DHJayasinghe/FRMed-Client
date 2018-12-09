using FacialRecordIdentification.Models;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FacialRecordIdentification.Controllers
{
    [RoutePrefix("Patient")]
    public class HomeController : Controller
    {
        private FacialRecMgmtDBDataContext dc;
        Random rnd = new Random();
        private string _ProfileImgDirectory = "";
        private string _FRECApiURL; //face recognition module, Python FLASK web service url

        public HomeController()
        {
            dc = new FacialRecMgmtDBDataContext(ConfigurationManager.ConnectionStrings["FacialRecMgmtDBConnectionString"].ConnectionString);

            //change this according to your running Python FLASK url
            _FRECApiURL = "http://localhost:5001/frec/api";

            //set attachment save directory path for patient profile images
            _ProfileImgDirectory = AppDomain.CurrentDomain.BaseDirectory + "/App_Data/PatientProfileImgData";

            if (!Directory.Exists(_ProfileImgDirectory))
                Directory.CreateDirectory(_ProfileImgDirectory);
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

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
                string _FileExtension = Path.GetExtension(model.WebCam.FileName);
                string _NewImageName = string.Concat(Guid.NewGuid().ToString(), _FileExtension);

                model.WebCam.SaveAs(Path.Combine(_ProfileImgDirectory, _NewImageName)); //save uploaded new profile picture
                Task<float[]> taskFaceEncode = FaceEncodeAsync(model.WebCam.InputStream, _NewImageName); //Create new face encoding task
                
                float[] unknownFaceEncoding = await taskFaceEncode; //get unknown face encodings

                dc.PatientProfiles.InsertOnSubmit(new PatientProfile
                {
                    PatientID = model.PatientId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    ProfilePicture = _NewImageName,
                    PreCalFaceEncoding = SetEncodings(unknownFaceEncoding), //convert face encodings to comma separated string
                    CreatedDate = DateTime.Now
                });
                dc.SubmitChanges(); //save patient profile record

                return Json(new { code = 200, text = "success" });
            }
            catch (Exception ex)
            {
                return Json(new { code = 500, text = ex.Message, log = ex });
            }
        }

        /// <summary>
        /// Send uploaded patient image file stream to Python web service and return face encodings
        /// </summary>
        [NonAction]
        private async Task<float[]> FaceEncodeAsync(Stream filestream, string filename)
        {
            using (HttpClient client = new HttpClient())
            {
                using (var formData = new MultipartFormDataContent())
                {
                    formData.Add(new StreamContent(filestream), "webcam", filename);

                    Task<HttpResponseMessage> faceEncodeTask = client.PostAsync(_FRECApiURL + "/faceencode", formData);
                    string content = await faceEncodeTask.Result.Content.ReadAsStringAsync();

                    float[] faceEncodings = JsonConvert.DeserializeObject<float[]>(content);
                    return faceEncodings;
                }
            }
        }

        /// <summary>
        /// Send Detect Face request to Python web service and return result
        /// </summary>
        [NonAction]
        private async Task<string> DetectFaceAsync(Guid[] KnownFaceIdArr, float[][] KnownFaceEncodingsArr, float[] UnknownFaceEncodings)
        {
            var searchModel = new SearchPatientRecordModel(KnownFaceIdArr, KnownFaceEncodingsArr, UnknownFaceEncodings);
            using (var client = new HttpClient())
            {
                Task<HttpResponseMessage> faceDetectTask = client.PostAsync(_FRECApiURL + "/detect", new StringContent(JsonConvert.SerializeObject(searchModel), Encoding.UTF8, "application/json"));

                var content = await faceDetectTask.Result.Content.ReadAsStringAsync();
                return content;
            }
        }

        /// <summary>
        /// Search Patient Medical Record using Uploaded Patient Image
        /// </summary>
        [HttpPost, Route("Record/Search")]
        public async Task<JsonResult> SearchPatientRecord(HttpPostedFileBase webcam)
        {
            Task<float[]> taskFaceEncode = FaceEncodeAsync(webcam.InputStream, webcam.FileName); //Create new face encoding task
            var knownFaceEncodings = dc.PatientProfiles.ToList().OrderBy(d => d.PatientID).Select(d => new { d.PatientID, d.PreCalFaceEncoding }); //load known face encodings from database

            float[] unknownFaceEncodings = await taskFaceEncode; //get unknown face encodings
            Task<string> taskDetectFace = DetectFaceAsync(
                knownFaceEncodings.Select(d => d.PatientID).ToArray(),
                knownFaceEncodings.Select(d => GetEncodings(d.PreCalFaceEncoding)).ToArray(),
                unknownFaceEncodings);

            var result = await taskDetectFace;
            if (result == "BAD_REQUEST")
                return Json(new { code = 400, text = "Bad Request" });
            else if (result == "NA")
                return Json(new { code = 200, text = "Matching Record Not Found" });


            Guid patientId = Guid.Parse(result);
            var diagnoseHistory = PatientHistory(patientId);

            return Json(new { code = 200, text = "success", data = diagnoseHistory });
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
        /// return face encodings as comma seperated string
        /// </summary>
        [NonAction]
        private string SetEncodings(float[] faceEncodings)
        {
            return string.Join(",", faceEncodings);
        }

        /// <summary>
        /// return comma seperated face encodings as float[]
        /// </summary>
        /// <param name="faceEncodings"></param>
        /// <returns></returns>
        [NonAction]
        private float[] GetEncodings(string faceEncodings)
        {
            return Array.ConvertAll(faceEncodings.Split(','), float.Parse);
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