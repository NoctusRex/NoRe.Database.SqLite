using NoRe.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoRe.Database.SqLite
{
    public class SqLiteConfiguration : Configuration
    {

        /// <summary>
        /// The path to the database file
        /// </summary>
        public string DatabasePath { get; set; }

        /// <summary>
        /// The version of the database
        /// </summary>
        public string DatabaseVersion { get; set; }

        /// <summary>
        /// Password of the database fiel
        /// </summary>
        public string Pwd { get; set; }

        public SqLiteConfiguration() : base(System.IO.Path.Combine(Pathmanager.ConfigurationDirectory, "SqLiteConfiguration.xml")) { }

        public override void Read()
        {
            SqLiteConfiguration temp = Read<SqLiteConfiguration>();
            if (temp is null) throw new Exception("Could not load configuration file");

            DatabasePath = temp.DatabasePath;
            DatabaseVersion = temp.DatabaseVersion;
            Pwd = temp.Pwd;
        }

        /// <summary>
        /// Returns the sqlite connection string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string connectionString = "";

            if (!string.IsNullOrEmpty(DatabasePath)) connectionString += $"Data Source={DatabasePath};";
            if (!string.IsNullOrEmpty(DatabaseVersion)) connectionString += $"Version={DatabaseVersion};";
            if (!string.IsNullOrEmpty(Pwd)) connectionString += $"Password={Pwd};";

            return connectionString;
        }
    }
}
