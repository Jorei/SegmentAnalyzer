namespace aspnetapp.Models
{
    public class ActivityViewModel
    {
        public void Process()
        {
            Route = new Route(Path.Combine("gpx", $"route_{SelectedRoute}"));

            Activity activity1 = new Activity(SelectedActivity1, Route);
            Activity activity2 = new Activity(SelectedActivity2, Route);

            Activities = new List<Activity>    { activity1, activity2 };
            Statistics.DifferenceBasedOnDistance(activity1, activity2);
        }

        public Route Route { get; private set; }
        public List<Activity> Activities { get; private set; }

        public string SelectedRoute { get; set; } = "Example_segment.tcx";
        public string SelectedActivity1 { get; set; } = Path.Combine("gpx", "Example_segment_trial1.tcx");
        public string SelectedActivity2 { get; set; } = Path.Combine("gpx", "Example_segment_trial2.gpx");
    }
}