using BasicBridge.Models;
using BasicBridge.Views;
using System.Collections.Generic;
using System.Windows.Input;
using System;
using System.Collections.ObjectModel;

namespace BasicBridge.ViewModels
{
    public class ToDoViewModel : BaseViewModel
    {
        private DataModel model;
        public ToDoViewModel(User user)
        {
            CurrentUser = user;
            model = new DataModel(user);
        }

        public User CurrentUser { get; set; }

        private List<ToDo> _toDos;
        public List<ToDo> ToDos
        {
            get
            {
                if(_toDos == null)
                {
                    _toDos = model.DefaultToDoFilter();
                }
                return _toDos;
            }
            set
            {
                _toDos = value;
                FlagPropertyChanged("ToDos");
            }
        }

        private ToDo _selectedToDo;
        public ToDo SelectedToDo
        {
            get
            {
                return _selectedToDo;
            }
            set
            {
                // this is my nightmare
                // need to re-do how the model presents data to the viewModel
                // let the viewmodel subscribe to events on the model to get changes

                _selectedToDo = value;

                PpFields = GetUserConfiguredToDoFields();
                PpNotes = GetUserConfiguredToDoNotes();
                UserInputs = GetUserInputs();
                NoteAutomations = GetToDoAutomations();

                FlagPropertyChanged("SelectedToDo");
            }
        }

        public void UpdateUserInputs()
        {
            // need to fix this so that it only runs when the InputValue has changed
            if (UserInputs != null)
            {
                foreach (var input in UserInputs)
                {
                    input.EntityNum = SelectedToDo.EntityNum;
                    input.ToDoId = SelectedToDo.ActCode;
                    input.UserId = CurrentUser.UserId;
                    input.InputValue = 
                        input.InputValue != null ? input.InputValue.Replace(@"'", @"''") : "";

                    model.UpdateUserInput(input);
                }

                UserInputs = GetUserInputs();
            }
        }

        private Dictionary<string, string> _toDoFilters;
        public Dictionary<string,string> ToDoFilters
        {
            get
            {
                if(_toDoFilters == null)
                {
                    _toDoFilters = new Dictionary<string, string>();
                    _toDoFilters.Add("Default", "DefaultToDoFilter");
                    _toDoFilters.Add("Unassigned E-Filing", "UnassignedEfilingsToDoFilter");
                    _toDoFilters.Add("Assigned E-Filing", "AssignedEfilingsToDoFilter");
                    _toDoFilters.Add("Cancel Audit", "CancelAuditToDoFilter");
                    _toDoFilters.Add("Efile Today", "EfileTodayToDoFilter");
                    _toDoFilters.Add("RTS Mail", "RtsMailToDoFilter");
                }
                return _toDoFilters;
            }
        }

        private string _selectedToDoFilter;
        public string SelectedToDoFilter
        {
            get
            {
                if(_selectedToDoFilter == null)
                {
                    _selectedToDoFilter = "DefaultToDoFilter";
                    GetFilteredToDos();
                }
                return _selectedToDoFilter;
            }
            set
            {
                _selectedToDoFilter = value;
                FlagPropertyChanged("SelectedToDoFilter");
                GetFilteredToDos();
            }
        }        

        private void GetFilteredToDos()
        {
            ToDos = typeof(DataModel).GetMethod(_selectedToDoFilter).Invoke(model, null) as List<ToDo>;
        }

        private string _selectedViewFileLocation;
        public string SelectedViewFileLocation
        {
            get
            {
                return _selectedViewFileLocation;
            }
            set
            {
                _selectedViewFileLocation = value;
                FlagPropertyChanged("SelectedViewFileLocation");
                System.Windows.Clipboard.SetText(_selectedViewFileLocation);
            }
        }

        private List<NoteAutomation> GetToDoAutomations()
        {
            if(SelectedToDo != null)
            {
                return model.GetToDoNoteAutomations(SelectedToDo);
            }

            return new List<NoteAutomation>();
        }

        private List<Field> _ppFields;
        public List<Field> PpFields
        {
            get
            {
                if (_ppFields == null)
                {
                    _ppFields = GetUserConfiguredToDoFields();
                }
                return _ppFields;
            }
            set
            {
                _ppFields = value;
                FlagPropertyChanged("PpFields");
            }
        }          
        public List<Field> GetUserConfiguredToDoFields()
        {
            if(SelectedToDo != null)
            {
                return model.GetUserConfiguredToDoFields(SelectedToDo);
            }

            return new List<Field>();
        }

