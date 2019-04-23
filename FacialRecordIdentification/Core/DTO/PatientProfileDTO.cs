using System;
using System.Collections.Generic;

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

        public IEnumerable<LabDiagnosisDTO> LabHistory { get; set; }
        public IEnumerable<MedicalDiagnosisDTO> MedicalHistory { get; set; }
    }

    public class LabDiagnosisDTO
    {
        private readonly DateTime _dateTime;

        public string Name { get; set; }
        public decimal Value { get; set; }
        public string Unit { get; set; }
        public string DateTime => _dateTime.ToString("yyyy-MM-dd hh:mm tt");

        public LabDiagnosisDTO() { }
        public LabDiagnosisDTO(DateTime dateTime) => _dateTime = dateTime;
    }

    public class MedicalDiagnosisDTO
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }
}