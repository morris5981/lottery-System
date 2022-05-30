using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lottery_System.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// 顯示抽獎頁面
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 動態產生活動select
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost()]
        public string GetEvenOption(string status)
        {
            Lottery_System.Service.LotteryService lotteryService = new Lottery_System.Service.LotteryService();
            var events = lotteryService.GetEventInfo(status);
            return JsonConvert.SerializeObject(events);
        }

        /// <summary>
        /// 抽獎並顯示名單
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost()]
        public string GetListOfWinners(string eventId, string award)
        {
            Lottery_System.Service.LotteryService lotteryService = new Lottery_System.Service.LotteryService();
            List<Lottery_System.Model.targetEvent> events = new List<Lottery_System.Model.targetEvent>();
            events = lotteryService.GetListOfWinners(eventId, award);
            return JsonConvert.SerializeObject(events);
            
        }

        /// <summary>
        /// 取得活動獎項
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost()]
        public string GetEventAwards(string eventId)
        {
            Lottery_System.Service.LotteryService lotteryService = new Lottery_System.Service.LotteryService();
            List<Lottery_System.Model.AwardsInfo> awardsInfos = new List<Lottery_System.Model.AwardsInfo>();
            awardsInfos = lotteryService.GetAwardsDes(eventId);
            return JsonConvert.SerializeObject(awardsInfos);

        }


        /// <summary>
        /// 取得歷史活動獎項
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public string GetHistoricalEventAwards(string eventId)
        {
            Lottery_System.Service.LotteryService lotteryService = new Lottery_System.Service.LotteryService();
            List<Lottery_System.Model.AwardsInfo> awardsInfos = new List<Lottery_System.Model.AwardsInfo>();
            awardsInfos = lotteryService.GetHistoricalEventAwards(eventId);
            return JsonConvert.SerializeObject(awardsInfos);

        }

        /// <summary>
        /// 取得歷史獲獎名單
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost()]
        public string GetHistoricalListOfWinners(string eventId, string award)
        {
            Lottery_System.Service.LotteryService lotteryService = new Lottery_System.Service.LotteryService();
            List<Lottery_System.Model.targetEvent> events = new List<Lottery_System.Model.targetEvent>();
            events = lotteryService.GetHistoricalListOfWinners(eventId, award);
            return JsonConvert.SerializeObject(events);

        }


        /// <summary>
        /// 顯示新增活動頁面
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        public ActionResult InsertEvent()
        {
            return View();
        }

        /// <summary>
        /// 新增活動
        /// </summary>
        /// <param name="eventInfo"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost()]
        public ActionResult InsertEvent(Lottery_System.Model.EventInfo eventInfo, FormCollection form)
        {
            if (!ModelState.IsValid)
            {
                // Return to the page with the Validation errorsMessages.
                return View();
            }
            string awardsDes = "";
            bool errorInput = false;
            for (var i = 1; i <= eventInfo.AwardsNum; i++)
            {
                string str = "Awards" + i;
                if (Convert.ToInt32(form[str]) <= 0)
                {
                    errorInput = true;
                    break;
                }
                if (string.IsNullOrEmpty(awardsDes)){
                    awardsDes = i + ":" + form[str];
                }
                else
                {
                    awardsDes = awardsDes + "," + i + ":" + form[str];
                }
            }
            if (errorInput)
            {
                TempData["ErrorMessage"] = "中獎人數輸入錯誤";
                return View();
            }
            eventInfo.AwardsDes = awardsDes;
            Lottery_System.Service.LotteryService lotteryService = new Lottery_System.Service.LotteryService();
            var result = lotteryService.InsertNewEvent(eventInfo);
            if (result)
            {
                TempData["SuccessMessage"] = "成功新增活動";
                return View("Index");
            }
            else
            {
                TempData["ErrorMessage"] = "格式輸入錯誤，或活動名稱重覆";
                return View();
            }
        }

        
        /// <summary>
        /// 歷史活動頁面
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        public ActionResult historicalEvent()
        {
            return View();
        }
    }
}