        private List<Note> _ppNotes;
        public List<Note> PpNotes
        {
            get
            {
                if (_ppNotes == null)
                {
                    _ppNotes = GetUserConfiguredToDoNotes();
                }
                return _ppNotes;
            }
            set
            {
                _ppNotes = value;
                FlagPropertyChanged("PpNotes");
            }
        }
      
        private List<Note> GetUserConfiguredToDoNotes()
        {
            if (SelectedToDo != null)
            {
                return model.GetUserConfiguredToDoNotes(SelectedToDo);
            }

            return new List<Note>();
        }

        private RelayCommand _openConfigView;
        public ICommand OpenConfigView
        {
            get
            {
                if(_openConfigView == null)
                {
                    _openConfigView = new RelayCommand(OpenConfigureView, IsSelectedToDoNull);
                }
                return _openConfigView;
            }
        }
        public void OpenConfigureView(object parameter)
        {
            var view = new ConfigView(SelectedToDo, CurrentUser);
            view.Show();
        }
        private bool IsSelectedToDoNull(object item)
        {
            if(SelectedToDo != null)
            {
                return true;
            }
            return false;
        }

        private RelayCommand _markToDoCompleteCommand;
        public ICommand MarkToDoCompleteCommand
        {
            get
            {
                if(_markToDoCompleteCommand == null)
                {
                    _markToDoCompleteCommand = new RelayCommand(MarkToDoComplete, CanUserMarkToDoComplete);
                }
                return _markToDoCompleteCommand;
            }
        }
        private void MarkToDoComplete(object item)
        {
            ProcessAutomations();
            model.SetToDoComplete(SelectedToDo);
            model.PopulateDocSentField(SelectedToDo.TranNum);
            GetFilteredToDos();
        }
        private bool CanUserMarkToDoComplete(object item)
        {
            if(SelectedToDo != null && SelectedToDoFilter=="DefaultToDoFilter")
            {
                return true;
            }
            return false;
        }

        private List<UserInput> _userInputs;
        public List<UserInput> UserInputs
        {
            get
            {
                return _userInputs;
            }
            set
            {
                _userInputs = value;
                FlagPropertyChanged("UserInputs");
            }
        }
        private List<UserInput> GetUserInputs()
        {
            if(SelectedToDo != null)
            {
                return model.GetToDoUserInputs(SelectedToDo);
            }

            return new List<UserInput>();
        }
        private void ProcessAutomations()
        {
            // toDo.Automations = model.getToDoAutomations(toDo, Environment.UserName);

            // the TranText for NoteAutomations is stored in the DB 
            // as the concatenation of all rows from ToDoUserNoteTranTextSegments
            // for a given ToDoId, UserId and note ActCode
            // ordered by Id? 

            // then, for each NoteAutomation, need to get the TranText by 
            // pulling back all the segments, parsing the segment type
            // then concatenating or doing another call (like for field inputs) and concatenating
            // by then end you should have your TranText

            foreach (var automation in NoteAutomations)
            {
                var noteToInsert = new Note();
                noteToInsert.EntityNum = SelectedToDo.EntityNum;
                noteToInsert.ActCode = automation.NoteToInsert;
                noteToInsert.ReservedF2 = SelectedToDo.ReservedF2;
                    
                // Here we need to build the TranText using the TranTextSegments associated with this ToDo

                var allSegments = model.GetUserTranTextSegments(SelectedToDo.ActCode);

                foreach(var segment in allSegments)
                {
                    if(segment is ConstantTranTextSegment)
                    {
                        noteToInsert.TranText += ((ConstantTranTextSegment)segment).ConstantText.Replace(@"'", @"''") + " ";
                    }
                    else if(segment is UserInputTranTextSegment)
                    {
                        var inputLabel = ((UserInputTranTextSegment)segment).InputLabel.Replace(@"'", @"''");
                        var inputValue = model.GetToDoUserInputValue(inputLabel, SelectedToDo).InputValue.Replace(@"'", @"''");

                        noteToInsert.TranText += inputValue + " ";
                    }
                    else if(segment is ToDoPropertyTranTextSegment)
                    {                        
                        noteToInsert.TranText +=
                            SelectedToDo.GetType()
                            .GetProperty(((ToDoPropertyTranTextSegment)segment).ToDoPropertyName)
                            .GetValue(SelectedToDo) + " ".Replace(@"'", @"''");
                    }
                }

                foreach(var input in UserInputs)
                {
                    if(input.InputLabel.ToLower().Contains("efile"))
                    {
                        int confNum;
                        var toTry = input.InputValue.IndexOf(' ') > 0 ? input.InputValue.Substring(0, input.InputValue.IndexOf(' ')).Trim() : input.InputValue;
                        int.TryParse(toTry, out confNum);
                        noteToInsert.AmountBilled = confNum;
                    }
                }

                model.InsertNote(noteToInsert);
            }
        }

