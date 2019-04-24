namespace FacialRecordIdentification.Persistent
{
    public class SampleDataRepository : ISampleDataRepository
    {
        private readonly string xmlDataFolder = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Database");
        
        public LabDiagnoses GetLabDiagnosisDS()
        {
            var filepath = System.IO.Path.Combine(xmlDataFolder, "lab_diagnosis_sample_data.xml");
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(LabDiagnoses));

            LabDiagnoses list; // Declare an object variable of the type to be deserialized.

            using (var reader = new System.IO.FileStream(filepath, System.IO.FileMode.Open))
            {
                list = (LabDiagnoses)serializer.Deserialize(reader);  // Call the Deserialize method to restore the object's state.
                return list;
            }
        }
        
        public PrimaryDiagnoses GetPrimaryDiagnosisDS()
        {
            var filepath = System.IO.Path.Combine(xmlDataFolder, "primary_diagnosis_sample_data.xml");
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(PrimaryDiagnoses));

            PrimaryDiagnoses list; // Declare an object variable of the type to be deserialized.

            using (var reader = new System.IO.FileStream(filepath, System.IO.FileMode.Open))
            {
                list = (PrimaryDiagnoses)serializer.Deserialize(reader);  // Call the Deserialize method to restore the object's state.
                return list;
            }
        }
    }

    public interface ISampleDataRepository
    {
        /// <summary>
        /// Get sample lab diagnosis dataset
        /// </summary>
        LabDiagnoses GetLabDiagnosisDS();

        /// <summary>
        /// Get sample primary diagnosis dataset
        /// </summary>
        PrimaryDiagnoses GetPrimaryDiagnosisDS();
    }
}