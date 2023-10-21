namespace aspnetapp.Models
{
    static public class Statistics 
    {
        static public List<double> Distance = new List<double>();
        static public List<double> Time     = new List<double>();

        static public void DifferenceBasedOnDistance(Activity activity1, Activity activity2)
        {
            Distance = new List<double>();
            Time     = new List<double>();

            for (int i = 0; i < activity1.Distance.Count; i++)
            {
                int index = activity2.Distance.IndexOf(activity1.Distance[i]);
                if (index != -1)
                {
                    Distance.Add(activity1.Distance[i]);
                    Time.Add((activity1.Time[i].Ticks - activity2.Time[index].Ticks) / 1e7); // 1 tick = 100 nanoseconds
                }
            }
        }

    }
}