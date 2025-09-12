using Xunit;
using ColcengineCSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColcengineCSharp.Tests
{
    public class ColcengineExtensionsTests
    {
        [Fact()]
        public async Task ChatAsyncTestAsync()
        {
            var obj = new
            {
                model = "doubao-1-5-pro-32k-250115",
                messages = new[]
                {
                    new { role = "system", content = "你是一个助手" },
                    new { role = "user", content = "你是谁?" }
                }
            };

            using HttpClient client = new HttpClient();

            string res = await client.ChatAsync(obj, "1ce0a728-5652-43eb-917e-c5fa6fd4e992");


            Xunit.Assert.NotNull(res);

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

            var url = await client.ImageGenerateAsync(obj, "1ce0a728-5652-43eb-917e-c5fa6fd4e992");

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
            var id = await client.VideoTaskCreateAsync(obj, "1ce0a728-5652-43eb-917e-c5fa6fd4e992");
            Xunit.Assert.NotNull(id);
        }

        [Fact()]
        public async Task CheckVideoTaskTestAsync()
        {
            using HttpClient client = new HttpClient();
            var id = await client.CheckVideoTask("cgt-20250912141001-8tq7n1", "1ce0a728-5652-43eb-917e-c5fa6fd4e992");

            Xunit.Assert.NotNull(id);
        }
    }
}