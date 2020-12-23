using System;
using System.Collections.Generic;
using System.Text;

namespace AutoAttendance_Core.Models
{
    public class AttendanceDataModel
    {
        /// <summary>
        /// 출석체크할 게시판
        /// </summary>
        public string bo_table { get; set; }

        /// <summary>
        /// 출석체크할 게시글
        /// </summary>
        public string wr_id { get; set; }

        /// <summary>
        /// 출석체크할 날짜
        /// </summary>
        public DateTime datetime { get; set; }
    }

    public class CommentModel : AttendanceDataModel
    {
        /// <summary>
        /// 출석체크 문구
        /// </summary>
        public string comment { get; set; }
    }
}