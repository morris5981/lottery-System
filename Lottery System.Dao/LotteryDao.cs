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
            string sql = @"INSERT INTO EventInfo (EventName, joinNum, AwardsNum, AwardsDes)
                            VALUES (@EventName, @joinNum, @AwardsNum, @AwardsDes);
                            
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
                cmd.Parameters.Add(new SqlParameter("@AwardsNum", eventInfo.AwardsNum));
                cmd.Parameters.Add(new SqlParameter("@AwardsDes", eventInfo.AwardsDes));
                cmd.Transaction = tran;
                try
                {
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
        public List<Lottery_System.Model.EventInfo> GetEventInfo(string status)
        {
            string sql = @"";
            if (status == "0")
            {
                sql = @"SELECT *
                        FROM EventInfo
                        WHERE EventInfo.isSelected = 0";
            }
            else
            {
                sql = @"SELECT EventInfo.EventId, EventInfo.EventName, EventInfo.joinNum, EventInfo.AwardsNum, EventInfo.AwardsDes, EventInfo.isSelected
                        FROM EventInfo, Employee
                        WHERE EventInfo.EventId = Employee.EventId AND Employee.Awards IS NOT NULL
                        GROUP BY EventInfo.EventId, EventInfo.EventName, EventInfo.joinNum, EventInfo.AwardsNum, EventInfo.AwardsDes, EventInfo.isSelected";
            }
            
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
                eventInfo.AwardsNum = Convert.ToInt32(dt.Rows[i]["AwardsNum"]);
                eventInfos.Add(eventInfo);
            }
            return eventInfos;
        }

        /// <summary>
        /// 取得獲獎人員以及更新名單
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<Lottery_System.Model.Employee> GetListOfWinners(string eventId, string award)
        {
            bool upDateListOfWinnersStatus = UpDateListOfWinners(eventId, award);
            if (upDateListOfWinnersStatus)
            {
                UpDateAwardsDes(eventId, award);
                string sql = @"SELECT *  
                                FROM Employee
                                WHERE Employee.EventId = @eventId AND Employee.Awards = @award";
                DataTable dt = new DataTable();
                using (SqlConnection conn = new SqlConnection(this.GetDBConnectString()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.Add(new SqlParameter("@eventId", eventId));
                    cmd.Parameters.Add(new SqlParameter("@award", award));
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    sqlDataAdapter.Fill(dt);
                    conn.Close();
                }
                List<Lottery_System.Model.Employee> employeeList = new List<Lottery_System.Model.Employee>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Lottery_System.Model.Employee employee = new Lottery_System.Model.Employee();
                    employee.EventId = Convert.ToInt32(dt.Rows[i]["EventId"]);
                    employee.EmployeeCode = dt.Rows[i]["EmployeeCode"].ToString();
                    employee.Awards = Convert.ToInt32(dt.Rows[i]["Awards"]);
                    employeeList.Add(employee);
                }
                return employeeList;
            }
            else
            {
                return null;
            }

        }


        /// <summary>
        /// 取得歷史獲獎人員
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<Lottery_System.Model.Employee> GetHistoricalListOfWinners(string eventId, string award)
        {
            string sql = @"SELECT *  
                            FROM Employee
                            WHERE Employee.EventId = @eventId AND Employee.Awards = @award";
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(this.GetDBConnectString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@eventId", eventId));
                cmd.Parameters.Add(new SqlParameter("@award", award));
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                sqlDataAdapter.Fill(dt);
                conn.Close();
            }
            List<Lottery_System.Model.Employee> employeeList = new List<Lottery_System.Model.Employee>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Lottery_System.Model.Employee employee = new Lottery_System.Model.Employee();
                employee.EventId = Convert.ToInt32(dt.Rows[i]["EventId"]);
                employee.EmployeeCode = dt.Rows[i]["EmployeeCode"].ToString();
                employee.Awards = Convert.ToInt32(dt.Rows[i]["Awards"]);
                employeeList.Add(employee);
            }
            return employeeList;

        }

        /// <summary>
        /// 更新得獎名單
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public bool UpDateListOfWinners(string eventId, string award)
        {
            List<Lottery_System.Model.AwardsInfo> awardsInfos = new List<Lottery_System.Model.AwardsInfo>();
            awardsInfos = GetAwardsDes(eventId);
            // 由尾獎開始抽
            bool result = true;
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
                cmd.Parameters.Add(new SqlParameter("@Awards", award));
                int AwardsNum = 0;
                for (var i=0; i< awardsInfos.Count; i++)
                {
                    if (awardsInfos[i].Awards == Convert.ToInt32(award))
                    {
                        AwardsNum = awardsInfos[i].AwardsNum;
                        break;
                    }
                }
                cmd.Parameters.Add(new SqlParameter("@AwardsNum", AwardsNum));
                cmd.Transaction = tran;
                try
                {
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
            return result;
        }
        

        /// <summary>
        /// 更新獎項描述
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="award"></param>
        public void UpDateAwardsDes(string eventId, string award)
        {
            List<Lottery_System.Model.AwardsInfo> awardsInfos = new List<Lottery_System.Model.AwardsInfo>();
            awardsInfos = GetAwardsDes(eventId);
            string awardsDes = "";
            for (var i = 0; i < awardsInfos.Count; i++)
            {
                if (awardsInfos[i].Awards == Convert.ToInt32(award))
                {
                    continue;
                }
                if (string.IsNullOrEmpty(awardsDes))
                {
                    awardsDes = awardsInfos[i].Awards + ":" + awardsInfos[i].AwardsNum;
                }
                else
                {
                    awardsDes = awardsDes + "," + awardsInfos[i].Awards + ":" + awardsInfos[i].AwardsNum;
                }
            }
            string sql = @"";
            if (string.IsNullOrEmpty(awardsDes))
            {
                sql = @"UPDATE
                            EventInfo
                        SET
                            EventInfo.isSelected = 1
                        FROM
                            EventInfo
                        WHERE
                            EventInfo.EventId = @eventId";
            }
            else
            {
                sql = @"UPDATE
                            EventInfo
                        SET
                            EventInfo.AwardsDes = @awardsDes
                        FROM
                            EventInfo
                        WHERE
                            EventInfo.EventId = @eventId";
            }
            
            using (SqlConnection conn = new SqlConnection(this.GetDBConnectString()))
            {

                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@eventId", eventId));
                cmd.Parameters.Add(new SqlParameter("@awardsDes", awardsDes));
                cmd.ExecuteScalar();
                conn.Close();
            }
        }


        /// <summary>
        /// 取得獎項敘述
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public List<Lottery_System.Model.AwardsInfo> GetAwardsDes(string eventId)
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
            List<Lottery_System.Model.AwardsInfo> awardsInfos = new List<Lottery_System.Model.AwardsInfo>();
            for (var i = 0; i < resultList.Length; i++)
            {
                Lottery_System.Model.AwardsInfo awardsInfo = new Lottery_System.Model.AwardsInfo();
                string[] temp = resultList[i].Split(':');
                awardsInfo.Awards = Convert.ToInt32(temp[0]);
                awardsInfo.AwardsNum = Convert.ToInt32(temp[1]);
                awardsInfos.Add(awardsInfo);
            }

            return awardsInfos;

        }

        /// <summary>
        /// 取得歷史活動獎項
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public List<Lottery_System.Model.AwardsInfo> GetHistoricalEventAwards(string eventId)
        {
            string sql = @"SELECT Awards, COUNT(Employee.Awards) AS AwardsNum  
                            FROM Employee
                            WHERE
                                Employee.EventId = @eventId AND Employee.Awards IS NOT NULL
                            GROUP BY Awards
                            ORDER BY Awards ASC";
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(this.GetDBConnectString()))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@eventId", eventId));
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                sqlDataAdapter.Fill(dt);
                conn.Close();
            }

            List<Lottery_System.Model.AwardsInfo> awardsInfos = new List<Lottery_System.Model.AwardsInfo>();
            for (var i = 0; i < dt.Rows.Count; i++)
            {
                Lottery_System.Model.AwardsInfo awardsInfo = new Lottery_System.Model.AwardsInfo();
                awardsInfo.Awards = Convert.ToInt32(dt.Rows[i]["Awards"]);
                awardsInfo.AwardsNum = Convert.ToInt32(dt.Rows[i]["AwardsNum"]);
                awardsInfos.Add(awardsInfo);
            }

            return awardsInfos;

        }
    }
}
