using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MemoryGame.API.Models
{
    public class GameRoom
    {
        public GameRoom()
        {

        }
        public GameRoom(List<PlayerCard> Cards)
        {
            this.Cards = Cards;
            GameId = Guid.NewGuid();
            StartedAt = DateTime.UtcNow;
            LastPickedCard = null;
        }
        public Guid GameId { get; set; }
        public int ErrorScore { get; set; } = 0;
        public List<PlayerCard> Cards { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public PlayerCard LastPickedCard { get; set; }

        public GameResult GetGameResult()
        {
            if (!this.CompletedAt.HasValue)
            {
                throw new Exception("Game is in progress");
            }
            var result = new GameResult()
            {
                ErrorScore = this.ErrorScore,
                TimeInSeconds = this.CompletedAt.Value.Subtract(StartedAt).TotalSeconds
            };
            return result;
        }
    }
}
