using LiteDB;
using static ChatAI.Dotnet.Services.LiteDBHelper;

namespace ChatAI.Dotnet.Services
{
    public class ConversationService
    {
        /// <summary>
        /// 根据ID获取对话数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ConversationModel GetByID(long id)
        {
            ConversationModel dt1 = LiteDBHelper.Instance.GetConversationModel().FindOne(x => x.Id == id);
           return dt1;
        }

        /// <summary>
        /// 保存对话，对话不存在则新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Save(ConversationModel model)
        {
            var db = LiteDBHelper.Instance.GetConversationModel();  
            var val = db.Upsert(model);
            return val;
        }
        /// <summary>
        /// 更新对话内容
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Replace(ConversationModel model)
        {
            var db = LiteDBHelper.Instance.GetConversationModel();
            return db.Update(model);
        }
    }
}
