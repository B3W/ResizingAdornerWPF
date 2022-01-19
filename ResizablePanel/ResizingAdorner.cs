using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace ResizablePanel
{
   public class ResizingAdorner : Adorner
   {
      private enum ThumbPosition
      {
         Top = 0,
         Bottom,
         Left,
         Right,
         TopLeft,
         TopRight,
         BottomLeft,
         BottomRight,
         NumPositions
      }

      #region Fields

      /// <summary>
      /// Collection of all visual children managed by adorner
      /// </summary>
      private readonly VisualCollection _visualChildren;

      /// <summary>
      /// Collection of all thumbs that can be used to resize adorned element
      /// </summary>
      private readonly Thumb[] _resizeThumbs;

      /// <summary>
      /// Mapping of configuration of thumb position to allowed drag direction
      /// </summary>
      private readonly Dictionary<ThumbPosition, ResizingThumb.AllowedDragDirections> dragDirectionMap
         = new Dictionary<ThumbPosition, ResizingThumb.AllowedDragDirections>()
      {
         { ThumbPosition.Top,          ResizingThumb.AllowedDragDirections.Up & ResizingThumb.AllowedDragDirections.Down },
         { ThumbPosition.Bottom,       ResizingThumb.AllowedDragDirections.Up & ResizingThumb.AllowedDragDirections.Down },
         { ThumbPosition.Left,         ResizingThumb.AllowedDragDirections.Left & ResizingThumb.AllowedDragDirections.Right },
         { ThumbPosition.Right,        ResizingThumb.AllowedDragDirections.Left & ResizingThumb.AllowedDragDirections.Right },
         { ThumbPosition.TopLeft,      ResizingThumb.AllowedDragDirections.All },
         { ThumbPosition.TopRight,     ResizingThumb.AllowedDragDirections.All },
         { ThumbPosition.BottomLeft,   ResizingThumb.AllowedDragDirections.All },
         { ThumbPosition.BottomRight,  ResizingThumb.AllowedDragDirections.All },
      };

      #endregion // Fields


      #region Properties

      // Overriding FrameworkElement property as this adorner will contain more than one visual
      // Allows WPF framework to interface with this adorner's visual collection
      protected override int VisualChildrenCount => _visualChildren.Count;

      #endregion // Properties


      #region Methods

      public ResizingAdorner(UIElement adornedElement) : base(adornedElement)
      {
         _visualChildren = new VisualCollection(adornedElement);
         _resizeThumbs = new Thumb[(int)ThumbPosition.NumPositions];

         // Create all resize thumbs
         _resizeThumbs[(int)ThumbPosition.Top] = CreateResizeThumb(ThumbPosition.Top, Cursors.SizeNS);
         _resizeThumbs[(int)ThumbPosition.Bottom] = CreateResizeThumb(ThumbPosition.Bottom, Cursors.SizeNS);
         _resizeThumbs[(int)ThumbPosition.Left] = CreateResizeThumb(ThumbPosition.Left, Cursors.SizeWE);
         _resizeThumbs[(int)ThumbPosition.Right] = CreateResizeThumb(ThumbPosition.Right, Cursors.SizeWE);
         _resizeThumbs[(int)ThumbPosition.TopLeft] = CreateResizeThumb(ThumbPosition.TopLeft, Cursors.SizeNWSE);
         _resizeThumbs[(int)ThumbPosition.TopRight] = CreateResizeThumb(ThumbPosition.TopRight, Cursors.SizeNESW);
         _resizeThumbs[(int)ThumbPosition.BottomLeft] = CreateResizeThumb(ThumbPosition.BottomLeft, Cursors.SizeNESW);
         _resizeThumbs[(int)ThumbPosition.BottomRight] = CreateResizeThumb(ThumbPosition.BottomRight, Cursors.SizeNWSE);

         // Add thumbs to the visual children of the adorner
         foreach (Thumb thumb in _resizeThumbs)
         {
            _ = _visualChildren.Add(thumb);
         }
      }


      // Overriding FrameworkElement property as this adorner will contain more than one visual
      // Allows WPF framework to interface with this adorner's visual collection
      protected override Visual GetVisualChild(int index)
      {
         return _visualChildren[index];
      }


      /// <summary>
      /// Creates a thumb that can be used to resize adorned element in specified direction
      /// </summary>
      /// <returns></returns>
      private ResizingThumb CreateResizeThumb(ThumbPosition position, Cursor cursor)
      {
         ResizingThumb resizeThumb = new ResizingThumb(dragDirectionMap[position])
         {
            Cursor = cursor,
         };

         resizeThumb.DragStarted += OnDragStarted;
         resizeThumb.DragDelta += OnDragDelta;

         return resizeThumb;
      }



      private void OnDragStarted(object sender, DragStartedEventArgs e)
      {

      }



      private void OnDragDelta(object sender, DragDeltaEventArgs e)
      {

      }

      #endregion // Methods
   }
}
