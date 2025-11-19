using Do_an_1.Models;
using Microsoft.AspNetCore.Http;

namespace Do_an_1.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(HttpContext httpContext, VnPayRequest request);
        bool ValidateSignature(IQueryCollection query);
    }
}