        private List<NoteAutomation> _noteAutomations;
        public List<NoteAutomation> NoteAutomations
        {
            get
            {
                return _noteAutomations;
            }
            set
            {
                _noteAutomations = value;
            }
        }

        private RelayCommand _processAutomationsCommand;
        public ICommand ProcessAutomationsCommand
        {
            get
            {
                if (_processAutomationsCommand == null)
                {
                    _processAutomationsCommand = new RelayCommand(ProcessAutomations);
                }
                return _processAutomationsCommand;
            }            
        }
        private void ProcessAutomations(object obj)
        {
            var toDo = obj as ToDo;

            foreach(var automation in toDo.Automations)
            {                
                automation.Run();
            }
        }

        private RelayCommand _openAddNoteViewCommand;
        public ICommand OpenAddNoteViewCommand
        {
            get
            {
                if(_openAddNoteViewCommand == null)
                {
                    _openAddNoteViewCommand = new RelayCommand(OpenAddNoteView, IsSelectedToDoNull);
                }
                return _openAddNoteViewCommand;
            }
        }
        private void OpenAddNoteView(object obj)
        {
            var view = new AddNoteView(SelectedToDo, CurrentUser);
            view.Show();
        }

        private RelayCommand _openDocFromViewFilesCommand;
        public ICommand OpenDocFromViewFilesCommand
        {
            get
            {
                if(_openDocFromViewFilesCommand == null)
                {
                    _openDocFromViewFilesCommand = new RelayCommand(OpenDocFromViewFiles);
                }
                return _openDocFromViewFilesCommand;
            }
        }
        private void OpenDocFromViewFiles(object obj)
        {
            System.Diagnostics.Process.Start("explorer.exe", SelectedViewFileLocation);            
        }

        private ObservableCollection<DocumentContextMenuViewModel> _contextMenuActions;
        public ObservableCollection<DocumentContextMenuViewModel> ContextMenuActions
        {
            get
            {
                if( _contextMenuActions == null)
                {
                    _contextMenuActions = new ObservableCollection<DocumentContextMenuViewModel>();
                    _contextMenuActions.Add(new DocumentContextMenuViewModel()
                    {
                        Header = "Open View Files Location",
                        ContextMenuAction = new RelayCommand(OpenViewFilesLocationOfDoc)
                    });
                }
                return _contextMenuActions;
            }
            set
            {
                _contextMenuActions = value;
            }
        }

        private RelayCommand _openViewFilesLocationOfDocCommand;
        public ICommand OpenViewFilesLocationOfDocCommand
        {
            get
            {
                if(_openViewFilesLocationOfDocCommand == null)
                {
                    _openViewFilesLocationOfDocCommand = new RelayCommand(OpenViewFilesLocationOfDoc);
                }
                return _openViewFilesLocationOfDocCommand;
            }
        }
        public void OpenViewFilesLocationOfDoc(object item)
        {
            System.Diagnostics.Process.Start("explorer.exe",
                SelectedViewFileLocation.Substring(0, SelectedViewFileLocation.LastIndexOf('\\')));
        }

        private RelayCommand _refreshToDoGridCommand;
        public ICommand RefreshToDoGridCommand
        {
            get
            {
                if(_refreshToDoGridCommand == null)
                {
                    _refreshToDoGridCommand = new RelayCommand(RefreshToDoGrid);
                }
                return _refreshToDoGridCommand;
            }
        }

        private void RefreshToDoGrid(object obj)
        {
            GetFilteredToDos();
        }
    }
}
