using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadgeScreen.M2VM.Models
{
    public class MessageModel
    {
        public Guid Id { get; set; }
        public string MessContent { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUsedDate { get; set; }

        public MessageModel(Guid id, string content, string tag, DateTime createdDate, DateTime lastUsedDate)
        {
            this.Id = id;
            this.MessContent = content;
            CreatedDate = createdDate;
            LastUsedDate = lastUsedDate;
        }

        public void UpdateLastUpdate(DateTime newLastUpdateDate)
        {
            LastUsedDate = newLastUpdateDate;
        }
    }
}