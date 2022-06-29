using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RegistrationAndLogin3.Models
{
    public class UserRegistration
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "FirstName(이름)을 입력해주세요.")]
        [Display(Name = "FirstName(이름)")]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "LastName(성)을 입력해주세요.")]
        [Display(Name = "LastName(성)")]
        public string LastName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Email을 입력해주세요.")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail 형식이 올바르지 않습니다.")]
        public string Email { get; set; }

        [Display(Name = "비밀번호")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "비밀번호를 입력해주세요.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Password \"{0}\" must have {2} character", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$@$!%*?&])[A-Za-z\d$@$!%*?&]{6,}$", ErrorMessage = "비빌번호는 최소 8자(대소문자, 숫자, 특수기호 최소 각각 1자)가 필요합니다.")]
        public string Password { get; set; }

        [Display(Name = "비밀번호 확인")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "비밀번호를 다시 입력해주세요.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "비밀번호가 일치하지 않습니다. 다시 시도해주세요!")]
        public string ConfirmPassword { get; set; }
    }
}