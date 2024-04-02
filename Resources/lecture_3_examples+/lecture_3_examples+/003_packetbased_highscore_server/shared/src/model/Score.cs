namespace shared
{
    /**
     * Simple player score value holder.
     */
    public class Score
    {
        public readonly string name;
        public readonly int score;

        public Score(string pName, int pScore)
        {
            name = pName;
            score = pScore;
        }
    }
}