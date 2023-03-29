using ChatAI.Dotnet.Services;
using ChatAI.Repository.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using OpenAI_API.Chat;
using OpenAI_API.Completions;
using OpenAI_API.Models;
using System.Text;
using Yitter.IdGenerator;
using static ChatAI.Dotnet.Services.LiteDBHelper;

namespace ChatAI.Dotnet.Controllers
{
    [Route("api")]
    [ApiController]
    public class APIController : ControllerBase
    {
        private readonly OpenAI_API.OpenAIAPI api;
        private readonly IConfiguration _configuration;
        private readonly OpenAI_API.Models.Model chatgptModel;
        private readonly int _maxTokens = 3500;
        public APIController(OpenAI_API.OpenAIAPI openAIAPI,IConfiguration configuration) {
            api = openAIAPI;
            _configuration = configuration;
            chatgptModel = new Model(_configuration.GetSection("ChaiAI")["Model"]) { OwnedBy = "openai" };
 
        }


        [HttpPost]
        [Route("generate/id")]
        public async Task<JsonResult> Generate()
        {
            return  OK(YitIdHelper.NextId().ToString());
        }

        [HttpPut]
        [Route("ai/suitable/{id}")]
        public async Task<JsonResult> response_suitable(long id, SuitableDTO suitableDTO)
        {
            return await Task<JsonResult>.Run(() =>
            {
                var service = new ConversationService();
                var conversation = service.GetByID(id);
                if (conversation is null || conversation.Id == 0)
                {
                    return ERR("对话不存在");
                }
                var convs = conversation.Convs;

                if (convs is null || convs.Count <= suitableDTO.idx)
                {
                    return ERR("下标有误");
                }
                convs[suitableDTO.idx].Suitable[suitableDTO.msg_idx]=suitableDTO.Suitable;
                service.Replace(conversation);
                return OK(string.Empty);
            }
         );
        }

     
        [HttpGet]
        [Route("chat/repeat/{id}")]
        public async Task chat_repeat(long id)
        {

            HttpContext.Response.Headers.Add("Content-Type", "text/event-stream");
            var service = new ConversationService();
            var conversation = service.GetByID(id);
            if (conversation is null || conversation.Id == 0)
            {
                await HttpContext.Response.WriteAsync($"event: error\n\n");
                await HttpContext.Response.WriteAsync($"data: 对话不存在\n\n");
                await HttpContext.Response.Body.FlushAsync();
                await HttpContext.Response.WriteAsync("data: [DONE]\n\n");
                await HttpContext.Response.Body.FlushAsync();
                HttpContext.Response.Body.Close();
            }
            else
            {
                StringBuilder ai_text = new StringBuilder();
                string content = string.Empty;
                // for example
                await foreach (var token in api.Chat.StreamChatEnumerableAsync(new ChatRequest()
                {
                    Model = chatgptModel,
                    Temperature = 0.8,
                    MaxTokens = _maxTokens,
                    TopP = 1,
                    FrequencyPenalty = 0.5,
                    PresencePenalty = 0,
                    Messages = GetChatMessages(conversation.Convs)
                }))
                {
                    // if (token.Choices[0].Delta.Role == "user")
                    //  {
                    content = token.Choices[0].Delta?.Content ?? "";
                    ai_text.Append(content);
                    content = content.Replace("\n", "[ENTRY]");
                    await HttpContext.Response.WriteAsync($"data: {content}\n\n");
                    await HttpContext.Response.Body.FlushAsync();
                    conversation = service.GetByID(id);
                    if (conversation.StopGenerating)
                        break;
                    // }

                }

                await HttpContext.Response.WriteAsync("data: [DONE]\n\n");
                await HttpContext.Response.Body.FlushAsync();

                HttpContext.Response.Body.Close();

                var lastConvs = conversation.Convs?[conversation.Convs.Count - 1];
                lastConvs?.Speeches?.Add(ai_text.ToString());
                lastConvs?.Suitable?.Add(0);
                service.Replace(conversation);
            }

        }

        [HttpGet]
        [Route("conv/{id}")]
        public JsonResult conv(long id)
        {
            var service = new ConversationService();
            var conversation = service.GetByID(id);
            return OK(conversation);

        }

