using IVA.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;

public class Utility
{
    public static int GenerateRandomNumber()
    {
        Random r = new Random();
        int rnd = r.Next(1000, 10000);
        return rnd;
    }    

    public static async Task SendCode(string Phone, string Code)
    {
        var sendCodeText = ConfigurationManager.AppSettings["SendCodeText"];
        string message = sendCodeText + Code;

        var sendUsingBell = Convert.ToBoolean(ConfigurationManager.AppSettings[Constant.ConfigurationKeys.SendUsingBell]);

        if (sendUsingBell)
        {
            await SendMessageBellAsync(Phone, message);
        }
        else
        {
            await SendMessage(Phone, message);
        }
    }

    [Obsolete("Don't use this, Instead use bell or dialog methods")]
    public static async Task SendMessage(string Phone, string Message)
    {
        var smsGateway = ConfigurationManager.AppSettings[Constant.ConfigurationKeys.SMS_GatewayURL];
        var authCode = ConfigurationManager.AppSettings[Constant.ConfigurationKeys.SMS_GatewayAuthCode];

        using (var stringContent = new StringContent("destination=" + Phone + "&q=" + authCode + "&message=" + Message,
                                                        System.Text.Encoding.UTF8, "application/x-www-form-urlencoded"))
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.PostAsync(smsGateway, stringContent);
                    var result = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                }
            }
        }
    }

    public static async Task<string> GetToken(string UserName, string Password)
    {
        //var tokenUrl = HttpContext.Current.Request.ServerVariables["HTTP_HOST"] + "/Token";
        var tokenUrl = System.Configuration.ConfigurationManager.AppSettings[Constant.ConfigurationKeys.Token_Url];
        var token = "";

        using (var stringContent = new StringContent("username=" + UserName + "&password=" + Password + "&grant_type=password",
                                                        System.Text.Encoding.UTF8, "application/x-www-form-urlencoded"))
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.PostAsync(tokenUrl, stringContent);
                    var result = await response.Content.ReadAsStringAsync();
                    dynamic tokenObj = JsonConvert.DeserializeObject<object>(result);
                    token = tokenObj.access_token;

                }
                catch (Exception ex)
                {
                }
            }
        }

        return token;        
    }

    public static void SendEmail(string subject, string body)
    {
        var adminEmail = ConfigurationManager.AppSettings["AdminEmail"];
        var ccEmail = ConfigurationManager.AppSettings["CCEmail"];
        var solkEmail = ConfigurationManager.AppSettings["SolkEmail"];

        MailMessage mail = new MailMessage(solkEmail, adminEmail);
        mail.CC.Add(ccEmail);
        SmtpClient client = new SmtpClient("smtp.ipage.com", 587);
        client.Credentials = new NetworkCredential("noreply-solk@ivatechnology.com", "^kjN[K8E");
        client.EnableSsl = true;
        client.DeliveryMethod = SmtpDeliveryMethod.Network;
        mail.Subject = subject;
        mail.Body = body;
        client.Send(mail);
    }

    private static async Task SendMessageDialogAsync(string Phone, string Message)
    {
        var smsGateway = ConfigurationManager.AppSettings[Constant.ConfigurationKeys.SMS_GatewayURL];
        var authCode = ConfigurationManager.AppSettings[Constant.ConfigurationKeys.SMS_GatewayAuthCode];

        //dialog service
        using (var stringContent = new StringContent("destination=" + Phone + "&q=" + authCode + "&message=" + Message,
                                                        Encoding.UTF8, "application/x-www-form-urlencoded"))
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.PostAsync(smsGateway, stringContent);
                    var result = await response.Content.ReadAsStringAsync();

                    if (result == "0")
                    {
                        LogInfo(Phone + " Sent at " + DateTime.Now.ToString("yyyy-dd-M HH-mm-ss"));
                    }
                    else
                    {
                        LogInfo(Phone + " Failed at " + DateTime.Now.ToString("yyyy-dd-M HH-mm-ss") + " " + result);
                    }
                }
                catch (Exception ex)
                {
                    LogInfo(Phone + " Failed at " + DateTime.Now.ToString("yyyy-dd-M HH-mm-ss") + " - Exception " + ex.Message);
                }
            }
        }

    }

    private static async Task SendMessageBellAsync(string Phone, string Message)
    {
        var BellSMSURL = ConfigurationManager.AppSettings[Constant.ConfigurationKeys.BellSMSURL];
        var BellSMSCompanyId = ConfigurationManager.AppSettings[Constant.ConfigurationKeys.BellSMSCompanyId];
        var BellSMSPassword = ConfigurationManager.AppSettings[Constant.ConfigurationKeys.BellSMSPassword];

        //http://119.235.1.63:4050/Sms.svc/SendSms?phoneNumber=[phoneNumber]&smsMessage=[smsMessage]&companyId=[companyId]&pword=[pword]
        var smsCommand = string.Concat(BellSMSURL, "?phoneNumber=", Phone, "&smsMessage=", Message, "&companyId=", BellSMSCompanyId, "&pword=", BellSMSPassword);

        using (var client = new HttpClient())
        {
            try
            {
                var response = await client.GetAsync(smsCommand);
                var result = await response.Content.ReadAsStringAsync();

                JObject joResponse = JObject.Parse(result);

                string responseCode = joResponse["Status"].ToString();
                string responseData = joResponse["Data"].ToString();
                string responseId = joResponse["ID"].ToString();

                if (responseCode == "200")
                {
                    LogInfo(Phone + " Sent at " + DateTime.Now.ToString("yyyy-dd-M HH-mm-ss") + " ID : " + responseId);
                }
                else
                {
                    LogInfo(Phone + " Failed at " + DateTime.Now.ToString("yyyy-dd-M HH-mm-ss") + " " + responseData + " ID : " + responseId);
                }
            }
            catch (Exception ex)
            {
                LogInfo(Phone + " Failed at " + DateTime.Now.ToString("yyyy-dd-M HH-mm-ss") + " - Exception " + ex.Message);
            }
        }
    }

    private static void LogInfo(string logText)
    {
        var logFilePath = ConfigurationManager.AppSettings[Constant.ConfigurationKeys.LOG_FILE_PATH];

        string dirPath = Path.GetDirectoryName(logFilePath);

        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);

        using (StreamWriter streamWriter = new StreamWriter(logFilePath, true))
            streamWriter.WriteLine(logText);
    }
}


public class DynamicEntity : DynamicObject
{
    private IDictionary<string, object> _values;

    public DynamicEntity(IDictionary<string, object> values)
    {
        _values = values;
    }
    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        if (_values.ContainsKey(binder.Name))
        {
            result = _values[binder.Name];
            return true;
        }
        result = null;
        return false;
    }

    /*
    usage:
    var values = new Dictionary<string, object>();
    values.Add("Title", "Hello World!");
    values.Add("Text", "My first post");
    values.Add("Tags", new[] { "hello", "world" });

    var post = new DynamicEntity(values);

    dynamic dynPost = post;
    var text = dynPost.Text;
     */
}