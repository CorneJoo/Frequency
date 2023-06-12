using Frequency.Shared;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Frequency.Server.Controllers
{
    [ApiController]
    public class FrequencyController : ControllerBase
    {
       
        private readonly ILogger<FrequencyController> _logger;

        public FrequencyController(ILogger<FrequencyController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("api/[controller]/GetFrequency")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFrequency(string bookUrl)
        {
            var words = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
            var word7len = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);

            await GetWordsFrequency(bookUrl, words);
           
            var orderList = words.ToList().OrderByDescending(a => a.Value);
            word7len = GetFirstSevenWordChar(orderList);

            var scrabbleList = GetScrabbleWordScores(words);
            var highestScore = scrabbleList.OrderByDescending(a => a.Value);

            return Ok(new FrequencyModel() 
            {
                MostFrequentWord = orderList.First().Key,
                MostFrequentWordCount = orderList.First().Value,
                MostFrequentSevenWord = word7len.First().Key,
                MostFrequentSevenWordCount = word7len.First().Value,
                HighestScoringWord = highestScore.First().Key,
                HighestScoringWordCount = highestScore.First().Value,
            });
        }

        private async Task GetWordsFrequency(string bookUrl, Dictionary<string, int> words)
        {
            var result = await new HttpClient().GetStringAsync(bookUrl);
            var wordPattern = new Regex(@"\w+");

            foreach (Match match in wordPattern.Matches(result))
            {
                if (!words.ContainsKey(match.Value))
                    words.Add(match.Value, 1);
                else
                    words[match.Value]++;
            }
        }

        private Dictionary<string, int> GetFirstSevenWordChar(IOrderedEnumerable<KeyValuePair<string, int>> orderList)
        {
            var returnList = new Dictionary<string, int>();

            foreach (var word in orderList)
            {
                if (word.Key.Length == 7)
                {
                    returnList.Add(word.Key, word.Value);
                    break;
                }
            }

            return returnList;
        }

        private Dictionary<string, int> GetScrabbleWordScores(Dictionary<string, int> words)
        {
            var scoreList = new Dictionary<string, int>();

            foreach (var word in words)
            {
                var score = ScrabbleScore(word.Key);
                scoreList.Add(word.Key, score);
            }

            return scoreList;
        }

        private static int ScrabbleScore(string scrabbleWord)
        {
            int score = 0;
            for (int i = 0; i < scrabbleWord.Length; i++)
            {
                char calculatedLetter = scrabbleWord.ToUpper().ElementAt(i);
                switch (calculatedLetter)
                {
                    case 'A':
                    case 'E':
                    case 'I':
                    case 'L':
                    case 'N':
                    case 'O':
                    case 'R':
                    case 'S':
                    case 'T':
                    case 'U':
                        score += 1;
                        break;
                    case 'D':
                    case 'G':
                        score += 2;
                        break;
                    case 'B':
                    case 'C':
                    case 'M':
                    case 'P':
                        score += 3;
                        break;
                    case 'F':
                    case 'H':
                    case 'V':
                    case 'W':
                    case 'Y':
                        score += 4;
                        break;
                    case 'K':
                        score += 5;
                        break;
                    case 'J':
                    case 'X':
                        score += 8;
                        break;
                    case 'Q':
                    case 'Z':
                        score += 10;
                        break;
                    default:
                        break;
                }
            }
            return score;
        }
    }
}