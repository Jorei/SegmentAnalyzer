using Microsoft.AspNetCore.Components.Web;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace aspnetapp.Models
{
    internal static class Serializers
    {
        public readonly static XmlSerializer Tcx = new XmlSerializer(typeof(TrainingCenterDatabase_t));
        public readonly static XmlSerializer Gpx = new XmlSerializer(typeof(gpxType));
    }

    public class Route
    {
        public string Name;
        public List<DateTime> Time;
        public List<double>   Distance;
        public List<double[]> LatLng;

        internal Trackpoint_t[] tcxPoints;
        internal wptType[]      gpxPoints;

        public Route()
        {

        }

        public Route(string filename)
        {
            Name = filename;

            if (filename.EndsWith(".tcx"))
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                {
                    TrainingCenterDatabase_t activity = Serializers.Tcx.Deserialize(fs) as TrainingCenterDatabase_t;

                    Activity_t Activity        = activity.Activities.Activity.First();
                    ActivityLap_t Lap          = Activity.Lap.First();

                    tcxPoints = Lap.Track;
                    tcxPoints = tcxPoints.Where(p => p.Position != null).ToArray();

                    Time     = tcxPoints.Select(p => DateTime.MinValue + (p.Time - tcxPoints[0].Time)).ToList();
                    LatLng   = tcxPoints.Select(p => new double[] { p.Position.LatitudeDegrees, p.Position.LongitudeDegrees }).ToList();
                    Distance = tcxPoints.Select(p => (p.DistanceMeters - tcxPoints[0].DistanceMeters) / 1000).ToList();
                }
            }

            if (filename.EndsWith(".gpx"))
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                {
                    gpxType activity = Serializers.Gpx.Deserialize(fs) as gpxType;

                    trkType Activity      = activity.trk.First();
                    trksegType Lap        = Activity.trkseg.First();

                    gpxPoints = Lap.trkpt;

                    Time     = gpxPoints.Select(p => DateTime.MinValue + (p.time - gpxPoints[0].time)).ToList();
                    LatLng   = gpxPoints.Select(p => new double[] { (double)p.lat, (double)p.lon }).ToList();
                    Distance = CoordsToDistance(LatLng);
                }
            }
        }

        internal double CalcDistance(double lat1, double lng1, double lat2, double lng2)
        {
            double R = 6371; // Earth radius

            double diff_lat = (lat2 - lat1) * (Math.PI / 180);
            double diff_lng = (lng2 - lng1) * (Math.PI / 180);

            double r_lat1 = lat1 * (Math.PI / 180);
            double r_lat2 = lat2 * (Math.PI / 180);

            double a = Math.Sin(diff_lat / 2) * Math.Sin(diff_lat / 2) +
                       Math.Sin(diff_lng / 2) * Math.Sin(diff_lng / 2) * Math.Cos(r_lat1) * Math.Cos(r_lat2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            double d = R * c;
            return Math.Abs(d);
        }

        internal List<double> CoordsToDistance(List<double[]> coords)
        {
            List<double> distance = new List<double> { 0 };
            for (int i = 1; i < coords.Count; i++)
            {
                distance.Add(distance[i - 1] + CalcDistance(coords[i - 1][0], coords[i - 1][1], coords[i][0], coords[i][1]));
            }
            return distance;
        }
    }
}