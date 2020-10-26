using System;

namespace MemoryGame.API.Models
{
    public class PlayerCard
    {
        public PlayerCard(int cardNumber)
        {
            this.CardNumber = cardNumber;
            this.Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public int CardNumber { get; set; }
        public bool Picked { get; set; }
        public bool Matched { get; set; }
        public bool GameOver { get; set; }
    }
}
