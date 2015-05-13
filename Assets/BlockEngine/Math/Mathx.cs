namespace BlockEngine.Math
{
    public class Mathx
    {
        public static int DivideRoundDown(int numerator, int positiveDivisor)
        {
            if (numerator >= 0)
            {
                return numerator / positiveDivisor;
            }
            else
            {
                return (numerator + 1) / positiveDivisor - 1;
            }
        }
    }
}
