using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ColcengineCSharp
{
    public static class Audio
    {
        public static async Task Create(string filePathWithoutExtension, AudioOptions options)
        {
            StringBuilder sb = new StringBuilder();
            using var webSocket = new System.Net.WebSockets.ClientWebSocket();
            var cancellationTokenSource = new CancellationTokenSource();
            webSocket.Options.SetRequestHeader("Authorization", $"Bearer;{options.AccessToken}");
            // webSocket.Options.CollectHttpResponseDetails = true;

            await webSocket.ConnectAsync(new Uri(options.Endpoint), cancellationTokenSource.Token);

            //var responseHeaders = webSocket.HttpResponseHeaders;

            // Prepare request
            var request = new Dictionary<string, object>
            {
                ["app"] = new Dictionary<string, object>
                {
                    ["appid"] = options.AppId,
                    ["token"] = options.AccessToken,
                    ["cluster"] = options.GetCluster()
                },
                ["user"] = new Dictionary<string, object>
                {
                    ["uid"] = Guid.NewGuid().ToString()
                },
                ["audio"] = new Dictionary<string, object>
                {
                    ["voice_type"] = options.VoiceType,
                    ["encoding"] = options.AudioEncoding,
                    ["speed_ratio"] = options.SpeedRatio,
                    ["emotion"] = options.Emotion,
                    ["enable_emotion"] = options.EnableEmotion,
                    ["emotion_scale"] = options.EmotionScale
                },

                ["request"] = new Dictionary<string, object>
                {
                    ["reqid"] = Guid.NewGuid().ToString(),
                    ["text"] = options.Text,
                    ["operation"] = "query",
                    ["with_timestamp"] = "1",

                    ["extra_param"] = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object>
                    {
                        ["disable_markdown_filter"] = false,
                    })
                }
            };

            // Send text request
            await ClientHelper.FullClientRequest(webSocket, System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(request), cancellationTokenSource.Token);

            // Receive audio data
            var audio = new List<byte>();
            while (true)
            {
                var message = await ClientHelper.ReceiveMessage(webSocket, cancellationTokenSource.Token);

                switch (message.MsgType)
                {
                    case MsgType.FrontEndResultServer:
                        if (message.Payload != null && message.Payload.Length > 0)
                        {
                            sb.Append(message);
                        }
                        break;
                    case MsgType.AudioOnlyServer:
                        if (message.Payload != null && message.Payload.Length > 0)
                        {
                            audio.AddRange(message.Payload);
                        }
                        break;
                    default:
                        throw new Exception($"{message}");
                }

                if (message.MsgType == MsgType.AudioOnlyServer && message.Sequence < 0)
                {
                    break;
                }
            }

            if (audio.Count == 0)
            {
                throw new Exception("Audio is empty");
            }

            string filename = $"{filePathWithoutExtension}.{options.AudioEncoding}";
            File.WriteAllBytes(filename, audio.ToArray());
            cancellationTokenSource.Cancel();

            string text = sb.ToString();
            int payloadIndex = text.IndexOf("Payload:");
            if (payloadIndex < 0)
            {
                throw new Exception("No Payload found in the response");
            }
            string payloadStr = text.Substring(payloadIndex + "Payload:".Length).Trim();

            // 先反序列化成动态对象
            var tmp = JObject.Parse(payloadStr);
            tmp["frontend"] = JObject.Parse(tmp["frontend"].ToString());
            File.WriteAllText($"{filePathWithoutExtension}.json", tmp.ToString(Formatting.Indented));



        }
    }


    public static class ColcengineExtensions
    {
        /// <summary>
        /// 调用大模型
        /// </summary>
        /// <param name="client"></param>
        /// <param name="obj"> 模型参数:
        /// <remarks>
        ///  示例：var obj = new 
        ///  {
        ///     model = "doubao-1-5-pro-32k-250115",
        ///     messages = new[]
        ///     {
        ///         new { role = "system", content = "你是一个助手" },
        ///     new { role = "user", content = "你是谁?" }
        ///     }
        ///  }
        /// </remarks>
        /// </param>
        /// <param name="apiKey">apiKey</param>
        /// <returns>大模型返回的消息</returns>
        public static async Task<string> ChatAsync(this HttpClient client, object obj, string apiKey)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://ark.cn-beijing.volces.com/api/v3/chat/completions");
            request.Headers.Add("Authorization", $"Bearer {apiKey}");

            request.Content = new StringContent(JsonConvert.SerializeObject(obj, Formatting.Indented));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            var jObj = JObject.Parse(responseBody);
            return (string)jObj["choices"]?[0]?["message"]?["content"];

        }


        /// <summary>
        /// 生成图片
        /// </summary>
        /// <param name="client"></param>
        /// <param name="obj"> 模型参数:
        /// <remarks>
        ///  示例：var obj = new
        ///  {
        ///      model = "doubao-seedream-3-0-t2i-250415",
        ///      prompt = "星际穿越，黑洞，黑洞里冲出一辆快支离破碎的复古列车，抢视觉冲击力，电影大片",
        ///      response_format = "b64_json",
        ///      size = "1024x1024",
        ///      watermark = false
        ///  };
        /// </remarks>
        /// </param>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public static async Task<string> ImageGenerateAsync(this HttpClient client, object obj, string apiKey)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://ark.cn-beijing.volces.com/api/v3/images/generations");
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            request.Content = new StringContent(json);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            var jObj = JObject.Parse(responseBody);

            var jFormat = JObject.Parse(json)["response_format"] ?? "url";
            return (string)jObj["data"]?[0]?[jFormat.ToString()];

        }
        /// <summary>
        /// 生成视频
        /// </summary>
        /// <param name="obj"> 模型参数:</param>
        /// <remarks>
        ///  示例：var obj = new
        ///  {
        ///      model = "doubao-seedance-1-0-pro-250528",
        ///      content = new[]
        ///      {
        ///          new {type="text",text = "多个镜头。一名侦探进入一间光线昏暗的房间。 --ratio 16:9"}
        ///      }
        /// };
        /// </remarks>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public static async Task<string> VideoTaskCreateAsync(this HttpClient client, object obj, string apiKey)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://ark.cn-beijing.volces.com/api/v3/contents/generations/tasks");
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            request.Content = new StringContent(json);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            var jObj = JObject.Parse(responseBody);

            return (string)jObj["id"];

        }


        /// <summary>
        /// 查询视频查询任务
        /// </summary>
        /// <param name="videoId"></param>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public static async Task<VideoTaskResult> CheckVideoTask(this HttpClient client, string videoId, string apiKey)
        {
            var result = new VideoTaskResult();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://ark.cn-beijing.volces.com/api/v3/contents/generations/tasks/{videoId}");

            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            var jObj = JObject.Parse(responseBody);

            switch (jObj["status"]?.ToString())
            {
                case "succeeded":
                    result.IsComplete = true;
                    result.Message = jObj["content"]?["video_url"]?.ToString();
                    break;
                case "queued":
                case "running":
                    result.IsComplete = false;
                    break;
                case "failed":
                    result.IsComplete = true;
                    result.Message = jObj["error"]?["message"]?.ToString();
                    break;
            }
            return result;
        }

    }



    public class VideoTaskResult
    {
        public bool IsComplete { set; get; }
        public string Message { set; get; }
    }
}
