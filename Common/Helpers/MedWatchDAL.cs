using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Common.Entities;

namespace Common.Helpers
{
    public static class MedWatchDAL
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static readonly string ConnectionString = @"Server=127.0.0.1;Database=MedWatch;Trusted_Connection=True;";

        public static void InsertBulkHospitalEvents(IEnumerable<IHospitalEvent> events)
        {
            var sw = Stopwatch.StartNew();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();

                    using ( var bulkCopy = new SqlBulkCopy( ConnectionString ) )
                    {
                        // Create the data table
                        DataTable table = new DataTable();
                        table.Columns.Add( "Id", typeof( long ) );
                        table.Columns.Add( "HospitalId", typeof( int ) );
                        table.Columns.Add( "PatientId", typeof( int ) );
                        table.Columns.Add( "EventType", typeof( short ) );
                        table.Columns.Add( "EventTimestamp", typeof( DateTime ) );
                        table.Columns.Add( "DiseaseType", typeof( short ) );
                        table.Columns.Add( "DoctorId", typeof( int ) );
                        foreach ( var e in events )
                        {
                            table.Rows.Add( e.EventId,
                                            e.HospitalId,
                                            e.PatiendId,
                                            Convert.ToInt16( e.EventType ),
                                            e.EventTime,
                                            e.DiseaseType,
                                            e.DoctorId );
                        }

                        // Specify the destination table name in the database
                        bulkCopy.DestinationTableName = "HospitalEvent";

                        // Store in the database
                        bulkCopy.WriteToServer( table );
                    }

                    connection.Close();
                }
                catch ( Exception ex )
                {
                    logger.Error( "ERROR: {0}", ex.Message );
                }
            }

            sw.Stop();
            logger.Trace( "InsertBulkHospitalEvents: {0} events inserted in {1} ms", events.Count(), sw.ElapsedMilliseconds );
        }
        
        public static IEnumerable<IHospitalEvent> FindHospitalEventsAfter( int hospitalId, long afterEventId, int maxEventCount )
        {
            var events = new List<IHospitalEvent>();
            
            try
            {
                using ( var conn = new SqlConnection( ConnectionString ) )
                {
                    conn.Open();

                    using ( var cmd = new SqlCommand( "HospitalEventSelectCommand", conn ) )
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        var param = new SqlParameter();
                        param.ParameterName = "@HospitalId";
                        param.Value = hospitalId;
                        param.SqlDbType = System.Data.SqlDbType.Int;
                        cmd.Parameters.Add( param );

                        param = new SqlParameter();
                        param.ParameterName = "@AfterEventId";
                        param.Value = afterEventId;
                        param.SqlDbType = System.Data.SqlDbType.BigInt;
                        cmd.Parameters.Add( param );

                        param = new SqlParameter();
                        param.ParameterName = "@MaxEventCount";
                        param.Value = maxEventCount;
                        param.SqlDbType = System.Data.SqlDbType.Int;
                        cmd.Parameters.Add(param);

                        SqlDataReader dr = cmd.ExecuteReader();

                        while ( dr.Read() )
                        {
                            var e = new HospitalEvent
                            {
                                EventId = dr.GetInt64( 0 ),
                                HospitalId = hospitalId,
                                PatiendId = dr.GetInt32( 1 ),
                                EventType = (HospitalEventType) dr.GetInt16( 2 ),
                                EventTime = dr.GetDateTime( 3 ),
                                DiseaseType = dr.IsDBNull( 4 ) ? (DiseaseType?) null : (DiseaseType) dr.GetInt16( 4 ),
                                DoctorId = dr.IsDBNull( 5 ) ? (int?) null : dr.GetInt32( 5 )
                            };

                            events.Add( e );
                        }
                    }

                    conn.Close();
                }
            }
            catch ( Exception ex )
            {
                logger.Error( "ERROR: {0}", ex.Message );
            }
            
            return events;
        }

        public static IEnumerable<Hospital> FindHospitals()
        {
            var hospitals = new List<Hospital>();

            string sql = "SELECT Id, Name, AssignedDoctors FROM Hospital ORDER BY Name";
            
            try
            {
                using ( var conn = new SqlConnection( ConnectionString ) )
                {
                    conn.Open();

                    using ( var cmd = new SqlCommand( sql, conn ) )
                    {
                        cmd.CommandType = System.Data.CommandType.Text;

                        SqlDataReader dr = cmd.ExecuteReader();

                        while ( dr.Read() )
                        {
                            var h = new Hospital
                            {
                                Id = dr.GetInt32( 0 ),
                                Name = dr.GetString( 1 ),
                                AssignedDoctors = dr.GetInt32( 2 )
                            };

                            hospitals.Add( h );
                        }
                    }

                    conn.Close();
                }
            }
            catch ( Exception ex )
            {
                logger.Error( "ERROR: {0}", ex.Message );
            }
          
            return hospitals;
        }

        public static IEnumerable<Disease> FindDiseases()
        {
            var diseases = new List<Disease>();

            string sql = "SELECT Id, Name, Priority, RequiredTime, TimeUnit FROM Disease";
            
            try
            {
                using ( var conn = new SqlConnection( ConnectionString ) )
                {
                    conn.Open();

                    using ( var cmd = new SqlCommand( sql, conn ) )
                    {
                        cmd.CommandType = System.Data.CommandType.Text;

                        SqlDataReader dr = cmd.ExecuteReader();

                        while ( dr.Read() )
                        {
                            var h = new Disease
                            {
                                Id = (DiseaseType) dr.GetInt16( 0 ),
                                Name = dr.GetString( 1 ),
                                Priority = (DiseasePriority) dr.GetInt16( 2 ),
                                RequiredTime = dr.GetInt32( 3 ),
                                TimeUnit = (RequiredTimeUnit) dr.GetInt16( 4 )
                            };

                            diseases.Add( h );
                        }
                    }

                    conn.Close();
                }
               
            }
            catch (Exception ex)
            {
                logger.Error( "ERROR: {0}", ex.Message );
            }
          
            return diseases;
        }
    }
}