        [HttpPut]
        [Route("stop/chat/{id}")]
        public JsonResult Stop(long id)
        {
            var service = new ConversationService();
            var conversation = service.GetByID(id);
            conversation.StopGenerating= true;
            service.Save(conversation);
            return OK("");

        }
        [HttpGet]
        [Route("chat/title/{id}")]
        public async Task Title(long id)
        {
            HttpContext.Response.Headers.Add("Content-Type", "text/event-stream");
            var service = new ConversationService();
            var conversation = service.GetByID(id);
            if (conversation is null || conversation.Id == 0)
            {
                await HttpContext.Response.WriteAsync($"event: error\n\n");
                await HttpContext.Response.WriteAsync($"data: 对话不存在\n\n");
                await HttpContext.Response.Body.FlushAsync();
                await HttpContext.Response.WriteAsync("data: [DONE]\n\n");
                await HttpContext.Response.Body.FlushAsync();
                HttpContext.Response.Body.Close();
            }
            else
            {
                conversation.Convs?.Add(new Conv { Speaker = "human", Speech = "为以上对话取一个符合的标题", Createtime = DateTime.Now });

                StringBuilder ai_text = new StringBuilder();
                string content = string.Empty;
                // for example
                await foreach (var token in api.Chat.StreamChatEnumerableAsync(new ChatRequest()
                {
                    Model = chatgptModel,
                    Temperature = 0.8,
                    MaxTokens = 100,
                    TopP = 1,
                    FrequencyPenalty = 0.5,
                    PresencePenalty = 0,
                    Messages = GetChatMessages(conversation.Convs)
                }))
                {
                    // if (token.Choices[0].Delta.Role == "user")
                    //  {
                    content = token.Choices[0].Delta?.Content ?? "";
                    ai_text.Append(content);
                    content = content.Replace("\n", "[ENTRY]");
                    await HttpContext.Response.WriteAsync($"data: {content}\n\n");
                    await HttpContext.Response.Body.FlushAsync();
                    conversation = service.GetByID(id);
                    if (conversation.StopGenerating)
                        break;
                    // }

                }

                await HttpContext.Response.WriteAsync("data: [DONE]\n\n");
                await HttpContext.Response.Body.FlushAsync();

                HttpContext.Response.Body.Close();
                conversation.Title = ai_text.ToString();
                service.Replace(conversation);
            }

        }

        [HttpGet]
        [Route("chat/{id}")]
        public async Task Chat(long id, string prompt)
        {
            string requestProtocl = HttpContext.Request.Protocol;
            HttpContext.Response.Headers.Add("Content-Type", "text/event-stream");

            var service = new ConversationService();
            var conversation = service.GetByID(id);

            if (conversation is null || conversation.Id == 0)
            {
                conversation = new ConversationModel() { Convs = new List<Conv>(), Id = id,StopGenerating =false, Title = prompt };
            }

            conversation.Convs?.Add(new Conv {Speaker="human",Speech=prompt,Createtime=DateTime.Now});


            conversation.StopGenerating = false;
            service.Save(conversation);

            StringBuilder ai_text = new StringBuilder();
            string content = string.Empty;
            // for example
            await foreach (var token in api.Chat.StreamChatEnumerableAsync(new ChatRequest()
            {
                Model = chatgptModel,
                Temperature = 0.8,
                MaxTokens = _maxTokens,
                TopP=1,
                FrequencyPenalty=0.5,
                PresencePenalty=0,
                Messages = GetChatMessages(conversation.Convs)
        }))
            {
                // if (token.Choices[0].Delta.Role == "user")
                //  {
                content = token.Choices[0].Delta?.Content??"";
               ai_text.Append(content);
                content = content.Replace("\n", "[ENTRY]");
               await HttpContext.Response.WriteAsync($"data: {content}\n\n");
               await HttpContext.Response.Body.FlushAsync();
                conversation = service.GetByID(id);
                if (conversation.StopGenerating)
                    break;
               // }
           
            }



        //    await api.Chat.StreamCompletionAsync(new ChatRequest()
        //    {
        //        Model = Model.ChatGPTTurbo,
        //        Temperature = 0.1,
        //        MaxTokens = 50,
        //        Messages = new ChatMessage[] {
        //    new ChatMessage(ChatMessageRole.User, prompt)
        //}
        //    }, (i, ret) =>
        //    {
        //        HttpContext.Response.WriteAsync($"data:{ret.Choices[0].Message.Content}\n\n");
        //        HttpContext.Response.Body.FlushAsync();
        //        ;
        //    });

            //for(int i = 0; i < 5; i++)
            //{
            //    await HttpContext.Response.WriteAsync($"data:{1}\n\n");
            //    await HttpContext.Response.Body.FlushAsync();
            //    await Task.Delay(1000);
            //}
            await HttpContext.Response.WriteAsync("data: [DONE]\n\n");
            await HttpContext.Response.Body.FlushAsync();

            HttpContext.Response.Body.Close();

            conversation.Convs?.Add(new Conv()
            {
                Speaker="ai",
                Speeches= new List<string>() { ai_text.ToString()},
                Suitable=new List<int>() { 0},
                Createtime=DateTime.Now,
            } );
            service.Save( conversation );


        }
        private JsonResult OK<T>(T msg)
        {
            return new JsonResult(new { code = 200, data = msg });
        }

        private JsonResult ERR(string msg)
        {
            return new JsonResult(new { code = 500, msg = msg });
        }

        private IList<ChatMessage> GetChatMessages(List<Conv> convs) {

            var msgs = new List<ChatMessage>();
             msgs.Add(new ChatMessage()
            {
                Role = ChatMessageRole.System,
                Content = ""
            });
            if (convs is null) return msgs;

            foreach (var conv in convs) {
                 msgs.Add(new ChatMessage()
                {
                    Role = conv.Speaker == "human" ? ChatMessageRole.User : ChatMessageRole.Assistant,
                    Content = string.IsNullOrWhiteSpace(conv.Speech) ? conv.Speeches?[conv.Speeches.Count-1]  : conv.Speech
                 });
            }
            return msgs;
        
        }

     

    }
}
