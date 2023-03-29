using LiteDB;

namespace ChatAI.Dotnet.Services
{
    public sealed class LiteDBHelper
    {
        private static readonly Lazy<LiteDBHelper> lazy = new Lazy<LiteDBHelper>(() => new LiteDBHelper());
        public static LiteDBHelper Instance { get { return lazy.Value; } }

        private LiteDBHelper()
        {

        }

        public LiteDB.LiteDatabase db = new LiteDB.LiteDatabase(GetDBPath());


        public ILiteCollection<ConversationModel> GetConversationModel()
        {
            return db.GetCollection<ConversationModel>();
        }

        public static string GetDBPath()
        {

            string connectionString = "chatai_dotnet.db";
            if (!File.Exists(connectionString))
            {
                File.Create(connectionString).Dispose();  //创建完文件之后，要把资源释放掉，才能继续往文件里书写内容
            }
            return connectionString;
        }

        public class ConversationModel
        {
            public long Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public bool StopGenerating { get; set; }
            public List<Conv>? Convs { get; set; }

         
        }
        public class Conv
        {
            public string Speaker { get; set; } = string.Empty;
            public string Speech { get; set; } = string.Empty;
            public DateTime Createtime { get; set; }
            public List<string>? Speeches { get; set; }
            public List<int>? Suitable { get; set; } 
        }

    }
}
