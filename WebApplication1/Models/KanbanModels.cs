using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WebApplication1.Models
{
    public class KanbanBoard
    {
        public KanbanBoard()
        {
            Title = "İş Takip Kanban";
            Columns = new List<KanbanColumn>();
        }

        public string Title { get; set; }

        public IList<KanbanColumn> Columns { get; set; }

        public KanbanColumn FindColumn(Guid id)
        {
            return Columns.FirstOrDefault(column => column.Id == id);
        }

        public KanbanCard FindCard(Guid id)
        {
            return Columns.SelectMany(column => column.Cards).FirstOrDefault(card => card.Id == id);
        }
    }

    public class KanbanColumn
    {
        public KanbanColumn()
        {
            Id = Guid.NewGuid();
            Cards = new List<KanbanCard>();
        }

        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Range(0, int.MaxValue)]
        public int? WorkInProgressLimit { get; set; }

        public IList<KanbanCard> Cards { get; set; }

        public bool IsAtCapacity => WorkInProgressLimit.HasValue && Cards.Count >= WorkInProgressLimit.Value;
    }

    public class KanbanCard
    {
        public KanbanCard()
        {
            Id = Guid.NewGuid();
            CreatedOn = DateTime.Now;
        }

        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public string Owner { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? DueDate { get; set; }
    }
}
