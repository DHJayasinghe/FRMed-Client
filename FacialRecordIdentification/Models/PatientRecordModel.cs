﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace FacialRecordIdentification.Models
{
    /// <summary>
    /// Model used to create new Patient record using dummy patient diagnosis profiles
    /// </summary>
    public class CreatePatientRecordModel
    {
        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public HttpPostedFileBase WebCam { get; set; }
    }

    /// <summary>
    /// Model used to send patient record search request to python web service
    /// </summary>
    public class SearchPatientRecordModel
    {
        public Guid[] KnownFaceIdArr { get; private set; }
        public float[][] KnownFaceEncodingsArr { get; private set; }
        public float[] UnknownFaceEncodings { get; private set; }

        public SearchPatientRecordModel(Guid[] knownFaceIdArr, float[][] knownFaceEncodingsArr, float[] unknownFaceEncodings)
        {
            KnownFaceIdArr = knownFaceIdArr;
            KnownFaceEncodingsArr = knownFaceEncodingsArr;
            UnknownFaceEncodings = unknownFaceEncodings;
        }
    }

    /// <summary>
    /// Patient Diagnosis History Details Model
    /// </summary>
    public class PatientDiagnosisHistoryModel
    {
        public PatientprofileDetailsModel Profile {  get; set; }
        public IEnumerable<PatientAdmissionDiagnoseModel> Admissions {  get; set; }
        
        public PatientDiagnosisHistoryModel(PatientprofileDetailsModel profile, IEnumerable<PatientAdmissionDiagnoseModel> admissions)
        {
            Profile = profile;
            Admissions = admissions;
        }
    }

    /// <summary>
    /// Patient Profile Details Model
    /// </summary>
    public class PatientprofileDetailsModel
    {
        public Guid PatientId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => string.Concat(FirstName, " ", LastName);
        public string DateOfBirth { get; set; }
        public string Race { get; set; }
        public string MaritalStatus { get; set; }
        public string Language { get; set; }
        public string Picture { get; set; }
        
        public PatientprofileDetailsModel(Guid patientId, string firstName, string lastName, DateTime dateOfBirth, string race, string maritalStatus, string language, string picture)
        {
            PatientId = patientId;
            FirstName = firstName;
            LastName = lastName;
            DateOfBirth = dateOfBirth.ToString("yyyy-MM-dd");
            Race = race;
            MaritalStatus = maritalStatus;
            Language = language;
            Picture = picture;
        }
    }

    /// <summary>
    /// Patient Admission Diagnose Model
    /// </summary>
    public class PatientAdmissionDiagnoseModel
    {
        public int AdmissionId { get; set; }
        public string DiagnosisCode { get; set; }
        public string DiagnosisDescription { get; set; }
        public List<PatientLabsResultModel> LabResults { get; set; }
        
        public PatientAdmissionDiagnoseModel(int admissionId, string diagnosisCode, string diagnosisDescription)
        {
            AdmissionId = admissionId;
            DiagnosisCode = diagnosisCode;
            DiagnosisDescription = diagnosisDescription;
        }

        public PatientAdmissionDiagnoseModel(int admissionId, string diagnosisCode, string diagnosisDescription, List<PatientLabsResultModel> labResults) : this(admissionId, diagnosisCode, diagnosisDescription)
        {
            LabResults = labResults;
        }
    }

    /// <summary>
    /// Patient Lab Result Model
    /// </summary>
    public class PatientLabsResultModel
    {
        public string LabName { get; set; }
        public double? LabValue { get; set; }
        public string LabUnits { get; set; }
        public string LabDateTime { get; set; }

        public PatientLabsResultModel(string labName, double? labValue, string labUnits, DateTime labDateTime)
        {
            LabName = labName;
            LabValue = labValue;
            LabUnits = labUnits;
            LabDateTime = labDateTime.ToString("yyyy-MM-dd hh:mm:ss tt");
        }
    }
}