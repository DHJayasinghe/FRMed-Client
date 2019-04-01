using Dapper.Contrib.Extensions;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;

namespace FacialRecordIdentification.Persistent
{
    public class PatientRepository : IPatientRepository
    {
        private readonly IDbConnection db;

        public PatientRepository() => db = new MySqlConnection(ConfigurationManager.ConnectionStrings["hhimsDBConnectionString"].ConnectionString);

        public Patient Get(int id) => db.Get<Patient>(id);

        public Patient Get(string refno) => db.GetAll<Patient>().Where(d => d.ReferenceNo == refno).FirstOrDefault();

        public List<Patient> GetAll() => db.GetAll<Patient>().ToList();

        public long Insert(Patient entity)
        {
            entity.HIN = System.DateTime.Now.ToString("yyMMddHHmmss"); //generate 12 char mock Personal Health Number (PHN) using current DateTime
            return db.Insert(entity);
        }

        public bool Update(Patient entity) => db.Update<Patient>(entity);

        public void Dispose() => db.Dispose();
    }

    public interface IPatientRepository
    {
        /// <summary>
        /// Get Patient by Id
        /// </summary>
        Patient Get(int id);

        /// <summary>
        /// Get Patient by Reference No
        /// </summary>
        Patient Get(string refno);

        /// <summary>
        /// Create new Patient record
        /// </summary>
        long Insert(Patient entity);

        /// <summary>
        /// Update Patient record
        /// </summary>
        bool Update(Patient entity);

        void Dispose();
    }
}