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

        public List<Lottery_System.Model.EventInfo> GetEventInfo()
        {
            Lottery_System.Dao.LotteryDao lotteryDao = new Lottery_System.Dao.LotteryDao();
            return lotteryDao.GetEventInfo();
        }
    }
}
