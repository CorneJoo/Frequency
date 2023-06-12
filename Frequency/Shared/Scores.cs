using System.Runtime.Serialization;

namespace Frequency.Shared
{
    public class Scores
    {
        [DataMember]
        public string Letter { get; set; }

        [DataMember]
        public int Score { get; set; }
    }
}
