using RegistrationAndLogin3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Windows;

namespace RegistrationAndLogin3.Controllers
{
    public class RegisterController : Controller
    {
        testEntities objCon = new testEntities();

        // GET: Register
        public ActionResult Index()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        public ActionResult Index(UserM objUsr)
        {
            // email not verified on registration time    
            objUsr.EmailVerification = false;

            // Email exists or not
            var IsExists = IsEmailExists(objUsr.Email);
            if (IsExists)
            {
                ModelState.AddModelError("EmailExists", "Email이 이미 존재합니다.");
                return View();
            }

            // GUID(전역 고유 식별자) 방식으로 ActivetionCode 생성.
            objUsr.ActivetionCode = Guid.NewGuid();
            //password convert    
            objUsr.Password = RegistrationAndLogin3.Models.encryptPassword.textToEncrypt(objUsr.Password);
            objCon.UserMs.Add(objUsr);
            objCon.SaveChanges();

            // 이메일 인증 링크
            SendEmailToUser(objUsr.Email, objUsr.ActivetionCode.ToString());
            var Message = "회원가입이 완료되었습니다. 이메일 인증하기 => " + objUsr.Email;
            ViewBag.Message = Message;

            return View("Registration");
        }

        // 이메일 중복체크
        public bool IsEmailExists(string eMail)
        {
            var IsCheck = objCon.UserMs.Where(email => email.Email == eMail).FirstOrDefault();
            return IsCheck != null;
        }

        // 회원가입 이메일 인증시 SMTP
        public void SendEmailToUser(string emailId, string activationCode)
        {
            var GenerateUserVerificationLink = "/Register/UserVerification/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, GenerateUserVerificationLink);

            var fromMail = new MailAddress("lim950808@naver.com", "스룩페이"); // 보내는 사람(관리자, admin) 이메일  
            var fromEmailpassword = "Dlawodjr0808"; // 보내는 사람(관리자, admin) 비밀번호     
            var toEmail = new MailAddress(emailId); // 받는 사람 이메일

            var smtp = new SmtpClient();
            smtp.Host = "smtp.naver.com"; // 네이버 메일. Gmail은 2022.05.30일부터 보안 수준이 낮은 앱 서비스 종료.
            smtp.Port = 587; // port는 naver랑 Gmail 모두 587
            smtp.EnableSsl = true; // SSL(Secure Sockets Layer)를 이용하여 연결을 암호화 할지 여부
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(fromMail.Address, fromEmailpassword);

            var Message = new MailMessage(fromMail, toEmail);
            Message.Subject = "회원가입 이메일 인증";
            Message.Body = "<br/> 회원가입이 성공적으로 완료되었습니다." +
                           "<br/> 아래의 링크를 눌러 이메일 인증을 해주세요." +
                           "<br/><br/><a href=" + link + ">" + link + "</a>";
            Message.IsBodyHtml = true;
            smtp.Send(Message); // 예) https://localhost:44343/Register/UserVerification/8782e81a-99d6-4565-910b-f422260b4c70
        }

        // 회원가입 후 이메일 인증
        public ActionResult UserVerification(string id)
        {
            bool Status = false;

            objCon.Configuration.ValidateOnSaveEnabled = false; // Ignor to password confirmation     
            var IsVerify = objCon.UserMs.Where(u => u.ActivetionCode == new Guid(id)).FirstOrDefault();

            if (IsVerify != null)
            {
                IsVerify.EmailVerification = true;
                objCon.SaveChanges();
                ViewBag.Message = "이메일 인증이 완료되었습니다.";
                Status = true;
            }
            else
            {
                ViewBag.Message = "유효하지 않은 요청. 이메일 인증 실패!";
                ViewBag.Status = false;
            }
            return View();
        }

