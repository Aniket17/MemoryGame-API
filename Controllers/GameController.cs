using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MemoryGame.API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MemoryGame.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameController : ControllerBase
    {
        private readonly ILogger<GameController> _logger;
        private readonly IWebHostEnvironment env;

        public GameController(ILogger<GameController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            this.env = env;
        }

        [HttpPost("play/{difficultyLevel}")]
        public async Task<StartGameViewModel> Play(DifficultyLevel difficultyLevel)
        {
            int numberOfCards = 5;
            switch (difficultyLevel)
            {
                case DifficultyLevel.Easy:
                    numberOfCards = 5;
                    break;
                case DifficultyLevel.Medium:
                    numberOfCards = 10;
                    break;
                case DifficultyLevel.Hard:
                    numberOfCards = 25;
                    break;
                default:
                    break;
            }

            var rng = new Random();
            var cards = Enumerable.Range(1, numberOfCards)
                .SelectMany(x => new List<PlayerCard>() { new PlayerCard(x), new PlayerCard(x) })
                .ToList();
            //write to file..
            var game = new GameRoom(cards);
            await WriteToFile(game);
            return new StartGameViewModel(game.GameId.ToString(), cards.Select(x => x.Id).OrderBy(x=>Guid.NewGuid()).ToList());
        }

        private async Task<string> WriteToFile(GameRoom room)
        {
            var fileName = room.GameId + ".json";
            var filePath = Path.Combine(env.ContentRootPath, "Data", "GameRooms", fileName);
            var fileContent = JsonConvert.SerializeObject(room);
            await System.IO.File.WriteAllTextAsync(filePath, fileContent);
            return room.GameId.ToString();
        }

        [HttpGet("room/{gameId}/pick/{cardId}")]
        public PlayerCard Pick(string gameId, Guid cardId)
        {
            try
            {
                //Load game
                GameRoom game = GetGameRoom(gameId);
                //mark that card as picked..
                var card = game.Cards.SingleOrDefault(x => x.Id == cardId);
                card.Picked = true;
                if (game.LastPickedCard != null)
                {
                    if (game.LastPickedCard.CardNumber == card.CardNumber)
                    {
                        //matched
                        card.Matched = true;
                        var last = game.Cards.SingleOrDefault(x => x.Id == game.LastPickedCard.Id);
                        last.Matched = true;
                        game.LastPickedCard = null;
                    }
                    else
                    {
                        game.ErrorScore += 1;
                        //set picked to false for both cards..
                        card.Picked = false;
                        var last = game.Cards.SingleOrDefault(x => x.Id == game.LastPickedCard.Id);
                        last.Picked = false;
                    }
                    game.LastPickedCard = null;
                }
                else
                {
                    //set last as this is a new pick
                    game.LastPickedCard = card;
                }
                if (game.Cards.All(x => x.Matched))
                {
                    game.CompletedAt = DateTime.UtcNow;
                    card.GameOver = true;
                }
                _ = WriteToFile(game).Result;
                return card;
            }
            catch (IOException ex)
            {
                this._logger.LogError(ex.ToString(), ex);
                throw ex;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.ToString(), ex);
                throw ex;
            }
        }

        private GameRoom GetGameRoom(string gameId)
        {
            var fileName = gameId + ".json";
            var filePath = Path.Combine(env.ContentRootPath, "Data", "GameRooms", fileName);
            if (!System.IO.File.Exists(filePath))
            {
                throw new ArgumentException("Game not started.");
            }
            var game = JsonConvert.DeserializeObject<GameRoom>(System.IO.File.ReadAllText(filePath));
            return game;
        }

        [HttpGet("room/{gameId}/results")]
        public GameResult GetResults(string gameId)
        {
            var game = GetGameRoom(gameId);
            return game.GetGameResult();
        }

        [HttpGet("difficultyLevels")]
        public List<string> GetDifficultyLevels()
        {
            return Enum.GetNames(typeof(DifficultyLevel)).ToList();
        }
    }
}
