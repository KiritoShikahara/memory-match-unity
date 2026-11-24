namespace MemoryMatch.Core
{
    public enum Prize { Coin10, Coin20, Mushroom, Flower, Star, OneUp, Gem, Clover }

    public static class Prizes
    {
        public static int PointsOf(Prize prize)
        {
            switch (prize)
            {
                case Prize.Coin10: return 10;
                case Prize.Coin20: return 20;
                case Prize.Mushroom: return 30;
                case Prize.Flower: return 40;
                case Prize.Star: return 50;
                case Prize.Gem: return 60;
                case Prize.Clover: return 15;
                default: return 100;
            }
        }
    }
}
