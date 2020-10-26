using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MemoryGame.API.Models
{
    public class StartGameViewModel
    {
        public StartGameViewModel(string gameId, List<Guid> cardIds)
        {
            GameId = gameId;
            CardIds = cardIds;
        }

        public string GameId { get; }
        public List<Guid> CardIds { get; }
    }
}
