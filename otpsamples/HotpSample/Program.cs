using OtpNet;
using System.Text;

namespace HotpSample
{
    internal class Program
    {

        static readonly byte[] secretKey = Encoding.UTF8.GetBytes("otp:catcherwong@a.com_xxxx");
        static void Main(string[] args)
        {
            var hotp = new Hotp(secretKey, OtpHashMode.Sha512);

            var h1 = hotp.ComputeHOTP(1);
            var h2 = hotp.ComputeHOTP(2);

            Console.WriteLine($"h1:{h1}, h2:{h2}");
            Console.WriteLine($"v1:{hotp.VerifyHotp(h1, 1)}, " +
                $"{hotp.VerifyHotp(h1, 2)}; " +
                $"v2: {hotp.VerifyHotp(h2, 2)}");
            Console.ReadKey();
        }
    }
}
