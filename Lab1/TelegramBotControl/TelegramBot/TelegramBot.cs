using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace TelegramBotControl
{
    public class TelegramBot
    {
        private string Token { get; set; }
        private HttpClient client { get; set; }
        private string baseUri { get; set; }
        private long nextUpdateId { get; set; }
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
            string response = null;
            try
            {
                response = this.Post(httpMethod, command, null);
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
                result.Exception = new Exception($"CheckStatus failed. Data returned from telegram: {response}", ex);
            }
            return result;
        }

        public SentMessageStatus SendMessage(string UserGroupOrChannel, string message)
        {
            string command = "sendMessage";
            string httpMethod = "POST";

            SentMessageStatus result = new SentMessageStatus();
            if (!this.privateChatIdsByUsername.ContainsKey(UserGroupOrChannel))
            {
                result.Exception = new Exception("Can not fing chat with user/group/channel. Firstly user/group/channel should write something to bot");
                return result;
            }
            var args = new Dictionary<string, string>();
            args["chat_id"] = this.privateChatIdsByUsername[UserGroupOrChannel];
            args["text"] = message;

            string response = null;
            try
            {
                response = this.Post(httpMethod, command, args);
                var reader = new JsonTextReader(new System.IO.StringReader(response));
                while (reader.Read())
                {
                    if (reader.Path.ToLower() == "ok" && reader.Read())
                    {
                        result.Ok = bool.Parse(reader.Value.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                result.Exception = new Exception($"SendMessage failed. Data returned from telegram: {response}", ex);
            }
            return result;
        }

        public Updates GetUpdates()
        {
            string command = "getUpdates";
            string httpMethod = "POST";
            var args = new Dictionary<string, string>();
            args["offset"] = this.nextUpdateId.ToString();

            Updates result = new Updates();

            string response = null;
            try
            {
                response = this.Post(httpMethod, command, args);
                ReadUpdates(result.IncomeMessages, response);
            }
            catch (Exception ex)
            {
                result.Exception = new Exception($"GetUpdates failed. Data returned from telegram: {response}", ex);
            }
            return result;
        }

        private string Post(string httpMethod, string command, Dictionary<string, string> parameters)
        {
            string uri = this.baseUri + "/" + command;

            var request = new HttpRequestMessage();
            request.Method = new HttpMethod(httpMethod);
            request.RequestUri = new Uri(uri);

            if (parameters != null)
            {
                request.Content = new FormUrlEncodedContent(parameters);
            }

            string response;
            using (this.client = new HttpClient())
            {
                response = client.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
            }
            return response;
        }

        private void ReadUpdates(List<IncomeMessage> result, string response)
        {
            var reader = new JsonTextReader(new System.IO.StringReader(response));
            while (reader.Read())
            {
                if (reader.Path.ToLower() == "ok" && reader.Read())
                {
                    bool ok = bool.Parse(reader.Value.ToString());
                    if (!ok) throw new Exception($"Getting updates failed. Data returned from telegram: {response}");
                }

                if (reader.Path.ToLower() == "result" && reader.TokenType == JsonToken.StartArray)
                {
                    while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                    {
                        var message = ReadMessage(reader);
                        result.Add(message);
                    }
                }
            }
        }

        private IncomeMessage ReadMessage(JsonTextReader reader)
        {
            var message = new IncomeMessage();
            string path = reader.Path;
            bool startObject = reader.TokenType == JsonToken.StartObject;
            if (!startObject) throw new Exception("Cannot read income message");

            while (reader.Read())
            {
                if (reader.Path.ToLower() == $"{path}.update_id" && reader.Read())
                {
                    this.nextUpdateId = Math.Max(this.nextUpdateId, long.Parse(reader.Value.ToString()) + 1);
                }

                if (reader.Path.ToLower() == $"{path}.channel_post" && reader.Read())
                {
                    ParseChannelMessage(reader, message);
                }

                if (reader.Path.ToLower() == $"{path}.message" && reader.Read())
                {
                    ParseUserMessage(reader, message);
                }

                if (reader.Path.ToLower() == path && reader.TokenType == JsonToken.EndObject)
                {
                    break;
                }
            }

            if (message.GroupTitle != null && message.ChatId != null)
            {
                this.privateChatIdsByUsername[message.GroupTitle] = message.ChatId;
            } else if (message.UserName != null && message.ChatId != null)
            {
                this.privateChatIdsByUsername[message.UserName] = message.ChatId;
            }
            if (message.ChannelTitle != null && message.ChatId != null)
            {
                this.privateChatIdsByUsername[message.ChannelTitle] = message.ChatId;
            }
            return message;
        }

        private void ParseChannelMessage(JsonTextReader reader, IncomeMessage message)
        {
            string path = reader.Path;
            bool startObject = reader.TokenType == JsonToken.StartObject;
            if (!startObject) throw new Exception("Cannot read income message");

            while (reader.Read())
            {
                if (reader.Path.ToLower() == $"{path}.chat.id" && reader.Read())
                {
                    message.ChatId = reader.Value.ToString();
                }

                if (reader.Path.ToLower() == $"{path}.chat.type" && reader.Read())
                {
                    message.SourceType = reader.Value.ToString() == "channel" ? IncomeMessageSource.ChannelPost : IncomeMessageSource.Unknown;
                }

                if (reader.Path.ToLower() == $"{path}.chat.title" && reader.Read())
                {
                    message.ChannelTitle = reader.Value.ToString();
                }

                if (reader.Path.ToLower() == $"{path}.text" && reader.Read())
                {
                    message.Text = reader.Value.ToString();
                }

                if (reader.Path.ToLower() == path && reader.TokenType == JsonToken.EndObject)
                {
                    break;
                }
            }
        }

        private void ParseUserMessage(JsonTextReader reader, IncomeMessage message)
        {
            string path = reader.Path;
            bool startObject = reader.TokenType == JsonToken.StartObject;
            if (!startObject) throw new Exception("Cannot read income message");

            while (reader.Read())
            {
                if (reader.Path.ToLower() == $"{path}.chat.id" && reader.Read())
                {
                    message.ChatId = reader.Value.ToString();
                }

                if (reader.Path.ToLower() == $"{path}.chat.type" && reader.Read())
                {
                    message.SourceType = reader.Value.ToString() == "private" ? IncomeMessageSource.PrivateMessage :
                         reader.Value.ToString() == "group" ? IncomeMessageSource.GroupMessage : IncomeMessageSource.Unknown;
                }

                if (reader.Path.ToLower() == $"{path}.chat.title" && reader.Read())
                {
                    message.GroupTitle = reader.Value.ToString();
                }

                if (reader.Path.ToLower() == $"{path}.from.username" && reader.Read())
                {
                    message.UserName = reader.Value.ToString();
                }

                if (reader.Path.ToLower() == $"{path}.text" && reader.Read())
                {
                    message.Text = reader.Value.ToString();
                }

                if (reader.Path.ToLower() == path && reader.TokenType == JsonToken.EndObject)
                {
                    break;
                }
            }
        }
    }
}

