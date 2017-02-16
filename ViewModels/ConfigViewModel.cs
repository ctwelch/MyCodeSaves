using BasicBridge.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Reflection;

namespace BasicBridge.ViewModels
{
    public class ConfigViewModel : BaseViewModel
    {
        public ConfigViewModel() { }
        public ConfigViewModel(ToDo selectedToDo, User user)
        {
            CurrentUser = user;
            model = new DataModel(user);
            SelectedToDo = selectedToDo;
            SelectedQuestionnaire = new Questionnaire() { Qlabel = "FORECLOSURE" };            
        }
        
        private DataModel model;
        public User CurrentUser { get; set; }

        private List<ToDo> _distinctToDos;
        public List<ToDo> DistinctToDos
        {
            get
            {
                if(_distinctToDos == null)
                {
                    _distinctToDos = model.GetEfileToDos();
                }
                return _distinctToDos;
            }
            set
            {
                _distinctToDos = value;
                FlagPropertyChanged("DistinctToDos");
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
                _selectedToDo = value;

                UserFieldSettings = GetUserFieldSettings();
                UserNoteSettings = GetUserNoteSettings();
                TranTextSegments = GetUserTranTextSegments();
                NoteAutomations = GetNoteAutomations();

                FlagPropertyChanged("SelectedToDo");
            }
        }

        private List<TranTextSegment> GetUserTranTextSegments()
        {
            return model.GetUserTranTextSegments(SelectedToDo.ActCode);
        }

        #region Fields /////////////////////////////////////////////////////////

        private List<Field> _fieldsMetaData;
        public List<Field> FieldsMetaData
        {
            get
            {
                return _fieldsMetaData;
            }
            set
            {
                _fieldsMetaData = value;
                FlagPropertyChanged("FieldsMetaData");
            }
        }

        private List<Questionnaire> _questionnaireNames;
        public List<Questionnaire> QuestionnaireNames
        {
            get
            {
                if(_questionnaireNames == null)
                {
                    _questionnaireNames = GetQuestionnaireNames();
                }
                return _questionnaireNames;
            }
            set
            {
                _questionnaireNames = value;
                FlagPropertyChanged("QuestionnaireNames");
            }
        }
        private List<Questionnaire> GetQuestionnaireNames()
        {
            return model.GetQuestionnaireNames();
        }

        private Questionnaire _selectedQuestionnaire;
        public Questionnaire SelectedQuestionnaire
        {
            get
            {
                return _selectedQuestionnaire;
            }
            set
            {
                _selectedQuestionnaire = value;
                
                FieldsMetaData = model.GetFieldsMetaData(value.Qlabel);
                UserFieldSettings = GetUserFieldSettings();

                FlagPropertyChanged("SelectedQuestionnaire");
            }
        }

        private Field _selectedField;
        public Field SelectedField
        {
            get
            {
                return _selectedField;
            }
            set
            {
                _selectedField = value;
                FlagPropertyChanged("SelectedField");
            }
        }

        private RelayCommand _addToDoUserFieldSettingCommand;
        public ICommand AddToDoUserFieldSettingCommand
        {
            get
            {
                if(_addToDoUserFieldSettingCommand == null)
                {
                    _addToDoUserFieldSettingCommand = new RelayCommand(AddToDoUserFieldSetting, CanUserAddFieldSetting);
                }
                return _addToDoUserFieldSettingCommand;
            }
        }
        private bool CanUserAddFieldSetting(object obj)
        {
            if(SelectedToDo != null && SelectedQuestionnaire != null)
            {
                return true;
            }
            return false;
        }
        private void AddToDoUserFieldSetting(object fieldToAdd)
        {
            var newToDoUserFieldSetting = new ToDoUserFieldSetting()
            {
                ToDoId = SelectedToDo.ActCode,
                UserId = CurrentUser.UserId,
                FieldNum = SelectedField.FieldNum,
                LabelName = SelectedField.LabelName.Replace("'", "''"),
                Questionnaire = SelectedQuestionnaire.Qlabel
            };
            

            model.InsertToDoUserFieldSetting(newToDoUserFieldSetting);
            UserFieldSettings = GetUserFieldSettings();
        }

