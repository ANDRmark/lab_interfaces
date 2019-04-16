using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;

namespace TelegramBotControl
{
    public class TelegramBot
    {
        private string Token { get; set; }
        private HttpClient client { get; set; }
        private string baseUri { get; set; }
        private long nextUpdateId { get; set; }
        private ChatIDs chatIDs { get; set; }
        private string backupChutIDsFileName { get; set; }

        public TelegramBot(string token, string backupChutIDsFileName = null)
        {
            this.Token = token;
            this.baseUri = "https://api.telegram.org/bot" + token;
            this.chatIDs = new ChatIDs();
            this.backupChutIDsFileName = backupChutIDsFileName;
            try
            {
                this.chatIDs = JsonConvert.DeserializeObject<ChatIDs>(System.IO.File.ReadAllText(backupChutIDsFileName));
            }
            catch { }
        }

        public void BackupChatIDs()
        {
            try
            {
                System.IO.File.WriteAllText(backupChutIDsFileName, JsonConvert.SerializeObject(this.chatIDs));
            }
            catch { }
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

        public SentMessageStatus SendMessageToUser(string UserName, string message)
        {
            return SendMessage(OutcomeMessageType.PrivateMessage, UserName, message);
        }

        public SentMessageStatus SendMessageToGroup(string GroupName, string message)
        {
            return SendMessage(OutcomeMessageType.GroupMessage, GroupName, message);
        }

        public SentMessageStatus SendMessageToChannel(string ChannelName, string message)
        {
            return SendMessage(OutcomeMessageType.ChannelPost, ChannelName, message);
        }

        private SentMessageStatus SendMessage(OutcomeMessageType destination, string UserOrGroupOrChannel, string message)
        {
            string command = "sendMessage";
            string httpMethod = "POST";

            SentMessageStatus result = new SentMessageStatus();

            string response = null;
            try
            {
                var args = CreateSendMessageArgs(destination, UserOrGroupOrChannel, message);
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

        private Dictionary<string, string> CreateSendMessageArgs(OutcomeMessageType destination, string UserOrGroupOrChannel, string message)
        {
            var args = new Dictionary<string, string>();
            switch (destination)
            {
                case OutcomeMessageType.PrivateMessage:
                    if (!this.chatIDs.privateChatIdsByUsername.ContainsKey(UserOrGroupOrChannel)) throw new Exception($"Can not fing chat with user/group/channel {UserOrGroupOrChannel}. Firstly user/group/channel should write something to bot");
                    args["chat_id"] = this.chatIDs.privateChatIdsByUsername[UserOrGroupOrChannel];
                    break;
                case OutcomeMessageType.GroupMessage:
                    if (!this.chatIDs.groupChatIdsByGroupName.ContainsKey(UserOrGroupOrChannel)) throw new Exception($"Can not fing chat with user/group/channel {UserOrGroupOrChannel}. Firstly user/group/channel should write something to bot");
                    args["chat_id"] = this.chatIDs.groupChatIdsByGroupName[UserOrGroupOrChannel];
                    break;
                case OutcomeMessageType.ChannelPost:
                    if (!this.chatIDs.channelChatIdsByChannelName.ContainsKey(UserOrGroupOrChannel)) throw new Exception($"Can not fing chat with user/group/channel {UserOrGroupOrChannel}. Firstly user/group/channel should write something to bot");
                    args["chat_id"] = this.chatIDs.channelChatIdsByChannelName[UserOrGroupOrChannel];
                    break;

            }
            args["text"] = message;
            return args;
        }

        public Updates GetUpdates()
        {
            return GetUpdates(CancellationToken.None);
        }

        public Updates GetUpdates(CancellationToken ct)
        {
            string command = "getUpdates";
            string httpMethod = "POST";
            var args = new Dictionary<string, string>();
            args["offset"] = this.nextUpdateId.ToString();

            Updates result = new Updates();

            string response = null;
            try
            {
                response = this.Post(httpMethod, command, args, ct);
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
            return Post(httpMethod, command, parameters, CancellationToken.None);
        }

        private string Post(string httpMethod, string command, Dictionary<string, string> parameters, CancellationToken ct)
        {
            string uri = this.baseUri + "/" + command;

            var request = new HttpRequestMessage();
            request.Method = new HttpMethod(httpMethod);
            request.RequestUri = new Uri(uri);

            if (parameters != null)
            {
                request.Content = new FormUrlEncodedContent(parameters);
            }

            string response = null;
            using (this.client = new HttpClient())
            {
                var task = client.SendAsync(request, ct);
                try
                {
                    response = task.Result.Content.ReadAsStringAsync().Result;
                } catch(AggregateException ex)
                {
                    if (task.IsCanceled)
                    {
                        return null;
                    }
                    throw ex;
                }
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
                this.chatIDs.groupChatIdsByGroupName[message.GroupTitle] = message.ChatId;
            }
            if (message.UserName != null && message.ChatId != null)
            {
                this.chatIDs.privateChatIdsByUsername[message.UserName] = message.ChatId;
            }
            if (message.ChannelTitle != null && message.ChatId != null)
            {
                this.chatIDs.channelChatIdsByChannelName[message.ChannelTitle] = message.ChatId;
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

