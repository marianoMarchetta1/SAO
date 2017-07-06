using IngematicaAngularBase.Model;
using IngematicaAngularBase.Model.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace IngematicaAngularBase.Dal
{
    public class LogDataAccess
    {
        private object GetVarcharParam(string value)
        {
            if (value != null)
                return value;
            else return DBNull.Value;
        }

        public int InsertLog(LogDTO log)
        {
            int idError = 0;
            SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Proyecto.Final"].ConnectionString);
            Conn.Open();
            try
            {
                SqlCommand command = new SqlCommand("InsertLog", Conn);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@Descripcion", SqlDbType.VarChar));
                command.Parameters["@Descripcion"].Value = GetVarcharParam(log.Descripcion);
                command.Parameters.Add(new SqlParameter("@Descripcion2", SqlDbType.VarChar));
                command.Parameters["@Descripcion2"].Value = GetVarcharParam(log.Descripcion2);
                command.Parameters.Add(new SqlParameter("@Controller", SqlDbType.VarChar));
                command.Parameters["@Controller"].Value = GetVarcharParam(log.Controller);
                command.Parameters.Add(new SqlParameter("@Accion", SqlDbType.VarChar));
                command.Parameters["@Accion"].Value = GetVarcharParam(log.Accion);
                command.Parameters.Add(new SqlParameter("@OtrosDatos", SqlDbType.VarChar));
                command.Parameters["@OtrosDatos"].Value = GetVarcharParam(log.OtrosDatos);
                command.Parameters.Add(new SqlParameter("@Usuario", SqlDbType.VarChar));
                command.Parameters["@Usuario"].Value = GetVarcharParam(log.Usuario);
                try
                {
                    idError = (int)command.ExecuteScalar();
                }
                catch (Exception)
                {
                    command.Cancel();
                    throw;
                }
            }
            finally
            {
                Conn.Close();
            }

            return idError;
        }
    }
}
