using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lottery_System.Model
{
    public class EventInfo
    {
        public int EventId { get; set; }

        /// <summary>
        /// 活動名稱
        /// </summary>
        [DisplayName("活動名稱")]
        [Required(ErrorMessage = "此欄位必填")]
        public string EventName { get; set; }


        [DisplayName("參加人數")]
        [Range(1, 9999999, ErrorMessage = "無效的輸入")]
        [Required(ErrorMessage = "此欄位必填")]
        public int joinNum { get; set; }


        [DisplayName("獎項")]
        [Range(1, 9999, ErrorMessage = "無效的輸入")]
        [Required(ErrorMessage = "此欄位必填")]
        public int AwardsNum { get; set; }

        public string AwardsDes { get; set; }
    }
}
