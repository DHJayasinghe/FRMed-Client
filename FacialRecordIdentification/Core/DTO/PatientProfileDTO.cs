namespace FacialRecordIdentification.Core
{
    public class PatientProfileDTO
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string DateOfBirth { get; set; }
        public string PersonalTitle { get; set; }
        public string PHN { get; set; }
    }
}