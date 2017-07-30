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
                nativeMap.DidSelectAnnotationView -= OnDidSelectAnnotationView;
                nativeMap.DidDeselectAnnotationView -= OnDidDeselectAnnotationView;
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
                nativeMap.DidSelectAnnotationView += OnDidSelectAnnotationView;
                nativeMap.DidDeselectAnnotationView += OnDidDeselectAnnotationView;
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
            var nativeMap = Control as MKMapView;

            // Draw circles for the new pins
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (CustomPin cp in e.NewItems)
                {
                    IMKOverlay circleOverlay = MKCircle.Circle(new CLLocationCoordinate2D(cp.Pin.Position.Latitude, cp.Pin.Position.Longitude), 4.0);
                    try
                    {
                        nativeMap.AddOverlay(Runtime.GetNSObject(circleOverlay.Handle) as IMKOverlay);

                    }
                    catch (Exception)
                    {
                        nativeMap.AddOverlay(circleOverlay);
                    }
                }
            }

            // Redraw the polyline and remove the old one
            var pins = sender as IEnumerable<CustomPin>;
            DrawPolyline(pins.Select(p => p.Pin.Position));
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

        void OnDidSelectAnnotationView(object sender, MKAnnotationViewEventArgs e)
        {
            var customView = e.View as CustomMKAnnotationView;
            map.SelectedPin = map.CustomPins.FirstOrDefault(cp => cp.Id == customView.Id);

            customPinView = new UIView();
            customPinView.Frame = new CGRect(0, 0, 200, 84);
            customPinView.Center = new CGPoint(0, -(e.View.Frame.Height + 75));
            e.View.AddSubview(customPinView);
        }

        void OnDidDeselectAnnotationView(object sender, MKAnnotationViewEventArgs e)
        {
            if (!e.View.Selected)
            {
                map.SelectedPin = null;
                customPinView.RemoveFromSuperview();
                customPinView.Dispose();
                customPinView = null;
            }
        }

        CustomPin GetCustomPin(MKPointAnnotation annotation)
        {
            var position = new Position(annotation.Coordinate.Latitude, annotation.Coordinate.Longitude);
            return map.CustomPins.FirstOrDefault(cp => cp.Pin.Position == position);
        }
    }
}
