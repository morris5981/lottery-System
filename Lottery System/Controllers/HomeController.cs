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
        public string Index(FormCollection form)
        {
            Lottery_System.Service.LotteryService lotteryService = new Lottery_System.Service.LotteryService();
            var events = lotteryService.GetEventInfo();
            return JsonConvert.SerializeObject(events);
        }

        /// <summary>
        /// 抽獎並顯示名單
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost()]
        public string GetListOfWinners(string eventId)
        {
            Lottery_System.Service.LotteryService lotteryService = new Lottery_System.Service.LotteryService();
            List<Lottery_System.Model.Employee> events = new List<Lottery_System.Model.Employee>();
            events = lotteryService.GetListOfWinners(eventId);
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
            string awardsDes = "";
            for (var i = 1; i <= eventInfo.Awards; i++)
            {
                string str = "Awards" + i;
                if (string.IsNullOrEmpty(awardsDes)){
                    awardsDes = form[str];
                }
                else
                {
                    awardsDes = awardsDes + "," + form[str];
                }
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
                TempData["ErrorMessage"] = "輸入錯誤(可能是輸入到重覆的活動名稱)";
                return View();
            }
        }
    }
}