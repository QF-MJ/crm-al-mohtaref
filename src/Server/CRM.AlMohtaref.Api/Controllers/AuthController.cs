using Microsoft.AspNetCore.Mvc;

namespace CRM.AlMohtaref.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // جلسات JWT حالية عديمة الحالة؛ إنهاء الجلسة يتم على العميل وهذا المسار يؤكد طلب تسجيل الخروج.
        return NoContent();
    }
}
