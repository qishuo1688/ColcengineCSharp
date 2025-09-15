# ColcengineCSharp

```csharp
//chat
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

string res = await client.ChatAsync(obj, "你的key");



```



```csharp
//生成语音
var options = new AudioOptions
{
    AppId = Environment.GetEnvironmentVariable("DouBaoAppid"),
    AccessToken = Environment.GetEnvironmentVariable("DouBaoAccessToken"),
    VoiceType = "zh_female_maomao_conversation_wvae_bigtts",
    EmotionScale = "5",
    SpeedRatio = "1.2",
    Text =
        "在这个世界上，有一种感情不需要轰轰烈烈的誓言，却能在平凡的日子里温暖彼此的心，那就是夫妻之间的默契与陪伴。你在我最脆弱的时候握住我的手，不用言语，我就知道你会一直在身边；我在你疲惫的夜晚为你准备一杯温热的茶，只为让你感受到家的温度。岁月匆匆，我们经历了欢笑与泪水，争吵与理解，但每一次的争执过后，依旧愿意彼此拥抱、彼此守护。"

};

```

