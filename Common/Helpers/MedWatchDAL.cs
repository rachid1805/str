using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Common.Entities;
using System.Data;
using System.Diagnostics;

namespace Common.Helpers
{
    public static class MedWatchDAL
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private const string ConnectionString = "Data Source=127.0.0.1;Initial Catalog=MedWatch;Integrated Security=True";

        public static void InsertBulkHospitalEvents(IEnumerable<IHospitalEvent> events)
        {
            logger.Trace( "InsertBulkHospitalEvents called" );
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
                        table.Columns.Add( "Id", typeof( int ) );
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
                    logger.Error( "ERROR: {0}", ex );
                }
            }

            sw.Stop();
            logger.Trace( "InsertBulkHospitalEvents completed in {0} ms", sw.ElapsedMilliseconds );
        }

        // TODO remove this
        public static void InsertHospitalEvents( IEnumerable<IHospitalEvent> events )
        {
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection( ConnectionString );
                conn.Open();

                // TODO : use SqlBulkCopy for batch insert
                foreach ( var e in events )
                {
                    using ( var cmd = new SqlCommand( "HospitalEventInsertCommand", conn ) )
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        var param = new SqlParameter();
                        param.ParameterName = "@HospitalId";
                        param.Value = e.HospitalId;
                        param.SqlDbType = System.Data.SqlDbType.Int;
                        cmd.Parameters.Add( param );

                        param = new SqlParameter();
                        param.ParameterName = "@PatientId";
                        param.Value = e.PatiendId;
                        param.SqlDbType = System.Data.SqlDbType.Int;
                        cmd.Parameters.Add( param );

                        param = new SqlParameter();
                        param.ParameterName = "@EventType";
                        param.Value = Convert.ToInt16( e.EventType );
                        param.SqlDbType = System.Data.SqlDbType.SmallInt;
                        cmd.Parameters.Add( param );

                        param = new SqlParameter();
                        param.ParameterName = "@EventTimestamp";
                        param.Value = e.EventTime;
                        param.SqlDbType = System.Data.SqlDbType.DateTime2;
                        cmd.Parameters.Add( param );

                        param = new SqlParameter();
                        param.ParameterName = "@DiseaseType";
                        if ( e.DiseaseType.HasValue )
                            param.Value = Convert.ToInt16( e.DiseaseType );
                        else
                            param.Value = DBNull.Value;
                        param.SqlDbType = System.Data.SqlDbType.SmallInt;
                        cmd.Parameters.Add( param );

                        param = new SqlParameter();
                        param.ParameterName = "@DoctorId";
                        if ( e.DoctorId.HasValue )
                            param.Value = e.DoctorId.Value;
                        else
                            param.Value = DBNull.Value;
                        param.SqlDbType = System.Data.SqlDbType.Int;
                        cmd.Parameters.Add( param );

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch ( Exception ex )
            {
                Console.WriteLine(ex);
            }
            finally
            {
                conn.Close();
            }
        }

        public static IEnumerable<IHospitalEvent> FindHospitalEventsAfter( int hospitalId, int afterEventId )
        {
            logger.Trace( "FindHospitalEventsAfter called" );
            var sw = Stopwatch.StartNew();

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
                        param.SqlDbType = System.Data.SqlDbType.Int;
                        cmd.Parameters.Add( param );

                        SqlDataReader dr = cmd.ExecuteReader();

                        while ( dr.Read() )
                        {
                            var e = new HospitalEvent
                            {
                                EventId = dr.GetInt32( 0 ),
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
                logger.Error( "ERROR: {0}", ex );
            }

            sw.Stop();
            logger.Trace( "FindHospitalEventsAfter completed in {0} ms", sw.ElapsedMilliseconds );

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
                logger.Error( "ERROR: {0}", ex );
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
                logger.Error( "ERROR: {0}", ex );
            }
          
            return diseases;
        }
    }
}
