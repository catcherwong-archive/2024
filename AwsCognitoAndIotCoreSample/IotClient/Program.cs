// See https://aka.ms/new-console-template for more information
using MQTTnet.Client;
using MQTTnet;
using Newtonsoft.Json;
using System.Text;

Console.WriteLine("Hello, World!");


// 1. login
HttpClient httpClient = new();
httpClient.BaseAddress = new Uri("http://localhost:7878");

var loginResp = await httpClient.PostAsync("login", new StringContent(JsonConvert.SerializeObject(new { Username = "admin", Password = "admin" }), Encoding.UTF8));
var loginRespContent = await loginResp.Content.ReadAsStringAsync();
var loginRespObj = JsonConvert.DeserializeObject<LoginResponse>(loginRespContent);

// 2. getIot
HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "getIot");
request.Headers.Add("B-Token", loginRespObj.token);
var iotResp = await httpClient.SendAsync(request);
var iotrespContent = await iotResp.Content.ReadAsStringAsync();
var iotRespObj = JsonConvert.DeserializeObject<IotResponse>(iotrespContent);

// 3. connect to mqtt with websocket
var factory = new MqttFactory();
var mqttClient = factory.CreateMqttClient();

var options = new MqttClientOptionsBuilder()
    .WithClientId(iotRespObj.client_id)
    .WithWebSocketServer(x => 
    {
        x.WithUri($"wss://{iotRespObj.iot_endpoint}/mqtt");
    })
    .Build();

await mqttClient.ConnectAsync(options);

await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic($"user/{iotRespObj.client_id}").Build());

await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
       .WithTopic($"user/{iotRespObj.client_id}")
          .WithPayload("Hello World").WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                   .Build());


internal class IotResponse
{
    public string iot_endpoint { get; set; }
    public string client_id { get; set; }

}

public class LoginResponse
{
    public string user_id { get; set; }
    public string token { get; set; }
}