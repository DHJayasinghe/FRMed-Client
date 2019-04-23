namespace FacialRecordIdentification
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class PrimaryDiagnoses
    {

        private PrimaryDiagnosesPrimaryDiagnosis[] primaryDiagnosisField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PrimaryDiagnosis")]
        public PrimaryDiagnosesPrimaryDiagnosis[] PrimaryDiagnosis
        {
            get
            {
                return primaryDiagnosisField;
            }
            set
            {
                primaryDiagnosisField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class PrimaryDiagnosesPrimaryDiagnosis
    {

        private int idField;

        private string codeField;

        private string descriptionField;

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
        public string Code
        {
            get
            {
                return codeField;
            }
            set
            {
                codeField = value;
            }
        }

        /// <remarks/>
        public string Description
        {
            get
            {
                return descriptionField;
            }
            set
            {
                descriptionField = value;
            }
        }
    }


}