        private RelayCommand _removeToDoUserFieldSettingCommand;
        public ICommand RemoveToDoUserFieldSettingCommand
        {
            get
            {
                if(_removeToDoUserFieldSettingCommand == null)
                {
                    _removeToDoUserFieldSettingCommand = new RelayCommand(RemoveToDoUserFieldSetting);
                }
                return _removeToDoUserFieldSettingCommand;
            }
        }
        private void RemoveToDoUserFieldSetting(object fieldToRemove)
        {
            var fieldToDelete = fieldToRemove as ToDoUserFieldSetting;
            var id = fieldToDelete.Id.ToString();
            model.DeleteToDoUserFieldSetting(id);
            UserFieldSettings = GetUserFieldSettings();
        }

        private List<ToDoUserFieldSetting> _userFieldSettings;
        public List<ToDoUserFieldSetting> UserFieldSettings
        {
            get
            {
                return GetUserFieldSettings();
            }
            set
            {
                _userFieldSettings = value;
                FlagPropertyChanged("UserFieldSettings");
            }
        }
        private List<ToDoUserFieldSetting> GetUserFieldSettings()
        {
            if(SelectedToDo != null && SelectedQuestionnaire != null)
            {
                return model.GetUserFieldSettings(SelectedToDo.ActCode, SelectedQuestionnaire.Qlabel);
            }
            return new List<ToDoUserFieldSetting>();
        }
        #endregion

        #region Notes /////////////////////////////////////////////////////////

        private List<Note> _notesMetaData;
        public List<Note> NotesMetaData
        {
            get
            {
                if(_notesMetaData == null)
                {
                    _notesMetaData = model.GetNotesMetaData();
                }
                return _notesMetaData;
            }
            set
            {
                _notesMetaData = value;
                FlagPropertyChanged("NotesMetaData");
            }
        }

        private Note _selectedNote;
        public Note SelectedNote
        {
            get
            {
                return _selectedNote;
            }
            set
            {
                _selectedNote = value;
                FlagPropertyChanged("SelectedNote");
            }
        }
        private List<ToDoUserNoteSetting> GetUserNoteSettings()
        {
            if (SelectedToDo != null)
            {
                return model.GetUserNoteSettings(SelectedToDo.ActCode);
            }
            return new List<ToDoUserNoteSetting>();
        }

        private List<ToDoUserNoteSetting> _userNoteSettings;
        public List<ToDoUserNoteSetting> UserNoteSettings
        {

            get
            {
                return _userNoteSettings;
            }
            set
            {
                _userNoteSettings = value;
                FlagPropertyChanged("UserNoteSettings");
            }
        }

        private RelayCommand _addToDoUserNoteSettingCommand;
        public ICommand AddToDoUserNoteSettingCommand
        {
            get
            {
                if(_addToDoUserNoteSettingCommand == null)
                {
                    _addToDoUserNoteSettingCommand = new RelayCommand(AddToDoUserNoteSetting, CanUserAddNoteSetting);
                }
                return _addToDoUserNoteSettingCommand;
            }
        }
        private void AddToDoUserNoteSetting(object obj)
        {
            var newToDoUserNoteSetting = new ToDoUserNoteSetting()
            {
                ToDoId = SelectedToDo.ActCode,
                UserId = CurrentUser.UserId,
                ActCode = SelectedNote.ActCode
            };
            
            model.InsertToDoUserNoteSetting(newToDoUserNoteSetting);
            UserNoteSettings = GetUserNoteSettings();
        }
        private bool CanUserAddNoteSetting(object obj)
        {
            return SelectedToDo != null;
        }

        private RelayCommand _removeToDoUserNoteSettingCommand;
        public ICommand RemoveToDoUserNoteSettingCommand
        {
            get
            {
                if(_removeToDoUserNoteSettingCommand == null)
                {
                    _removeToDoUserNoteSettingCommand = new RelayCommand(RemoveToDoUserNoteSetting);
                }
                return _removeToDoUserNoteSettingCommand;
            }
        }
        private void RemoveToDoUserNoteSetting(object noteToRemove)
        {
            var noteToDelete = noteToRemove as ToDoUserNoteSetting;
            var id = noteToDelete.Id;
            model.DeleteToDoUserNoteSetting(id.ToString());
            UserNoteSettings = GetUserNoteSettings();
        }

