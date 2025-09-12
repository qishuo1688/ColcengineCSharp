using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ColcengineCSharp
{
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
        public static async Task<string> ChatAsync(this HttpClient client, object obj,string apiKey)
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

            var jFormat = JObject.Parse(json)["response_format"]??"url";
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
                    break ;
                case "queued":
                case "running":
                    result.IsComplete = false;
                    break;
                case "failed":
                    result.IsComplete = true;
                    result.Message = jObj["error"]?["message"]?.ToString();
                    break ;
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