        // GET: Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public ActionResult Login(UserLogin LgnUsr)
        {
            var _passWord = RegistrationAndLogin3.Models.encryptPassword.textToEncrypt(LgnUsr.Password);
            bool Isvalid = objCon.UserMs.Any(x => x.Email == LgnUsr.EmailId && x.EmailVerification == true && x.Password == _passWord);
            if (Isvalid)
            {
                int timeout = LgnUsr.Rememberme ? 60 : 5; // Timeout in minutes, 60 = 1 hour.    
                var ticket = new FormsAuthenticationTicket(LgnUsr.EmailId, false, timeout);
                string encrypted = FormsAuthentication.Encrypt(ticket);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                cookie.Expires = System.DateTime.Now.AddMinutes(timeout);
                cookie.HttpOnly = true;
                Response.Cookies.Add(cookie);
                return RedirectToAction("Index", "UserDash");
            }
            else
            {
                ModelState.AddModelError("", "해당 이메일 또는 비밀번호가 존재하지 않습니다. 다시 시도해주세요!");
            }
            return View();
        }

        // 로그아웃
        [Authorize]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Register");
        }

        // 비밀번호 변경페이지로 이동
        public ActionResult ForgetPassword()
        {
            return View();
        }

        // 비밀번호 찾기
        [HttpPost]
        public ActionResult ForgetPassword(ForgetPassword forgetPassword)
        {
            var IsExists = IsEmailExists(forgetPassword.EmailId);
            if (!IsExists)
            {
                ModelState.AddModelError("EmailNotExists", "해당 이메일 주소가 존재하지 않습니다.");
                return View();
            }
            var objUsr = objCon.UserMs.Where(x => x.Email == forgetPassword.EmailId).FirstOrDefault();

            // Genrate OTP     
            string OTP = GenerateOTP();

            objUsr.ActivetionCode = Guid.NewGuid();
            objUsr.OTP = OTP;
            objCon.Entry(objUsr).State = System.Data.Entity.EntityState.Modified;
            objCon.SaveChanges();

            ForgetPasswordEmailToUser(objUsr.Email, objUsr.ActivetionCode.ToString(), objUsr.OTP);
            var Message = "발송된 이메일 내 URL를 클릭 후 발급된 OTP 인증번호와 함께 새로운 비밀번호를 입력해주세요. => " + objUsr.Email;
            ViewBag.Message = Message;

            return View("FindPassword");
        }

        // OTP 생성
        public string GenerateOTP()
        {
            string OTPLength = "4";
            string OTP = string.Empty;

            string Chars = string.Empty;
            Chars = "1,2,3,4,5,6,7,8,9,0";

            char[] splitChar = { ',' };
            string[] arr = Chars.Split(splitChar);
            string NewOTP = "";
            string temp = "";
            Random random = new Random();
            for (int i = 0; i < Convert.ToInt32(OTPLength); i++)
            {
                temp = arr[random.Next(0, arr.Length)];
                NewOTP += temp;
                OTP = NewOTP;
            }
            return OTP;
        }

        // 비밀번호 변경시 SMTP
        public void ForgetPasswordEmailToUser(string emailId, string activationCode, string OTP)
        {
            var GenerateUserVerificationLink = "/Register/ChangePassword/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, GenerateUserVerificationLink);
            var fromMail = new MailAddress("lim950808@naver.com", "Srook");
            var fromEmailpassword = "Dlawodjr0808";
            var toEmail = new MailAddress(emailId);

            var smtp = new SmtpClient();
            smtp.Host = "smtp.naver.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(fromMail.Address, fromEmailpassword);

            var Message = new MailMessage(fromMail, toEmail);
            Message.Subject = "비밀번호 변경을 위한 OTP 인증번호 발급";
            Message.Body = "<br/> 아래의 링크에 들어가 비밀번호 변경을 완료해주세요." +
                            "<br/><br/><a href=" + link + ">" + link + "</a>" +
                            "<br/> OTP 인증번호 : " + OTP;
            Message.IsBodyHtml = true;
            smtp.Send(Message);
        }

        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePassword changePassword)
        {
            var IsVerify = objCon.UserMs.Where(u => u.OTP == changePassword.OTP).FirstOrDefault();
            changePassword.Password = RegistrationAndLogin3.Models.encryptPassword.textToEncrypt(changePassword.Password);

            if (IsVerify != null)
            {
                objCon.Entry(IsVerify).State = System.Data.Entity.EntityState.Modified;
                objCon.SaveChanges();
                MessageBox.Show("비밀번호가 변경되었습니다. 로그인해주세요.");
                return RedirectToAction("Login", "Register");
            }
            else
            {       
                MessageBox.Show("OTP 인증번호가 올바르지 않습니다. 다시 입력해주세요.");
            }
            return View();
        }
    }
}