        // Allow user to select which notes will be available to them in the application
        // this is because there are over 55 thousand notes available in PerfectPractice

        private List<Note> _customUserNotes;
        public List<Note> CustomUserNotes
        {
            get
            {
                if(_customUserNotes == null)
                {
                    _customUserNotes = model.GetCustomUserNotes();
                }
                return _customUserNotes;
            }
            set
            {
                _customUserNotes = value;
                FlagPropertyChanged("CustomUserNotes");
            }
        }

        private RelayCommand _addCustomUserNoteCommand;
        public ICommand AddCustomUserNoteCommand
        {
            get
            {
                if(_addCustomUserNoteCommand == null)
                {
                    _addCustomUserNoteCommand = new RelayCommand(AddCustomUserNote);
                }
                return _addCustomUserNoteCommand;
            }
        }
        private void AddCustomUserNote(object obj)
        {
            model.InsertCustomUserNote(SelectedNote.ActCode);
            CustomUserNotes = model.GetCustomUserNotes();
        }

        private RelayCommand _removeCustomUserNoteCommand;
        public ICommand RemoveCustomUserNoteCommand
        {
            get
            {
                if(_removeCustomUserNoteCommand == null)
                {
                    _removeCustomUserNoteCommand = new RelayCommand(RemoveCustomUserNote);
                }
                return _removeCustomUserNoteCommand;
            }
        }
        private void RemoveCustomUserNote(object noteToRemove)
        {
            var noteToDelete = noteToRemove as Note;
            model.DeleteCustomUserNote(noteToDelete.CustomUserNoteId.ToString());
            CustomUserNotes = model.GetCustomUserNotes();
        }
        #endregion

        #region  Note Automations /////////////////////////////////////////////////////////

        private List<TranTextSegment> _tranTextSegments;
        public List<TranTextSegment> TranTextSegments
        {
            get
            {
                return _tranTextSegments;
            }
            set
            {
                _tranTextSegments = value;
                FlagPropertyChanged("TranTextSegments");
            }
        }  

        private Dictionary<string, TranTextSegment> _segmentTypes;
        public Dictionary<string, TranTextSegment> SegmentTypes
        {
            get
            {
                if(_segmentTypes == null)
                {
                    _segmentTypes = SetUpSegmentTypes();
                }
                return _segmentTypes;
            }
            set
            {
                _segmentTypes = value;
            }
        }
        public Dictionary<string, TranTextSegment> SetUpSegmentTypes()
        {
            var segmentTemplates = new Dictionary<string, TranTextSegment>();

            segmentTemplates.Add("Constant", new ConstantTranTextSegment());
            segmentTemplates.Add("User Input", new UserInputTranTextSegment());
            segmentTemplates.Add("ToDo Property", new ToDoPropertyTranTextSegment());

            return segmentTemplates;
        }

        private TranTextSegment _newSegment;
        public TranTextSegment NewSegment
        {
            get
            {
                return _newSegment;
            }
            set
            {
                _newSegment = value;
                NewSegments = new List<ITranTextSegment>() { _newSegment };
                FlagPropertyChanged("NewSegment");
            }
        }

        List<ITranTextSegment> _newSegments;
        public List<ITranTextSegment> NewSegments
        {
            get
            {
                return _newSegments;
            }
            set
            {
                _newSegments = value;
                FlagPropertyChanged("NewSegments");
            }
        }

        private List<PropertyInfo> _toDoProperties;
        public List<PropertyInfo> ToDoProperties
        {
            get
            {
                if(_toDoProperties == null)
                {
                    _toDoProperties = new ToDo().GetType().GetProperties().ToList();
                }
                return _toDoProperties;
            }
            set
            {
                _toDoProperties = value;
            }
        }

