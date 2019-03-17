using Dapper.Contrib.Extensions;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;

namespace FacialRecordIdentification.Persistent
{
    public class PatientRepository
    {
        private readonly IDbConnection db;

        public PatientRepository() => db = new MySqlConnection(ConfigurationManager.ConnectionStrings["hhimsDBConnectionString"].ConnectionString);

        public Patient Get(int id) => db.Get<Patient>(id);

        public List<Patient> GetAll() => db.GetAll<Patient>().ToList();

        public long Insert(Patient entity)
        {
            entity.HIN = System.DateTime.Now.ToString("yyMMddHHmmss"); //generate 12 char mock Personal Health Number (PHN) using current DateTime
            return db.Insert(entity);
        }

        public bool Update(Patient entity) => db.Update<Patient>(entity);

        public void Dispose() => db.Dispose();
    }
}