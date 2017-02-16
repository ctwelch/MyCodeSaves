using BasicBridge.Models;
using BasicBridge.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BasicBridge.Views
{
    /// <summary>
    /// Interaction logic for ConfigView.xaml
    /// </summary>
    public partial class ConfigView : Window
    {
        public ConfigView(ToDo selectedToDo, User user)
        {
            InitializeComponent();

            DataContext = new ConfigViewModel(selectedToDo, user);
            SelectedToDo = selectedToDo;
        }
        public ToDo SelectedToDo { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<ToDo> todos = ToDoPicker1.ItemsSource as List<ToDo>;
            var que = todos.First(x => x.ActCode == SelectedToDo.ActCode);
            ToDoPicker1.SelectedIndex = ToDoPicker1.Items.IndexOf(que);

            List<Questionnaire> questionnaires = QuestionnairePicker.ItemsSource as List<Questionnaire>;
            var que2 = questionnaires.First(x => x.Qlabel == "FORECLOSURE");
            QuestionnairePicker.SelectedIndex = QuestionnairePicker.Items.IndexOf(que2);
        }
    }
}
