using BasicBridge.Models;
using BasicBridge.ViewModels;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace BasicBridge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(User user)
        {
            InitializeComponent();

            DataContext = new ToDoViewModel(user);
            
            
        }

        private void ToDoGrid_LostFocus(object sender, RoutedEventArgs e)
        {            
            ((ToDoViewModel)DataContext).UpdateUserInputs();            
        }       

        private void ViewFileLocations_MouseDoubleClickHandler(object sender, RoutedEventArgs e)
        {
            ((ToDoViewModel)DataContext).OpenDocFromViewFilesCommand.Execute(new { });
        }

    }
}
