using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace FacialRecordIdentification.Models
{
    public class CreatePatientRecordModel
    {
        [Required]
        public Guid PatientId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        [Required]
        public HttpPostedFileBase WebCam { get; set; }
    }

    public class SearchPatientRecordModel
    {
        public Guid[] KnownFaceId { get; private set; }
        public float[][] KnownFaceEncoding { get; private set; }
        public float[] UnknownFaceEncoding { get; private set; }

        public SearchPatientRecordModel(Guid[] knownFaceId, float[][] knownFaceEncoding, float[] unknownFaceEncoding)
        {
            KnownFaceId = knownFaceId;
            KnownFaceEncoding = knownFaceEncoding;
            UnknownFaceEncoding = unknownFaceEncoding;
        }
    }
}