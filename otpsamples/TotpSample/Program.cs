using OtpNet;
using System.Text;

namespace TotpSample
{
    internal class Program
    {
        static readonly byte[] secretKey = Encoding.UTF8.GetBytes("otp:catcherwong@a.com_xxxx");

        static void Main(string[] args)
        {
            // Normal();

            // FixTime();

            Window();

            //OnlyOnce();

            // Size();

            Console.WriteLine("done");
            Console.ReadKey();
        }

        static void Normal()
        {
            var (code, sec) = Gen();
            Console.WriteLine($"code:{code}, remaining:{sec}s");

            var res = Check(code);
            Console.WriteLine(res);

            // after remaining seconds, code should not be valid
            Thread.Sleep(sec * 1000);

            res = Check(code);
            Console.WriteLine(res);

            (string code, int sec) Gen()
            {
                var totp = new Totp(secretKey: secretKey, mode: OtpHashMode.Sha512);
                var code = totp.ComputeTotp();
                var sec = totp.RemainingSeconds();
                return (code, sec);
            }

            bool Check(string code)
            {
                var totp = new Totp(secretKey: secretKey, mode: OtpHashMode.Sha512);
                return totp.VerifyTotp(code, out _);
            }
        }
        static void FixTime()
        {
            // sometimes, client's time is not correct, less or more than server's time
            // for example, client's time is 1 day before server's time
            var tc = new TimeCorrection(DateTime.UtcNow.AddDays(-1));

            var (code, sec) = Gen(tc);
            Console.WriteLine($"code:{code}, remaining:{sec}s");

            // the check without time correction should not be valid
            var res = Check(code);
            Console.WriteLine(res);

            // the check with time correction should be valid
            res = Check(code, tc);
            Console.WriteLine(res);

            (string code, int sec) Gen(TimeCorrection? tc = null)
            {
                var totp = new Totp(secretKey: secretKey, mode: OtpHashMode.Sha512, timeCorrection: tc);
                var code = totp.ComputeTotp();
                var sec = totp.RemainingSeconds();
                return (code, sec);
            }

            bool Check(string code, TimeCorrection? tc = null)
            {
                var totp = new Totp(secretKey: secretKey, mode: OtpHashMode.Sha512, timeCorrection: tc);
                return totp.VerifyTotp(code, out _);
            }
        }
        static void Window()
        {
            // current
            var (codeNow, secNow) = Gen(DateTime.UtcNow);
            Console.WriteLine($"code:{codeNow}, remaining:{secNow}s");
            var resNow = Check(codeNow);
            Console.WriteLine(resNow);

            // previous 15 seconds, in 2x previous window
            var (codeP1, secP1) = Gen(DateTime.UtcNow.AddSeconds(-15));
            Console.WriteLine($"code p1:{codeP1}, remaining p1:{secP1}s");
            var resP1 = Check(codeP1);
            Console.WriteLine(resP1);

            // previous 45 seconds, in 2x previous window
            var (codeP2, secP2) = Gen(DateTime.UtcNow.AddSeconds(-45));
            Console.WriteLine($"code p2:{codeP2}, remaining p2:{secP2}s");
            var resP2 = Check(codeP2);
            Console.WriteLine(resP2);

            // previous 90 seconds, over 2x previous window, should not be valid
            var (codeP3, secP3) = Gen(DateTime.UtcNow.AddSeconds(-90));
            Console.WriteLine($"code p3:{codeP3}, remaining p3:{secP3}s");
            var resP3 = Check(codeP3);
            Console.WriteLine(resP3);

            // future 15 seconds, in 2x future window
            var (codeF1, secF1) = Gen(DateTime.UtcNow.AddSeconds(15));
            Console.WriteLine($"code f1:{codeF1}, remaining f1:{secF1}s");
            var resF1 = Check(codeF1);
            Console.WriteLine(resF1);

            // future 45 seconds, in 2x future window
            var (codeF2, secF2) = Gen(DateTime.UtcNow.AddSeconds(45));
            Console.WriteLine($"code f2:{codeF2}, remaining f2:{secF2}s");
            var resF2 = Check(codeF2);
            Console.WriteLine(resF2);

            // future 90 seconds, over 2x future window, should not be valid
            var (codeF3, secF3) = Gen(DateTime.UtcNow.AddSeconds(90));
            Console.WriteLine($"code f3:{codeF3}, remaining f#:{secF3}s");
            var resF3 = Check(codeF3);
            Console.WriteLine(resF3);

            (string code, int sec) Gen(DateTime dt)
            {
                var totp = new Totp(secretKey: secretKey, mode: OtpHashMode.Sha512);
                var code = totp.ComputeTotp(dt);
                var sec = totp.RemainingSeconds();
                return (code, sec);
            }

            bool Check(string code)
            {
                var totp = new Totp(secretKey: secretKey, mode: OtpHashMode.Sha512);
                return totp.VerifyTotp(code, out _, new VerificationWindow(previous: 2, future: 2));
            }
        }
        static void OnlyOnce()
        {
            // cache, persist
            HashSet<long> stepMatchedSet = [];

            var (code, sec) = Gen();
            Console.WriteLine($"code:{code}, remaining:{sec}s");

            // first time
            var res = Check(code);
            Console.WriteLine(res);

            // second time, should not be valid
            res = Check(code);
            Console.WriteLine(res);

            (string code, int sec) Gen()
            {
                var totp = new Totp(secretKey: secretKey, mode: OtpHashMode.Sha512);
                var code = totp.ComputeTotp();
                var sec = totp.RemainingSeconds();
                return (code, sec);
            }

            bool Check(string code)
            {
                var totp = new Totp(secretKey: secretKey, mode: OtpHashMode.Sha512);
                var flag = totp.VerifyTotp(code, out long timeStepMatched);

                // only once
                if (!stepMatchedSet.Add(timeStepMatched))
                {
                    flag = false;
                }

                return flag;
            }
        }
        static void Size()
        {
            // 0 < topSize  <= 10
            var totp = new Totp(secretKey: secretKey, mode: OtpHashMode.Sha512, totpSize: 10);
            var code = totp.ComputeTotp();
            var sec = totp.RemainingSeconds();

            Console.WriteLine($"code:{code}, remaining:{sec}s");
        }
    }
}
