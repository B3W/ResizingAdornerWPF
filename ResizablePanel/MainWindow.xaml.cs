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

         // Add text to the RichTextBox
         MyRichTextBox.Document.Blocks.Clear();
         MyRichTextBox.Document.Blocks.Add(new Paragraph(new Run("RICHTEXTBOX")));
      }

      private void MainWindow_ContentRendered(object sender, EventArgs e)
      {
         // Adorn all elements of the main panel
         ResizeAdornerConfig btnConfig = new ResizeAdornerConfig();
         AdornerLayer.GetAdornerLayer(MyButton).Add(new ResizeAdorner(MyButton, btnConfig));

         ResizeAdornerConfig txtbxConfig = new ResizeAdornerConfig();
         AdornerLayer.GetAdornerLayer(MyTextBox).Add(new ResizeAdorner(MyTextBox, txtbxConfig));

         ResizeAdornerConfig rtxtbxConfig = new ResizeAdornerConfig();
         AdornerLayer.GetAdornerLayer(MyRichTextBox).Add(new ResizeAdorner(MyRichTextBox, rtxtbxConfig));

         ResizeAdornerConfig borderConfig = new ResizeAdornerConfig();
         AdornerLayer.GetAdornerLayer(MyBorder).Add(new ResizeAdorner(MyBorder, borderConfig));
      }
   }
}
