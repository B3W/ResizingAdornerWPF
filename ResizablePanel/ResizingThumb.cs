using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace ResizablePanel
{
   public class ResizingThumb : Thumb
   {
      [Flags]
      public enum AllowedDragDirections
      {
         Up = 0b0001,
         Down = 0b0010,
         Left = 0b0100,
         Right = 0b1000,
         All = 0b1111
      }

      #region Fields
      #endregion // Fields


      #region Properties

      /// <summary>
      /// Bit field indicating the directions this thumb can be dragged
      /// </summary>
      public AllowedDragDirections DragDirections { get; }

      #endregion // Properties


      #region Methods

      public ResizingThumb(AllowedDragDirections allowedDragDirections)
      {
         DragDirections = allowedDragDirections;
      }

      #endregion // Methods
   }
}
