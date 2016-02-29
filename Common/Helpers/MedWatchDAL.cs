using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Common.Entities;

namespace Common.Helpers
{
    public static class MedWatchDAL
    {
        private const string ConnectionString = "Data Source=127.0.0.1;Initial Catalog=MedWatch;Integrated Security=True";
        
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
                        param.Value = e.PatiendId;
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
                        if ( e.DiseaseType.HasValue )
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
                // TODO log error
            }
            finally
            {
                conn.Close();
            }
        }

        public static IEnumerable<IHospitalEvent> FindHospitalEventsAfter( int hospitalId, int afterEventId )
        {
            var events = new List<IHospitalEvent>();

            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection( ConnectionString );
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
            }
            catch ( Exception ex )
            {
                // TODO log error
            }
            finally
            {
                conn.Close();
            }

            return events;
        }

        public static IEnumerable<Hospital> FindHospitals()
        {
            var hospitals = new List<Hospital>();

            string sql = "SELECT Id, Name FROM Hospital ORDER BY Name";

            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection( ConnectionString );
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
                        };

                        hospitals.Add( h );
                    }
                }
            }
            catch ( Exception ex )
            {
                // TODO log error
            }
            finally
            {
                conn.Close();
            }

            return hospitals;
        }
    }
}
