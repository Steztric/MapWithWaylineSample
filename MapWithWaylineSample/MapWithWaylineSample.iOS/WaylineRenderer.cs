using CoreGraphics;
using CoreLocation;
using MapKit;
using ObjCRuntime;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.iOS;
using Xamarin.Forms.Platform.iOS;
using MapWithWaylineSample;
using MapWithWaylineSample.iOS;

[assembly: ExportRenderer(typeof(MapWithWayline), typeof(WaylineRenderer))]
namespace MapWithWaylineSample.iOS
{
    public class WaylineRenderer : MapRenderer
    {
        MapWithWayline map;
        MKMapView nativeMap;

        // Wayline
        MKPolylineRenderer polylineRenderer;
        IMKOverlay currentWayline;

        // Pins
        UIView customPinView;

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.View> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                var formsMap = (MapWithWayline)e.OldElement;
                formsMap.CustomPins.CollectionChanged -= CustomPins_CollectionChanged;
                map = null;

                nativeMap.OverlayRenderer = null;
                nativeMap.GetViewForAnnotation = null;
                nativeMap = null;
            }

            if (e.NewElement != null)
            {
                var formsMap = (MapWithWayline)e.NewElement;
                formsMap.CustomPins.CollectionChanged += CustomPins_CollectionChanged;
                map = formsMap;

                nativeMap = Control as MKMapView;
                nativeMap.OverlayRenderer = GetOverlayRenderer;
                nativeMap.GetViewForAnnotation = GetViewForAnnotation;
            }
        }

        private void DrawPolyline(IEnumerable<Position> elements)
        {
            var coords = elements.Select(position => new CLLocationCoordinate2D(position.Latitude, position.Longitude));

            IMKOverlay oldWayline = currentWayline;

            if (oldWayline != null)
            {
                nativeMap.RemoveOverlay(oldWayline);
            }

            var wayline = MKPolyline.FromCoordinates(coords.ToArray());

            IMKOverlay overlay = Runtime.GetNSObject(wayline.Handle) as IMKOverlay;

            nativeMap.AddOverlay(overlay);
            currentWayline = overlay;
        }

        private void CustomPins_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Redraw the polyline and remove the old one
            DrawPolyline(map.CustomPins.Select(p => p.Pin.Position));
        }

        MKOverlayRenderer GetOverlayRenderer(MKMapView mapView, IMKOverlay overlayWrapper)
        {
            IMKOverlay overlay = Runtime.GetNSObject(overlayWrapper.Handle) as IMKOverlay;

            if (overlay is MKPolyline)
            {
                if (polylineRenderer == null)
                {
                    polylineRenderer = new MKPolylineRenderer(overlay as MKPolyline);
                    polylineRenderer.FillColor = UIColor.Blue;
                    polylineRenderer.StrokeColor = UIColor.Red;
                    polylineRenderer.LineWidth = 3;
                    polylineRenderer.Alpha = 0.4f;
                }
                return polylineRenderer;
            }
            else
            {
                return null;
            }
        }

        MKAnnotationView GetViewForAnnotation(MKMapView mapView, IMKAnnotation annotation)
        {
            MKAnnotationView annotationView = null;

            if (annotation is MKUserLocation) return null;

            var anno = annotation as MKPointAnnotation;

            if (anno == null) return null;

            var customPin = GetCustomPin(anno);
            if (customPin == null)
            {
                throw new Exception("Custom pin not found");
            }

            annotationView = mapView.DequeueReusableAnnotation(customPin.Id);
            if (annotationView == null)
            {
                var customView = new CustomMKAnnotationView(annotation, customPin.Id);
                customView.CalloutOffset = new CGPoint(0, 0);
                customView.Id = customPin.Id;
                customView.Depth = customPin.Depth;

                annotationView = customView;
            }
            annotationView.CanShowCallout = true;

            return annotationView;
        }
        CustomPin GetCustomPin(MKPointAnnotation annotation)
        {
            var position = new Position(annotation.Coordinate.Latitude, annotation.Coordinate.Longitude);
            return map.CustomPins.FirstOrDefault(cp => cp.Pin.Position == position);
        }
    }
}
