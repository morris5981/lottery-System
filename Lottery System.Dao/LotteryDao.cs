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
                            
                            DECLARE @SQL NVARCHAR(max)
                            DECLARE @count varchar(255)
                            DECLARE @generateNum int
                            DECLARE @generateEventName varchar(255)
                            SET @generateNum = @joinNum
                            SET @generateEventName = @EventName
                            SET @count = CONCAT('event', (SELECT EventInfo.EventId FROM EventInfo
	                            WHERE EventInfo.EventName = @EventName))

                            SET @SQL=N'CREATE TABLE ' + @count + '(
                                EmployeeCode varchar(255),
	                            Awards int
                            );'
                            EXEC(@SQL)

                            SET @SQL=N';WITH nums AS
                                (SELECT 1 AS value
                                UNION ALL
                                SELECT value + 1 AS value
                                FROM nums
                                WHERE nums.value < ' + CAST(@generateNum AS NVARCHAR(10)) + ')
                            INSERT INTO ' + @count + '(EmployeeCode)
                            SELECT CONCAT(''A '', nums.value) 
                            FROM nums, EventInfo
                            WHERE EventInfo.EventName = ''' + @generateEventName + '''
                            OPTION(MAXRECURSION 0)'
                            EXEC(@SQL)";
            
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
                catch(Exception e)
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
                        WHERE EventInfo.isSelected != 2
                        ORDER BY EventInfo.EventId DESC";
            }
            else
            {
                sql = @"SELECT *
                        FROM EventInfo
                        WHERE EventInfo.isSelected != 0";
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
        public List<Lottery_System.Model.targetEvent> GetListOfWinners(string eventId, string award)
        {
            bool upDateListOfWinnersStatus = UpDateListOfWinners(eventId, award);
            if (upDateListOfWinnersStatus)
            {
                UpDateAwardsDes(eventId, award);
                string sql = @"DECLARE @SQL NVARCHAR(max)
                                DECLARE @targetEventId int
                                DECLARE @targetAwards int
                                SET @targetEventId = @eventId
                                SET @targetAwards = @award
                                SET @SQL = N'SELECT * FROM event'+ CAST(@targetEventId AS NVARCHAR(10)) +' AS targetEvent
                                WHERE targetEvent.Awards = '+ CAST(@targetAwards AS NVARCHAR(10))
                                EXEC(@SQL)
                                ";
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
                List<Lottery_System.Model.targetEvent> targetEvents = new List<Lottery_System.Model.targetEvent>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Lottery_System.Model.targetEvent targetEvent = new Lottery_System.Model.targetEvent();
                    targetEvent.EmployeeCode = dt.Rows[i]["EmployeeCode"].ToString();
                    targetEvent.Awards = Convert.ToInt32(dt.Rows[i]["Awards"]);
                    targetEvents.Add(targetEvent);
                }
                return targetEvents;
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
        public List<Lottery_System.Model.targetEvent> GetHistoricalListOfWinners(string eventId, string award)
        {
            string sql = @"DECLARE @SQL NVARCHAR(max)
                            DECLARE @targetEventId int
                            DECLARE @targetAwards int
                            SET @targetEventId = @eventId
                            SET @targetAwards = @award
                            SET @SQL = N'SELECT * FROM event'+ CAST(@targetEventId AS NVARCHAR(10)) +' AS targetEvent
                            WHERE targetEvent.Awards = '+ CAST(@targetAwards AS NVARCHAR(10))
                            EXEC(@SQL)
                            "; 
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
            List<Lottery_System.Model.targetEvent> targetEvents = new List<Lottery_System.Model.targetEvent>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Lottery_System.Model.targetEvent targetEvent = new Lottery_System.Model.targetEvent();
                targetEvent.EmployeeCode = dt.Rows[i]["EmployeeCode"].ToString();
                targetEvent.Awards = Convert.ToInt32(dt.Rows[i]["Awards"]);
                targetEvents.Add(targetEvent);
            }
            return targetEvents;

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
            string sql = @"DECLARE @SQL NVARCHAR(max)
                            DECLARE @targetEventId int
                            DECLARE @targetAwardsNum int
                            DECLARE @targetAwards int
                            SET @targetEventId = @eventId
                            SET @targetAwardsNum = @AwardsNum
                            SET @targetAwards = @Awards
                            SET @SQL = N'UPDATE
	                            targetEvent
                            SET
	                            targetEvent.Awards = '+ CAST(@targetAwards AS NVARCHAR(10)) +'
                            FROM
	                            event'+ CAST(@targetEventId AS NVARCHAR(10)) +' AS targetEvent
                            WHERE
	                            targetEvent.EmployeeCode IN
	                            (SELECT TOP ('+ CAST(@targetAwardsNum AS NVARCHAR(10)) + ') EmployeeCode 
	                            FROM event'+ CAST(@targetEventId AS NVARCHAR(10)) +' AS subTargetEvent
	                            WHERE subTargetEvent.Awards IS NULL 
	                            ORDER BY NEWID())'
                            EXEC(@SQL)";
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
                catch
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
                            EventInfo.isSelected = 2
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
                            EventInfo.AwardsDes = @awardsDes, EventInfo.isSelected = 1
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
            string sql = @"DECLARE @SQL NVARCHAR(max)
                            DECLARE @targetEventId int
                            SET @targetEventId = @eventId
                            SET @SQL = N'SELECT Awards, COUNT(targetEvent.Awards) AS AwardsNum  
                            FROM event'+ CAST(@targetEventId AS NVARCHAR(10)) +' AS targetEvent
                            WHERE
                                targetEvent.Awards IS NOT NULL
                            GROUP BY Awards
                            ORDER BY Awards ASC'
                            EXEC(@SQL)";
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
