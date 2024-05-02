using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody]LoginDto dto)
        {
            if (dto != null && dto.Username == "admin" && dto.Password == "admin")
            {
                return Ok(new 
                {
                    user_id = Guid.NewGuid().ToString("N"),
                    token = Guid.NewGuid().ToString("N")
                });
            }
            else
            {
                return Unauthorized("Login Failed");
            }
        }

        private const string CognitoIdentityPoolId = "us-west-2:xxxxxxx";
        private const string CognitoIdentityProviderName = "myprovider";
        private const string IotCoreEndpoint = "xxxxxx.iot.us-west-2.amazonaws.com";

        [HttpGet("getIot")]
        public async Task<IActionResult> GetIotCoreInfo([FromHeader(Name = "B-Token")] string token)
        {
            if(string.IsNullOrWhiteSpace(token))
            {
                return Unauthorized("Token is required");
            }

            // will read ak/sk from env
            Amazon.CognitoIdentity.AmazonCognitoIdentityClient client = new Amazon.CognitoIdentity.AmazonCognitoIdentityClient();

           var resp = await client.GetOpenIdTokenForDeveloperIdentityAsync(new Amazon.CognitoIdentity.Model.GetOpenIdTokenForDeveloperIdentityRequest
            {
                IdentityPoolId = CognitoIdentityPoolId,
                Logins = new Dictionary<string, string>
                {
                    { CognitoIdentityProviderName, token }
                }
            });

            // will read ak/sk from env
            Amazon.IoT.AmazonIoTClient iotClient = new Amazon.IoT.AmazonIoTClient();

            var iotPolicy = @"{
    ""Version"": ""2012-10-17"",
    ""Statement"": [
        {
            ""Effect"": ""Allow"",
            ""Action"": [
                ""iot:Publish"",
                ""iot:Subscribe"",
                ""iot:Receive""
            ],
            ""Resource"": [""arn:aws:iot:*:*:topic/user/${cognito-identity.amazonaws.com:sub}""]
        },
        {
            ""Effect"": ""Allow"",
            ""Action"": [
                ""iot:Connect""
            ],
            ""Resource"": [""arn:aws:iot:*:*:client/${cognito-identity.amazonaws.com:sub}""]
        }
    ]
}";

            _ =  await iotClient.CreatePolicyAsync(new Amazon.IoT.Model.CreatePolicyRequest
            {
                PolicyName = "CognitoIotCorePolicy",
                PolicyDocument = iotPolicy
            });


            var attachResp = await iotClient.AttachPolicyAsync(new Amazon.IoT.Model.AttachPolicyRequest
            {
                PolicyName = "CognitoIotCorePolicy",
                Target = resp.IdentityId
            });


            return Ok(new 
            {
                iot_endpoint = IotCoreEndpoint,
                client_id = resp.IdentityId,
            });

        }

        public class LoginDto
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }


}
