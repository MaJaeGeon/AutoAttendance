using AutoAttendance_Core.Services;
using AutoAttendance_Core.Models;
using System;
using System.Collections.Generic;
using System.Timers;
using System.Threading;

namespace AutoAttendance_Core
{
    class Program
    {
        static void Main(string[] args)
        {
            #region 서비스 준비

            AttendanceService service = new AttendanceService("id", "password").login();

            var attendanceData2 = service.GetAttendanceData("https://polyin.top/bbs/board.php?bo_table=bbs_202002_04", new int[] { 9, 14 });

            // 데이터 정렬
            attendanceData2.Sort(delegate (AttendanceDataModel A, AttendanceDataModel B)
            {
                return DateTime.Compare(A.datetime, B.datetime);
            });

            List<AttendanceDataModel> attendanceData = new List<AttendanceDataModel>();

            // 출석 시간 지난 데이터 삭제
            foreach (var ad in attendanceData2)
            {
                if (DateTime.Now.CompareTo(ad.datetime) != 1) attendanceData.Add(ad);
            }

            #endregion


            System.Timers.Timer timer = new System.Timers.Timer(10 * 1000);

            timer.Elapsed += (sender, e) =>
            {
                if (attendanceData.Count == 0) return;

                // 현재시간이 attendanceData[0] 의 시간과 같다면 출석을 해야되는 시간이니 댓글을 작성한다.
                if (DateTime.Now.ToString("yyyy/MM/dd HH:mm") == attendanceData[0].datetime.ToString("yyyy/MM/dd HH:mm"))
                {
                    service.Run(new CommentModel
                    {
                        bo_table = attendanceData[0].bo_table,
                        wr_id = attendanceData[0].wr_id,
                        datetime = attendanceData[0].datetime,
                        comment = "Comment"
                    });

                    attendanceData.RemoveAt(0);
                }

            };

            timer.Start();

            while (true)
            {
                string input = Console.ReadLine();

                switch (input)
                {
                    case "show":
                        if (attendanceData.Count > 0)
                            foreach (var i in attendanceData)
                                Console.WriteLine($"Queue : {i.bo_table}, {i.wr_id}, {i.datetime}");
                        else Console.WriteLine("You have completed all attendance.");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}