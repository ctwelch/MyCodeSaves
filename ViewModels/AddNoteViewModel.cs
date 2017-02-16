using BasicBridge.Models;
using System.Collections.Generic;
using System.Windows.Input;

namespace BasicBridge.ViewModels
{
    public class AddNoteViewModel :BaseViewModel
    {
        public AddNoteViewModel(ToDo toDo, User currentUser)
        {
            _toDo = toDo;
            _user = currentUser;
            model = new DataModel(_user);
        }

        private ToDo _toDo;
        private static User _user;
        private DataModel model;

        public string Title
        {
            get
            {
                return "Add Note to File # " + _toDo.EntityId;
            }
        }

        private List<Note> _notesToEnter;
        public List<Note> NotesToEnter
        {
            get
            {
                if(_notesToEnter == null)
                {
                    _notesToEnter = model.GetNotesMetaData();
                }
                return _notesToEnter;
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

        private RelayCommand _insertNoteCommand;
        public ICommand InsertNoteCommand
        {
            get
            {
                if(_insertNoteCommand == null)
                {
                    _insertNoteCommand = new RelayCommand(InsertNote, (o) => SelectedNote != null);
                }
                return _insertNoteCommand;
            }
        }

        private void InsertNote(object obj)
        {
            SelectedNote.EntityNum = _toDo.EntityNum;
            model.InsertNote(SelectedNote);
        }
    }
}
