using AutoAttendance_Core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace AutoAttendance_Core.Services
{
    public class AttendanceService
    {
        private CookieContainer cookieContainer = new CookieContainer();


        /// <summary>
        /// 출석체크 로직을 실행한다.
        /// </summary>
        /// <param name="id">학과 홈페이지의 아이디</param>
        /// <param name="pw">학과 홈페이지의 비밀번호</param>
        /// <param name="postModel">출석체크정보</param>
        public void Run(string id, string pw, CommentModel postModel)
        {
            if (postModel == null) // 출석체크정보 없음.
            {
                Console.WriteLine("NULL PostModel");
                return;
            }

            if (login(id, pw)) // 로그인 실패
            {
                Console.WriteLine("로그인 실패");
                return;
            }

            string token = GetToken();
            if (string.IsNullOrEmpty(token)) // 토큰 받아오기 실패
            {
                Console.WriteLine("토큰 실패");
                return;
            }

            if (PostComment(token, postModel)) // 출석체크 실패
            {
                Console.WriteLine("출석체크 실패");
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
        private bool login(string id, string pw)
        {
            id = HttpUtility.UrlEncode(id);
            pw = HttpUtility.UrlEncode(pw);

            string data = $"url=https%253A%252F%252Fpolyin.top&mb_id={id}&mb_password={pw}";

            string respText = HttpRequest("https://polyin.top/bbs/login_check.php", "https://polyin.top", data);

            return respText.Contains("오류");
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



        /// <summary>
        /// HTTPWebRequest를 보낸다.
        /// </summary>
        /// <param name="url">HttpRequest를 보낼 url</param>
        /// <param name="referer">Request의 referer값</param>
        /// <param name="data">Request에 함께 보낼 data</param>
        /// <returns></returns>
        private string HttpRequest(string url, string referer = null, string data = null)
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
