using AutoAttendance_Core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace AutoAttendance_Core.Services
{
    public class AttendanceService
    {
        private CookieContainer cookieContainer = new CookieContainer();
        private readonly string Id = null;
        private readonly string Pw = null;

        public AttendanceService(string id, string pw)
        {
            Id = HttpUtility.UrlEncode(id);
            Pw = HttpUtility.UrlEncode(pw);
        }

        #region 출석체크 로직

        /// <summary>
        /// 출석체크 로직을 실행한다.
        /// </summary>
        /// <param name="id">학과 홈페이지의 아이디</param>
        /// <param name="pw">학과 홈페이지의 비밀번호</param>
        /// <param name="postModel">출석체크정보</param>
        public void Run(CommentModel postModel)
        {
            login();

            if (postModel == null) // 출석체크정보 없음.
            {
                Console.WriteLine("NULL PostModel");
                return;
            }

            string token = GetToken();
            if (string.IsNullOrEmpty(token)) // 토큰 받아오기 실패
            {
                Console.WriteLine("Token Failed");
                return;
            }

            if (PostComment(token, postModel)) // 출석체크 실패
            {
                Console.WriteLine("Attendance Failed");
                return;
            }

            Console.WriteLine("Attendance Success");
        }


        /// <summary>
        /// 학과홈페이지에 로그인을 한다.
        /// </summary>
        /// <param name="id">학번</param>
        /// <param name="pw">비밀번호</param>
        /// <returns>로그인 실패시 true를 반환한다.</returns>
        public AttendanceService login()
        {
            string data = $"url=https%253A%252F%252Fpolyin.top&mb_id={Id}&mb_password={Pw}";

            string respText = HttpRequest("https://polyin.top/bbs/login_check.php", "https://polyin.top", data);

            return string.IsNullOrEmpty(respText)? null : this;
        }


        /// <summary>
        /// 댓글을 작성한다.
        /// </summary>
        /// <param name="token">댓글작성시 필요한 토큰</param>
        /// <param name="commentModel">댓글의 정보</param>
        /// <returns>댓글을 작성실패시 true를 반환한다.</returns>
        private bool PostComment(string token, CommentModel commentModel)
        {
            string data = $"token={token}&w=c&bo_table={commentModel.bo_table}&wr_id={commentModel.wr_id}& is_good=0&wr_content={commentModel.comment}";
            string respText = HttpRequest("https://polyin.top/bbs/write_comment_update.php", null, data);

            Console.WriteLine(respText);

            return respText.Contains("오류");
        }


        /// <summary>
        /// 댓글작성시 필요한 토큰을 받아온다.
        /// </summary>
        /// <returns>받아온 토큰을 반환한다.</returns>
        private string GetToken()
        {
            string respText = HttpRequest("https://polyin.top/bbs/ajax.comment_token.php");

            JObject obj = JObject.Parse(respText);
            return obj["token"].ToString();
        }

        #endregion


        /// <summary>
        /// 학과게시판의 공지로부터 출석체크게시판을 가져온다.
        /// </summary>
        /// <param name="url">학과게시판 url</param>
        /// <param name="hours">출석체크를할 시간</param>
        /// <returns></returns>
        public List<AttendanceDataModel> GetAttendanceData(string url, int[] hours)
        {
            string respText = HttpRequest(url);

            var page = new HtmlAgilityPack.HtmlDocument();
            page.LoadHtml(respText);

            Regex regex = new Regex("[0-9]*/[0-9]*");

            List<AttendanceDataModel> dataList = new List<AttendanceDataModel>();

            foreach (var node in page.DocumentNode.SelectNodes("//li[@class='list-item bg-light']"))
            {
                var innerNode = node.SelectSingleNode("div[@class='wr-subject']").SelectSingleNode("a[@class='item-subject']");

                var urlQuery = HttpUtility.ParseQueryString(innerNode.Attributes["href"].Value);
                string bo_table = urlQuery.GetValues(0)[0];
                string wr_id = urlQuery.GetValues(1)[0];

                string nodeTitle = innerNode.SelectSingleNode("b").InnerText;
                var datetime = DateTime.Parse(regex.Match(nodeTitle).Value);

                // 공지에있는 날짜가 현재 날짜보다 이전이라면 년도를 1씩 증가시킨다.
                datetime = (DateTime.Compare(datetime, DateTime.Now) < 0) ? datetime.AddYears(1) : datetime;
                
                // 지정된 시간을 추가한다.
                foreach (int hour in hours) dataList.Add(new AttendanceDataModel { bo_table = bo_table, wr_id = wr_id, datetime = datetime.AddHours(hour) });                
            }

            return dataList;
        }


        /// <summary>
        /// HTTPWebRequest를 보낸다.
        /// </summary>
        /// <param name="url">HttpRequest를 보낼 url</param>
        /// <param name="referer">Request의 referer값</param>
        /// <param name="data">Request에 함께 보낼 data</param>
        /// <returns></returns>
        public string HttpRequest(string url, string referer = null, string data = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Http.Get;
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "*/*";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.66 Safari/537.36";
            request.Referer = referer;
            request.CookieContainer = cookieContainer;

            // Post 방식일 때
            if (!string.IsNullOrEmpty(data))
            {
                request.Method = WebRequestMethods.Http.Post;

                byte[] bytes = Encoding.UTF8.GetBytes(data);
                request.ContentLength = bytes.Length;

                using (Stream reqStream = request.GetRequestStream())
                {
                    reqStream.Write(bytes, 0, bytes.Length);
                }
            }

            string responseText = null;
            using (HttpWebResponse resp = (HttpWebResponse)request.GetResponse())
            {
                Stream stream = resp.GetResponseStream();

                using (StreamReader sr = new StreamReader(stream))
                {
                    responseText = sr.ReadToEnd();
                }
            }

            return responseText;
        }
    }
}
