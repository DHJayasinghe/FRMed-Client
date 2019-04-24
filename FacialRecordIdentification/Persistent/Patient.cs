using Dapper.Contrib.Extensions;
using System;

namespace FacialRecordIdentification.Persistent
{
    [Table("patient")]
    public class Patient
    {
        [Key]
        public int PID { get; set; }
        public int LPID { get; set; }
        public int HID { get; set; }
        public int Old_PID { get; set; }
        public string Personal_Title { get; set; }
        public string Full_Name_Registered { get; set; }
        public string Personal_Used_Name { get; set; }
        public string NIC { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Personal_Civil_Status { get; set; }
        public string HIN { get; set; }
        //newly added field
        public string ReferenceNo { get; set; }
    }
}