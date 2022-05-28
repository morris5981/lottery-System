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
                            
                            ;WITH nums AS
                               (SELECT 1 AS value
                                UNION ALL
                                SELECT value + 1 AS value
                                FROM nums
                                WHERE nums.value < @joinNum)
                            INSERT INTO Employee(EventId, EmployeeCode)
                            SELECT EventInfo.EventId AS EventId, CONCAT('A ', nums.value) 
                            FROM nums, EventInfo
                            WHERE EventInfo.EventName = @EventName
                            OPTION(MAXRECURSION 0)";
            
            int result;
            using (SqlConnection conn = new SqlConnection(this.GetDBConnectString()))
            {

                conn.Open();
                SqlTransaction tran = conn.BeginTransaction(IsolationLevel.ReadCommitted);
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@EventName", eventInfo.EventName));
                cmd.Parameters.Add(new SqlParameter("@joinNum", eventInfo.joinNum));
                cmd.Parameters.Add(new SqlParameter("@Awards", eventInfo.Awards));
                cmd.Parameters.Add(new SqlParameter("@AwardsDes", eventInfo.AwardsDes));
                cmd.Transaction = tran;
                try
                {
                    // 新增：將 InsertDate 方法的商業邏輯複製一份
                    cmd.CommandText = sql;
                    result = Convert.ToInt32(cmd.ExecuteScalar());
                    // Commit
                    tran.Commit();
                    conn.Close();
                    return true;
                }
                catch
                {
                    // Rollback
                    tran.Rollback();
                    conn.Close();
                    return false;
                }
                
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


        public List<Lottery_System.Model.Employee> GetListOfWinners(string eventId)
        {
            bool updateStatus = UpDateListOfWinners(eventId);
            List<Lottery_System.Model.Employee> employees = new List<Lottery_System.Model.Employee>();
            return employees;

        }

        /// <summary>
        /// 更新得獎名單
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public bool UpDateListOfWinners(string eventId)
        {
            string[] awardsDes = GetAwardsDes(eventId);
            // 由尾獎開始抽
            bool result = true;
            for (var i = (awardsDes.Count() - 1); i >= 0; i--)
            {
                string sql = @"UPDATE
                                    Emp
                                SET
                                    Emp.Awards = @Awards
                                FROM
                                    Employee AS Emp
                                WHERE
                                    Emp.EmployeeCode IN
	                                (SELECT TOP (@AwardsNum) EmployeeCode FROM Employee
	                                WHERE Employee.Awards IS NULL AND Employee.EventId = @eventId
	                                ORDER BY NEWID()) AND Emp.EventId = @eventId";
                using (SqlConnection conn = new SqlConnection(this.GetDBConnectString()))
                {

                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction(IsolationLevel.ReadCommitted);
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.Add(new SqlParameter("@eventId", eventId));
                    cmd.Parameters.Add(new SqlParameter("@Awards", i + 1));
                    cmd.Parameters.Add(new SqlParameter("@AwardsNum", Convert.ToInt32(awardsDes[i])));
                    cmd.Transaction = tran;
                    try
                    {
                        // 新增：將 InsertDate 方法的商業邏輯複製一份
                        cmd.CommandText = sql;
                        cmd.ExecuteScalar();
                        // Commit
                        tran.Commit();
                        conn.Close();
                    }
                    catch (InvalidCastException e)
                    {
                        // Rollback
                        tran.Rollback();
                        conn.Close();
                        result = false;
                    }
                }
            }
            return result;

        }


        /// <summary>
        /// 取得獎項敘述
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public string[] GetAwardsDes(string eventId)
        {
            string sql = @"SELECT AwardsDes  
                            FROM EventInfo
                            Where EventInfo.EventId = @eventId";
            string result;
            using (SqlConnection conn = new SqlConnection(this.GetDBConnectString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@eventId", eventId));
                result = Convert.ToString(cmd.ExecuteScalar());
                conn.Close();
            }
            string[] resultList = result.Split(',');

            return resultList;

        }
    }
}
