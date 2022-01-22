using System;
using System.Windows;
using System.Windows.Documents;

namespace ResizablePanel
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      public MainWindow()
      {
         ContentRendered += MainWindow_ContentRendered;

         InitializeComponent();
      }

      private void MainWindow_ContentRendered(object sender, EventArgs e)
      {
         // Adorn all elements of the main panel
         ResizingAdornerConfig btnConfig = new ResizingAdornerConfig();
         AdornerLayer.GetAdornerLayer(MyButton).Add(new ResizingAdorner(MyButton, btnConfig));

         ResizingAdornerConfig txtbxConfig = new ResizingAdornerConfig();
         AdornerLayer.GetAdornerLayer(MyTextBox).Add(new ResizingAdorner(MyTextBox, txtbxConfig));

         ResizingAdornerConfig rtxtbxConfig = new ResizingAdornerConfig();
         AdornerLayer.GetAdornerLayer(MyRichTextBox).Add(new ResizingAdorner(MyRichTextBox, rtxtbxConfig));
      }
   }
}
