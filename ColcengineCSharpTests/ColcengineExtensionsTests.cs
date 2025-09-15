using Xunit;

namespace ColcengineCSharp.Tests
{
    public class ColcengineExtensionsTests
    {
       
        [Fact()]
        public async Task ChatAsyncTestAsync()
        {
            var obj = new
            {
                model = "doubao-1-5-pro-32k-2501151",
                messages = new[]
                {
                    new { role = "system", content = "你是一个助手" },
                    new { role = "user", content = "你是谁?" }
                }
            };
            try
            {


                using HttpClient client = new HttpClient();

                string res = await client.ChatAsync(obj, Environment.GetEnvironmentVariable("DouBaoAPIkey"));
                Xunit.Assert.NotNull(res);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }



        }

        [Fact()]
        public async Task ImageGenerateAsyncTestAsync()
        {
            var obj = new
            {
                model = "doubao-seedream-3-0-t2i-250415",
                prompt = "星际穿越，黑洞，黑洞里冲出一辆快支离破碎的复古列车，抢视觉冲击力，电影大片",
                response_format = "b64_json",
                size = "1024x1024",
                watermark = false
            };

            using HttpClient client = new HttpClient();

            var url = await client.ImageGenerateAsync(obj,Environment.GetEnvironmentVariable("DouBaoAPIkey"));

            Xunit.Assert.NotNull(url);
        }

        [Fact()]
        public async Task VideoTaskCreateAsyncTest()
        {

            var obj = new
            {
                model = "doubao-seedance-1-0-pro-250528",
                content = new[]
                {
                    new {type="text",text = "多个镜头。一名侦探进入一间光线昏暗的房间。他检查桌上的线索，手里拿起桌上的某个物品。镜头转向他正在思索。 --ratio 16:9"}
                }
            };
            using HttpClient client = new HttpClient();
            var id = await client.VideoTaskCreateAsync(obj,Environment.GetEnvironmentVariable("DouBaoAPIkey"));
            Xunit.Assert.NotNull(id);
        }

        [Fact()]
        public async Task CheckVideoTaskTestAsync()
        {
            using HttpClient client = new HttpClient();
            var id = await client.CheckVideoTask("cgt-20250912141001-8tq7n1",Environment.GetEnvironmentVariable("DouBaoAPIkey"));

            Xunit.Assert.NotNull(id);
        }
    }
}