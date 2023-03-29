using ChatAI.Repository.Model;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI.Repository.Service
{
    public class ConversationService
    {
        private SqlSugarClient _db;

        public ConversationService()
        {
            this._db = DBInstance.GetInstance();
        }

        /// <summary>
        /// 根据ID获取对话数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Conversation GetByID(long id)
        {
            return this._db.Queryable<Conversation>().Where(c => c.Id == id).First();
        }

        /// <summary>
        /// 保存对话，对话不存在则新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Save(Conversation model)
        {
            return this._db.Storageable(model).ExecuteCommand() >0 ?true : false;
        }
        /// <summary>
        /// 更新对话内容
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Replace(Conversation model)
        {
            return this._db.Updateable(model).ExecuteCommand() > 0 ? true : false;
        }
    }
}
