using FacialRecordIdentification.Models;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
        private string _FRECApiURL;

        public HomeController()
        {
            dc = new FacialRecMgmtDBDataContext(ConfigurationManager.ConnectionStrings["FacialRecMgmtDBConnectionString"].ConnectionString);
            _FRECApiURL = "http://localhost:5001/frec/api";
            _ProfileImgDirectory = AppDomain.CurrentDomain.BaseDirectory + "/App_Data/PatientProfileImgData"; //set attachment save directory path
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

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost, Route("MockData/Generate")]
        public JsonResult RandomMockData()
        {
            try
            {
                var _PatientIdArr = dc.PatientCorePopulateds.Select(d => d.PatientID).ToArray();
                int _RandIndex = rnd.Next(0, _PatientIdArr.Count());
                Guid _SelectedPatientId = _PatientIdArr[_RandIndex];
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

        [HttpPost, Route("Record/Save")]
        public async Task<JsonResult> SavePatientRecord(CreatePatientRecordModel model)
        {
            try
            {
                string _FileExtension = Path.GetExtension(model.WebCam.FileName);
                string _NewImageName = string.Concat(Guid.NewGuid().ToString(), _FileExtension);
                model.WebCam.SaveAs(Path.Combine(_ProfileImgDirectory, _NewImageName));

                //HttpContent stringContent = new StringContent(paramString);
                HttpContent fileStreamContent = new StreamContent(model.WebCam.InputStream);
                //HttpContent bytesContent = new ByteArrayContent(paramFileBytes);
                using (var client = new HttpClient())
                {
                    using (var formData = new MultipartFormDataContent())
                    {
                        //formData.Add(stringContent, "webcam", "param1");
                        formData.Add(fileStreamContent, "webcam", _NewImageName);
                        //formData.Add(bytesContent, "file2", "file2");
                        var response = await client.PostAsync(_FRECApiURL + "/faceencode", formData);
                        if (!response.IsSuccessStatusCode)
                            return null;

                        var contents = await response.Content.ReadAsStringAsync();
                        var faceEncodings = JsonConvert.DeserializeObject<float[]>(contents);

                        dc.PatientProfiles.InsertOnSubmit(new PatientProfile
                        {
                            PatientID = model.PatientId,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            ProfilePicture = _NewImageName,
                            PreCalFaceEncoding = SetEncodings(faceEncodings), //face encoding values to comma separated string
                            CreatedDate = DateTime.Now
                        });
                        dc.SubmitChanges();

                        return Json(new { code = 200, text = "success" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { code = 500, text = ex.Message, log = ex });
            }
        }

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

        [HttpPost, Route("Record/Search")]
        public async Task<JsonResult> SearchPatientRecord(HttpPostedFileBase webcam)
        {
            Task<float[]> taskFaceEncode = FaceEncodeAsync(webcam.InputStream, webcam.FileName);

            var knownFaceEncodings = dc.PatientProfiles.ToList().OrderBy(d => d.PatientID).Select(d => new { d.PatientID, d.PreCalFaceEncoding });
            float[] unknownFaceEncoding = await taskFaceEncode;
            var searchModel = new SearchPatientRecordModel(knownFaceEncodings.Select(d => d.PatientID).ToArray(), knownFaceEncodings.Select(d => GetEncodings(d.PreCalFaceEncoding)).ToArray(), unknownFaceEncoding);
            
            

            HttpContent stringContent = new StringContent(JsonConvert.SerializeObject(searchModel));
            HttpContent fileStreamContent = new StreamContent(webcam.InputStream);
            //HttpContent bytesContent = new ByteArrayContent(paramFileBytes);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                using (var formData = new MultipartFormDataContent())
                {
                    formData.Add(stringContent, "data");
                    formData.Add(fileStreamContent, "webcam", webcam.FileName);

                    var response = await client.PostAsync(_FRECApiURL + "/detect", new StringContent(JsonConvert.SerializeObject(searchModel), Encoding.UTF8, "application/json"));
                    if (!response.IsSuccessStatusCode)
                        return null;

                    var contents = await response.Content.ReadAsStringAsync();
                    //var faceEncodings = JsonConvert.DeserializeObject<float[]>(contents);

                    return Json(new { code = 200, text = "success" });
                }
            }
        }


        private string SetEncodings(float[] faceEncodings)
        {
            return string.Join(",", faceEncodings);
        }

        private float[] GetEncodings(string faceEncodings)
        {
            return Array.ConvertAll(faceEncodings.Split(','), float.Parse);
        }
    }
}