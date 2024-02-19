using MQTTnet;
using MQTTnet.Client;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

var cert = CertificateHelper.GetConvertedPfxCert();
//var cert = CertificateHelper.GenPfxCert();

var mqttFactory = new MqttFactory();

List<X509Certificate2> certificates = [cert];

using (var mqttClient = mqttFactory.CreateMqttClient())
{
    var mqttClientOptions = new MqttClientOptionsBuilder()
        .WithTcpServer(ConstValue.AwsIotEndPoint, ConstValue.AwsIotPort)
        .WithClientId(ConstValue.MQTTClientId)
        .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V311)
        .WithTlsOptions(new MqttClientTlsOptionsBuilder()
            .WithAllowUntrustedCertificates(true)
            .WithSslProtocols(SslProtocols.Tls12)
            .UseTls(true)
            .WithIgnoreCertificateChainErrors(true)
            .WithIgnoreCertificateRevocationErrors(true)
            .WithClientCertificates(certificates)
            .Build()
            )
        .Build();

    mqttClient.ConnectingAsync += async e =>
    {
        await Console.Out.WriteLineAsync($"CONNECTTING===={e.ClientOptions.ClientId},{e.ClientOptions.ProtocolVersion}");
    };

    mqttClient.ConnectedAsync += async e =>
    {
        await Console.Out.WriteLineAsync($"CONNECTED====OK");
    };

    mqttClient.DisconnectedAsync += async e =>
    {
        await Console.Out.WriteLineAsync($"DISCONNECTED===={e.Reason}");

        if (e.ClientWasConnected)
        {
            await mqttClient.DisconnectAsync();
        }
    };

    using (var timeout = new CancellationTokenSource(5000))
    {
        await mqttClient.ConnectAsync(mqttClientOptions, timeout.Token);
        Console.WriteLine("The MQTT client is connected.");
    }

    using (var timeout = new CancellationTokenSource(5000))
    {
        await mqttClient.DisconnectAsync(MqttClientDisconnectOptionsReason.NormalDisconnection);
        Console.WriteLine("The MQTT client is disconnected.", timeout.Token);
    }
}

public static class CertificateHelper
{
    public static X509Certificate2 GetConvertedPfxCert()
    {
        // openssl pkcs12 -export -in xxxxx.cert.pem -inkey xxxxx.private.key -out xxxxx.cert.pfx -certfile AmazonRootCA1.crt
        var cert = new X509Certificate2(ConstValue.ConvertedPfxPath, ConstValue.ConvertedPfxPwd);
        return cert;
    }

    public static X509Certificate2 GenPfxCert()
    {
        var pwd = "123456";
        var tmpCert = X509Certificate2.CreateFromPemFile(ConstValue.DeviceCertPemPath, ConstValue.DevicePrivateKeyPath);
        var bytes = tmpCert.Export(X509ContentType.Pkcs12, pwd);
        return new X509Certificate2(bytes, pwd);
    }
}

public static class ConstValue
{
    public static string DeviceCertPemPath = @"/your/path/<xxxxx>.cert.pem";
    public static string DevicePrivateKeyPath = @"/your/path/<xxxxx>.private.key";

    public static string ConvertedPfxPath = @"/your/path/<xxxxx>.cert.pfx";
    public static string ConvertedPfxPwd = @"123456";

    public static string AwsIotEndPoint = @"<yyyyy>.iot.<region>.amazonaws.com";
    public static int AwsIotPort = 8883;

    // Replace with your clientid that meet aws policy("iot:Connect")
    public static string MQTTClientId = @"basicPubSub";
}