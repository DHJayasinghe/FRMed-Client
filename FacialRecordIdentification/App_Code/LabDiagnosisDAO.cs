namespace FacialRecordIdentification
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class LabDiagnoses
    {

        private LabDiagnosesLabDiagnosis[] labDiagnosisField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("LabDiagnosis")]
        public LabDiagnosesLabDiagnosis[] LabDiagnosis
        {
            get
            {
                return labDiagnosisField;
            }
            set
            {
                labDiagnosisField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class LabDiagnosesLabDiagnosis
    {

        private int idField;

        private string labNameField;

        private decimal labValueField;

        private string labUnitsField;

        private System.DateTime labDateTimeField;

        /// <remarks/>
        public int ID
        {
            get
            {
                return idField;
            }
            set
            {
                idField = value;
            }
        }

        /// <remarks/>
        public string LabName
        {
            get
            {
                return labNameField;
            }
            set
            {
                labNameField = value;
            }
        }

        /// <remarks/>
        public decimal LabValue
        {
            get
            {
                return labValueField;
            }
            set
            {
                labValueField = value;
            }
        }

        /// <remarks/>
        public string LabUnits
        {
            get
            {
                return labUnitsField;
            }
            set
            {
                labUnitsField = value;
            }
        }

        /// <remarks/>
        public System.DateTime LabDateTime
        {
            get
            {
                return labDateTimeField;
            }
            set
            {
                labDateTimeField = value;
            }
        }
    }


}