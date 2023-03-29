using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ChatAI.Repository.Model
{
    [SugarTable("conversation")]//当和数据库名称不一样可以设置表别名 指定表明
    public class Conversation
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]//数据库是自增才配自增 
        public int Id { get; set; }

        public string Speaker { get; set; } = string.Empty;
        public string Speecn { get; set; } = string.Empty;
        public int Createtime { get; set; }
        public string Speeches { get; set; } = string.Empty;
        public string Suitable { get; set; } = string.Empty;
      
    }
}

 