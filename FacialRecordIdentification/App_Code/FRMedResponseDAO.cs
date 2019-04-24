using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FacialRecordIdentification
{
    public class FRMedResponseDAO
    {
        public int code { get; set; }
        public string text { get; set; }
        public FRMedFaceProfileDAO data { get; set; }
    }

    public class FRMedFaceProfileDAO {
        public string ReferenceNo { get; set; }
        public string FaceImage { get; set; }
    }
}