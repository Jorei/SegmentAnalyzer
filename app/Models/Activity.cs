using System.Globalization;

namespace aspnetapp.Models
{
    public class Activity : Route
    {
        public List<double> Heartrate = new List<double>();
        public List<double> Altitude = new List<double>();
        public List<double> Cadans = new List<double>();
        public List<double> Speed = new List<double>();

        private Route Route;

        public Activity(string filename, Route route) : base(filename)
        {
            Route = route;

            ParseActivityData(filename);
            ProjectActivityToRoute();
        }

        private void ParseActivityData(string filename)
        {
            if (filename.EndsWith(".tcx"))
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                {
                    var pointStart  = tcxPoints.Where(p => p.Position != null).First(p => CalcDistance(Route.LatLng[0] [0], Route.LatLng[0] [1], (double)p.Position.LatitudeDegrees, (double)p.Position.LongitudeDegrees) < 0.025);
                    var idxStart = Array.IndexOf(tcxPoints, pointStart);
                    var pointEnd    = tcxPoints.Where(p => p.Position != null).Skip(idxStart + 1000).First(p => CalcDistance(Route.LatLng[^1][0], Route.LatLng[^1][1], (double)p.Position.LatitudeDegrees, (double)p.Position.LongitudeDegrees) < 0.025);
                    var idxEnd = Array.IndexOf(tcxPoints, pointEnd);

                    var Trackpoints = tcxPoints[idxStart..idxEnd];

                    Time      = Trackpoints.Select(p => DateTime.MinValue + (p.Time - Trackpoints[0].Time)).ToList();
                    LatLng    = Trackpoints.Select(p => new double[] { p.Position.LatitudeDegrees, p.Position.LongitudeDegrees }).ToList();
                    Heartrate = Trackpoints.Select(p => (double)p.HeartRateBpm.Value).ToList();
                    Cadans    = Trackpoints.Select(p => (double)p.Cadence).ToList();
                    Altitude  = Trackpoints.Select(p => p.AltitudeMeters).ToList();

                    NumberFormatInfo provider = new NumberFormatInfo();
                    provider.NumberDecimalSeparator = ".";

                    Speed     = Trackpoints.Select(p => Convert.ToDouble(Convert.ToDouble(p.Extensions.Any[0].InnerText, provider)  * 3.6)).ToList();
                }
            }

            if (filename.EndsWith(".gpx"))
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                {
                    var pointStart = gpxPoints.First(p => CalcDistance(Route.LatLng[0] [0], Route.LatLng[0] [1], (double)p.lat, (double)p.lon) < 0.025);
                    var indexStart = Array.IndexOf(gpxPoints, pointStart);

                    var pointEnd = gpxPoints.Skip(indexStart + 1000).First(p => CalcDistance(Route.LatLng[^1][0], Route.LatLng[^1][1], (double)p.lat, (double)p.lon) < 0.025);
                    var indexEnd = Array.IndexOf(gpxPoints, pointEnd);

                    var Trackpoints = gpxPoints[Array.IndexOf(gpxPoints, pointStart)..Array.IndexOf(gpxPoints, pointEnd)];

                    Time      = Trackpoints.Select(p => DateTime.MinValue + (p.time - Trackpoints[0].time)).ToList();
                    LatLng    = Trackpoints.Select(p => new double[] { (double)p.lat, (double)p.lon }).ToList();

                    foreach (var trackpoint in Trackpoints)
                    {
                        foreach (var extension in trackpoint.extensions.Any)
                        {
                            Heartrate.Add(Convert.ToDouble(extension.GetElementsByTagName("gpxtpx:hr")[0].InnerText));
                            Cadans.Add(Convert.ToDouble(extension.GetElementsByTagName("gpxtpx:cad")[0].InnerText));
                        }
                    }

                    Altitude  = Trackpoints.Select(p => (double)p.ele).ToList();
                }
            }
        }

        private void ProjectActivityToRoute()
        {
            Distance       = new List<double>();
            int routeIndex = 0;
            int routeScope = 15;

            for (int i = 0; i < LatLng.Count; i++)
            {
                LatLng[i]  = Route.LatLng.Skip(routeIndex).Take(routeScope).MinBy(p => CalcDistance(p[0], p[1], LatLng[i][0], LatLng[i][1]));
                routeIndex = Route.LatLng.IndexOf(LatLng[i]);
                Distance.Add(Route.Distance.ElementAt(routeIndex));
            }
        }
    }
}