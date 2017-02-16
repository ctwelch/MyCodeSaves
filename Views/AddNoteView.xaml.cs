using BasicBridge.Models;
using BasicBridge.ViewModels;
using System.Windows;

namespace BasicBridge.Views
{
    /// <summary>
    /// Interaction logic for AddNoteView.xaml
    /// </summary>
    public partial class AddNoteView : Window
    {
        public AddNoteView(ToDo toDo, User currentUser)
        {
            InitializeComponent();
            DataContext = new AddNoteViewModel(toDo, currentUser);
        }
    }
}