        private string _selectedToDoProperty;
        public string SelectedToDoProperty
        {
            get
            {
                return _selectedToDoProperty;
            }
            set
            {
                _selectedToDoProperty = value.Substring(value.IndexOf(" ") + 1);
            }
        } 

        private RelayCommand _addNewSegmentCommand;
        public ICommand AddNewSegmentCommand
        {
            get
            {
                if(_addNewSegmentCommand == null)
                {
                    _addNewSegmentCommand = new RelayCommand(AddNewSegment);
                }
                return _addNewSegmentCommand;
            }
        }
        private void AddNewSegment(object obj)
        {
            NewSegment.ToDoId = SelectedToDo.ActCode;
            NewSegment.UserId = CurrentUser.UserId;

            Type theType = NewSegment.GetType();

            if(theType == typeof(ConstantTranTextSegment))
            {
                NewSegment.SegmentType = "Constant";
                var newSegment = NewSegment as ConstantTranTextSegment;
                newSegment.ConstantText = newSegment.ConstantText.Replace("'", "''");
                model.InsertNewSegment(newSegment);
            }

            if (theType == typeof(UserInputTranTextSegment))
            {
                NewSegment.SegmentType = "UserInput";
                var newSegment = NewSegment as UserInputTranTextSegment;
                newSegment.InputLabel = newSegment.InputLabel.Replace("'", "''");
                model.InsertNewSegment(newSegment);
            }

            if (theType == typeof(ToDoPropertyTranTextSegment))
            {
                NewSegment.SegmentType = "ToDoProperty";
                ((ToDoPropertyTranTextSegment)NewSegment).ToDoPropertyName = SelectedToDoProperty;
                model.InsertNewSegment(NewSegment as ToDoPropertyTranTextSegment);
            }

            TranTextSegments = GetUserTranTextSegments();
            NewSegments = null;
        }

        private RelayCommand _removeSegmentCommand;
        public ICommand RemoveSegmentCommand
        {
            get
            {
                if (_removeSegmentCommand == null)
                {
                    _removeSegmentCommand = new RelayCommand(RemoveSegment);
                }
                return _removeSegmentCommand;
            }
        }               
        private void RemoveSegment(object obj)
        {
            model.DeleteSegment(int.Parse(obj.ToString()));
            TranTextSegments = GetUserTranTextSegments();
        }
        public bool CanUserRemoveSegment
        {
            get
            {
                if(NewSegment != null)
                {
                    return false;
                }
                return true;
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
                FlagPropertyChanged("NoteAutomations");
            }
        }
        private List<NoteAutomation> GetNoteAutomations()
        {
            if(SelectedToDo != null)
            {
                return model.GetToDoNoteAutomations(SelectedToDo);
            }
            return new List<NoteAutomation>();
        }

        private RelayCommand _addNoteAutomationCommand;
        public ICommand AddNoteAutomationCommand
        {
            get
            {
                if(_addNoteAutomationCommand == null)
                {
                    _addNoteAutomationCommand = new RelayCommand(AddNoteAutomation);
                }
                return _addNoteAutomationCommand;
            }
        }
        private void AddNoteAutomation(object obj)
        {
            var note = obj as Note;
            var noteAutomation = new NoteAutomation()
                { ToDoId = SelectedToDo.ActCode,
                  NoteToInsert = note.ActCode,
                  UserId = CurrentUser.UserId };
            model.InsertNoteAutomation(noteAutomation);
            NoteAutomations = GetNoteAutomations();
        }

        private RelayCommand _removeNoteAutomationCommand;
        public ICommand RemoveNoteAutomationCommand
        {
            get
            {
                if(_removeNoteAutomationCommand == null)
                {
                    _removeNoteAutomationCommand = new RelayCommand(DeleteNoteAutomation);
                }
                return _removeNoteAutomationCommand;
            }
        }
        private void DeleteNoteAutomation(object obj)
        {
            var toRemove = obj as NoteAutomation;
            model.DeleteNoteAutomation(toRemove.Id);
            NoteAutomations = GetNoteAutomations();
        }
        #endregion
    }
}
