using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lottery_System.Dao
{
    public class LotteryDao
    {
        /// <summary>
        /// 取得DB連線字串
        /// </summary>
        /// <returns></returns>
        private string GetDBConnectString()
        {
            return Lottery_System.Common.ConfigTool.GetConnectionString("Default");
        }
        

        /// <summary>
        /// insert new event
        /// </summary>
        /// <param name="eventIfo"></param>
        /// <returns></returns>
        public bool InsertNewEvent(Lottery_System.Model.EventInfo eventInfo)
        {
            string sql = @"INSERT INTO EventInfo (EventName, joinNum, Awards, AwardsDes)
                            VALUES (@EventName, @joinNum, @Awards, @AwardsDes);
                            
                            INSERT INTO Employee(EventName, EmployeeCode)
                            SELECT @EventName AS EventName, 'A ' + CONVERT(VARCHAR(4), N)
                            FROM 
                            (
                                SELECT DISTINCT NUMBER AS N
                                FROM master.dbo.spt_values
                                WHERE name IS NULL
                            ) NumberPool
                            WHERE N BETWEEN 1 AND @joinNum
                            ORDER BY N";
            //string sql = @"SELECT *
            //                FROM EventInfo";
            int result;
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(this.GetDBConnectString()))
            {

                conn.Open();
                SqlTransaction tran = conn.BeginTransaction(IsolationLevel.ReadCommitted);
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@EventName", eventInfo.EventName));
                cmd.Parameters.Add(new SqlParameter("@joinNum", eventInfo.joinNum));
                cmd.Parameters.Add(new SqlParameter("@Awards", eventInfo.Awards));
                cmd.Parameters.Add(new SqlParameter("@AwardsDes", eventInfo.AwardsDes));
                //SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                //sqlDataAdapter.Fill(dt);
                cmd.Transaction = tran;
                try
                {
                    // 新增：將 InsertDate 方法的商業邏輯複製一份
                    cmd.CommandText = sql;
                    result = Convert.ToInt32(cmd.ExecuteScalar());
                    // Commit
                    tran.Commit();
                    return true;
                }
                catch
                {
                    // Rollback
                    tran.Rollback();
                    return false;
                }
                conn.Close();
            }
        }

        /// <summary>
        /// Get event info
        /// </summary>
        /// <returns></returns>
        public List<Lottery_System.Model.EventInfo> GetEventInfo()
        {
            string sql = @"SELECT *
                            FROM EventInfo";
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(this.GetDBConnectString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                sqlDataAdapter.Fill(dt);
                conn.Close();
            }
            List<Lottery_System.Model.EventInfo> eventInfos = new List<Lottery_System.Model.EventInfo>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Lottery_System.Model.EventInfo eventInfo = new Lottery_System.Model.EventInfo();
                eventInfo.EventId = Convert.ToInt32(dt.Rows[i]["EventId"]);
                eventInfo.EventName = dt.Rows[i]["EventName"].ToString();
                eventInfo.joinNum = Convert.ToInt32(dt.Rows[i]["joinNum"]);
                eventInfo.Awards = Convert.ToInt32(dt.Rows[i]["Awards"]);
                eventInfos.Add(eventInfo);
            }
            return eventInfos;
        }


        //public List<Lottery_System.Model.Employee> GetListOfWinners()
        //{


        //}
    }
}
