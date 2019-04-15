using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft;
using Newtonsoft.Json;

namespace TelegramBotControl
{
    class Program
    {
        static void Main(string[] args)
        {
            string botToken = "547209759:AAE3BX47EAeydupkaTOHPadG9wixml23ChA";
            TelegramBot TB = new TelegramBot(botToken);
            var status = TB.CheckStatus();
            var messages = TB.GetUpdates();
            messages = TB.GetUpdates();
            messages = TB.GetUpdates();
            Console.WriteLine($"You are controlling bot: {status.BotUsetname}");
            Console.ReadLine();
        }
    }

    public class TelegramBot
    {
        private string Token { get; set; }
        private HttpClient client { get; set; }
        private string baseUri { get; set; }
        private long currentUpdateId { get; set; }
        private Dictionary<string, string> privateChatIdsByUsername { get; set; }

        public TelegramBot(string token)
        {
            this.Token = token;
            this.baseUri = "https://api.telegram.org/bot" + token;
            this.privateChatIdsByUsername = new Dictionary<string, string>();
        }

        public CheckStatus CheckStatus()
        {
            string command = "getme";
            string httpMethod = "GET";

            CheckStatus result = new CheckStatus();

            var response = this.Post(httpMethod, command, null);
            try
            {
                var reader = new JsonTextReader(new System.IO.StringReader(response));
                while (reader.Read())
                {
                    if (reader.Path.ToLower() == "ok" && reader.Read())
                    {
                        result.Ok = bool.Parse(reader.Value.ToString());
                    }

                    if (reader.Path.ToLower() == "result.username" && reader.Read())
                    {
                        result.BotUsetname = reader.Value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                result.Exception = new Exception($"Something is going wrong. Data returned from telegram: {response}", ex);
            }
            return result;
        }

        public void SendMessage(string userName, string message)
        {

        }

        public List<IncomeMessage> GetUpdates()
        {
            string command = "getUpdates";
            string httpMethod = "POST";
            if (this.currentUpdateId != 0) this.currentUpdateId++;
            var args = new Dictionary<string, string>();
            args["update_id"] = currentUpdateId.ToString();

            List<IncomeMessage> result = new List<IncomeMessage>();

            var response = this.Post(httpMethod, command, args);
            try
            {
                var reader = new JsonTextReader(new System.IO.StringReader(response));
                while (reader.Read())
                {
                    if(reader.Path.ToLower() == "result" && reader.TokenType == JsonToken.StartArray)
                    {
                        while(reader.Read() && reader.TokenType != JsonToken.EndArray)
                        {
                            var message = ReadMessage(reader);
                            result.Add(message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Something is going wrong. Data returned from telegram: {response}", ex);
            }
            return result;
        }


        private string Post(string httpMethod, string command, object parameters)
        {
            string uri = this.baseUri + "/" + command;
            var request = new HttpRequestMessage();
            request.Method = new HttpMethod(httpMethod);
            request.RequestUri = new Uri(uri);

            string response;
            using (this.client = new HttpClient())
            {
                response = client.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
            }
            return response;
        }

        private IncomeMessage ReadMessage(JsonTextReader reader)
        {
            var message = new IncomeMessage();
            string UserName = null;
            string ChatId = null;
            string path = reader.Path;
            bool startObject = reader.TokenType == JsonToken.StartObject;
            if (!startObject) throw new Exception("Cannot read income message");

            while (reader.Read())
            {
                if (reader.Path.ToLower() == $"{path}.update_id" && reader.Read())
                {
                    this.currentUpdateId = Math.Max(this.currentUpdateId, long.Parse(reader.Value.ToString()));
                }

                if (reader.Path.ToLower() == $"{path}.message.chat.username" && reader.Read())
                {
                    UserName = reader.Value.ToString();
                    message.UserName = UserName;
                }

                if (reader.Path.ToLower() == $"{path}.message.chat.id" && reader.Read())
                {
                    ChatId = reader.Value.ToString();
                    message.ChatId = ChatId;
                }

                if (reader.Path.ToLower() == $"{path}.message.text" && reader.Read())
                {
                    message.Text = reader.Value.ToString();
                }

                if (reader.Path.ToLower() == path && reader.TokenType == JsonToken.EndObject)
                {
                    break;
                }
            }

            if (UserName != null && ChatId != null)
            {
                this.privateChatIdsByUsername[UserName] = ChatId;
            }
            return message;
        }
    }

    public class CheckStatus
    {
        public bool Ok { get; set; }
        public string BotUsetname { get; set; }
        public Exception Exception { get; set; }
    }

    public class IncomeMessage
    {
        public string UserName { get; set; }
        public string ChatId { get; set; }
        public string Text { get; set; }
    }
}
