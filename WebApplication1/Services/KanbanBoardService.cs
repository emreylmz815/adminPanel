using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class KanbanBoardService
    {
        private const string SessionKey = "KANBAN_BOARD_STATE";

        public KanbanBoard GetOrCreateBoard(HttpContextBase context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var board = context.Session[SessionKey] as KanbanBoard;
            if (board != null)
            {
                return board;
            }

            board = CreateDefaultBoard();
            context.Session[SessionKey] = board;
            return board;
        }

        public void SaveBoard(HttpContextBase context, KanbanBoard board)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Session[SessionKey] = board;
        }

        public void Reset(HttpContextBase context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Session.Remove(SessionKey);
        }

        private static KanbanBoard CreateDefaultBoard()
        {
            var board = new KanbanBoard();
            board.Columns = new List<KanbanColumn>
            {
                new KanbanColumn { Name = "Yapılacak" },
                new KanbanColumn { Name = "Devam Ediyor" },
                new KanbanColumn { Name = "Tamamlandı" }
            };

            return board;
        }

        public void MoveCard(KanbanBoard board, Guid cardId, Guid targetColumnId)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            var card = board.FindCard(cardId);
            if (card == null)
            {
                return;
            }

            var originColumn = board.Columns.FirstOrDefault(c => c.Cards.Any(cardItem => cardItem.Id == cardId));
            var targetColumn = board.FindColumn(targetColumnId);
            if (targetColumn == null)
            {
                return;
            }

            if (originColumn != null)
            {
                originColumn.Cards = originColumn.Cards.Where(cardItem => cardItem.Id != cardId).ToList();
            }

            if (targetColumn.IsAtCapacity)
            {
                return;
            }

            targetColumn.Cards.Add(card);
        }
    }
}
