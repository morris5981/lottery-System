using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lottery_System.Service
{
    public class LotteryService
    {
        public bool InsertNewEvent(Lottery_System.Model.EventInfo eventInfo)
        {
            Lottery_System.Dao.LotteryDao lotteryDao = new Lottery_System.Dao.LotteryDao();
            return lotteryDao.InsertNewEvent(eventInfo);
        }

        public List<Lottery_System.Model.EventInfo> GetEventInfo(string status)
        {
            Lottery_System.Dao.LotteryDao lotteryDao = new Lottery_System.Dao.LotteryDao();
            return lotteryDao.GetEventInfo(status);
        }

        public List<Lottery_System.Model.Employee> GetListOfWinners(string eventId, string award)
        {
            Lottery_System.Dao.LotteryDao lotteryDao = new Lottery_System.Dao.LotteryDao();
            return lotteryDao.GetListOfWinners(eventId, award);

        }

        public List<Lottery_System.Model.Employee> GetHistoricalListOfWinners(string eventId, string award)
        {
            Lottery_System.Dao.LotteryDao lotteryDao = new Lottery_System.Dao.LotteryDao();
            return lotteryDao.GetHistoricalListOfWinners(eventId, award);
        }

        public List<Lottery_System.Model.AwardsInfo> GetAwardsDes(string eventId)
        {
            Lottery_System.Dao.LotteryDao lotteryDao = new Lottery_System.Dao.LotteryDao();
            return lotteryDao.GetAwardsDes(eventId);
        }

        public List<Lottery_System.Model.AwardsInfo> GetHistoricalEventAwards(string eventId)
        {
            Lottery_System.Dao.LotteryDao lotteryDao = new Lottery_System.Dao.LotteryDao();
            return lotteryDao.GetHistoricalEventAwards(eventId);

        }
    }
}
