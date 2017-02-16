using BasicBridge.Data;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System;

namespace BasicBridge.Models
{
    public class DataModel
    {
        private string _ppConnectionString;
        private string _scannerConnection;
        private DataAccess _db;
        private DataAccess _scannerDb;

        public DataModel()
        {
#if DEBUG
            _ppConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["PPReportConnection"].ConnectionString;
#else
            _ppConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MainSystemConnection"].ConnectionString;
#endif

            _scannerConnection = System.Configuration.ConfigurationManager.ConnectionStrings["ScannerConnection"].ConnectionString;

            _db = new DataAccess(_ppConnectionString, DbProviderFactories.GetFactory("System.Data.Odbc"));
            _scannerDb = new DataAccess(_scannerConnection, DbProviderFactories.GetFactory("System.Data.SqlClient"));
        }
        public DataModel(User currentUser) : this()
        {            
            _currentUser = currentUser;
        }

        private User _currentUser;
        public List<UserInput> GetToDoUserInputs(ToDo toDo)
        {
            var sql = "SELECT ui.Id, n.InputLabel, ui.InputValue " +
                      "FROM ( " + 
                      " SELECT * FROM ToDoNoteAutomationTranTextSegments " + 
                      " WHERE SegmentType = 'UserInput' AND ToDoId = '" + toDo.ActCode + "' " + 
                      " AND UserId = '" + _currentUser.UserId + "') n " + 
                      "LEFT JOIN ToDoUserInputs ui " + 
                      "ON ui.ToDoId = n.ToDoId AND ui.UserId = n.UserId " +
                      "AND ui.EntityNum = " + toDo.EntityNum + " ORDER BY n.Id DESC;";

            return _db.ReadPocos<UserInput>(sql);
        }
        public UserInput GetToDoUserInputValue(string inputLabel, ToDo toDo)
        {
            var sql = "SELECT Id, InputLabel, InputValue " +
                      "FROM ToDoUserInputs " +
                      "WHERE EntityNum = " + toDo.EntityNum + " " +
                      "AND InputLabel = '" + inputLabel + "' AND ToDoId = '" + toDo.ActCode + "' " + 
                      "AND UserId = '" + _currentUser.UserId + "';";

            var result = _db.ReadPocos<UserInput>(sql);

            return result.Count > 0 ? result.First() : new UserInput() { InputValue = "" };
        }
        public void UpdateUserInput(UserInput userInput)
        {
            var sql =
                "MERGE INTO ToDoUserInputs " +
                "ON Id = " + userInput.Id + " " +
                "WHEN MATCHED THEN " +
                    "UPDATE " +
                    "SET InputValue = '" + userInput.InputValue + "' " +
                    ", InputLabel = '" + userInput.InputLabel + "' " +
                    ", UserId = '" + userInput.UserId + "' " +                  
                "WHEN NOT MATCHED THEN " +
                    "INSERT (ToDoId, EntityNum, UserId, InputLabel, InputValue) VALUES ('" +
                    userInput.ToDoId + "', " + userInput.EntityNum + ", '" + 
                    userInput.UserId + "', '" + userInput.InputLabel + "', '" + userInput.InputValue + "');";

            _db.Write(sql);
        }

        //private IEnumerable<IAutomation> GetToDoAutomations(ToDo toDo, string userId)
        //{
        //    var sql = "SELECT * FROM ToDoUserAutomations WHERE ToDoId = '" + toDo.ActCode + "' AND UserId = '" + userId + "'";

        //    return _db.ReadPocos<ToDoUserNoteAutomation>(sql);
        //}

        public List<ToDo> GetEfileToDos()
        {
            var sql = "SELECT CodeLabel as ActCode " +
                      "FROM uCodes WHERE CodeLabel LIKE 'EFILE%' " +
                      "ORDER BY CodeLabel;";

            return _db.ReadPocos<ToDo>(sql);
        }

        public List<Field> GetUserConfiguredToDoFields(ToDo toDo)
        {
            //var sql = "SELECT u.LabelName, e.MemoText " +
            //          "FROM ToDoUserFieldSettings tdfs " +
            //          "LEFT JOIN UqObj u ON u.Qlabel = tdfs.Questionnaire AND u.FieldNum = tdfs.FieldNum " +
            //          "INNER JOIN EqAnswer e ON e.EntityRole = u.Qlabel AND e.FieldNum = u.FieldNum " +
            //          "WHERE e.EntityNum = " + entityNum + " AND tdfs.ToDoId = '" + toDoId + "' " +
            //          "AND tdfs.UserId = '" + Environment.UserName + "';";

            var sql1 = "SELECT tdfs.LabelName, tdfs.FieldNum, tdfs.Questionnaire FROM ToDoUserFieldSettings tdfs " + 
                       "INNER JOIN UqObj u ON u.Qlabel = tdfs.Questionnaire AND u.FieldNum = tdfs.FieldNum " +
                       "WHERE tdfs.ToDoId = '" + toDo.ActCode + "' " + "AND tdfs.UserId = '" + _currentUser.UserId + "';";

            var fields = _db.ReadPocos<Field>(sql1);

            foreach(var field in fields)
            {
                var sql2 = "SELECT MemoText FROM EqAnswer WHERE EntityNum = " + toDo.EntityNum + " " +
                           "AND FieldNum = " + field.FieldNum + " AND EntityRole = '" + field.Questionnaire + "';";

                var result = _db.ReadPocos<Field>(sql2);

                field.MemoText = result.Count == 0 ? "" : result.First().MemoText; 
            }

            return fields;
        }

        public List<Field> GetFieldsMetaData(string questionnaire)
        {
            var sql = "SELECT FieldNum, LabelName " +
                      "FROM UqObj " +
                      "WHERE QLabel = '" + questionnaire + "' " + 
                      "AND FldFormat NOT IN ('Alias', 'Section', 'ComboBoxList', 'Button - Doc', 'Button - Command', 'Button - Report', 'CheckBox', 'Lookup' ) " + 
                      "AND UPPER(LabelName) NOT LIKE '%RETIRED%' " +
                      "ORDER BY LabelName;";

            return _db.ReadPocos<Field>(sql);
        }

        public List<Questionnaire> GetQuestionnaireNames()
        {
            var sql = "SELECT DISTINCT Qlabel " +
                      "FROM UqObj " +
                      "ORDER BY QLabel;";

            return _db.ReadPocos<Questionnaire>(sql);
        }

        public List<Note> GetUserConfiguredToDoNotes(ToDo toDo)
        {
            var sql = "SELECT n.TranNum, n.ActCode, n.StartDtAct, n.TranText " +
                      "FROM Notes n INNER JOIN ToDoUserNoteSettings tdns ON n.EntityNum = " + toDo.EntityNum + " " +
                      "AND n.ActCode = tdns.CodeLabel " +
                      "WHERE tdns.ToDoId = '" + toDo.ActCode + "' AND tdns.UserId = '" + _currentUser.UserId + "' " +
                      "UNION ALL " +
                      "SELECT TranNum, ActCode, StartDtAct, '**************PENDING****************' + char(10) + char(13) + TranText " +
                      "FROM tblAutoMerge_Input i INNER JOIN ToDoUserNoteSettings tdns ON i.EntityNum = " + toDo.EntityNum + " " +
                      "AND ActCode = tdns.CodeLabel AND tdns.UserId = '" + _currentUser.UserId + "' " +
                      "AND tdns.ToDoId = '" + toDo.ActCode + "' " +
                      "ORDER BY n.StartDtAct DESC;";

            return _db.ReadPocos<Note>(sql);
        }

        public void SetToDoComplete(ToDo toDo)
        {
            var sql = "UPDATE ToDoAppt SET TranStatus = 'FINISH', History =  CONCAT(COALESCE(History, ''), ' Completed via Bridge Automation'), " +
                      "DateChgd = CURDATE(), TimeChgd = CAST(CURTIME() as SQL_CHAR(12)), ChangedBy = " + _currentUser.EntityNum + ", " + 
                      "ChangeInfo = COALESCE(ChangeInfo, '') +  CHAR(10) + CHAR(13) + ' Completed via Bridge Automation', " +
                      "StopDtAct  = CURDATE() " +
                      "WHERE TranNum = " + toDo.TranNum;

            _db.Write(sql);
        }

        public NoteAutomation SelectedNoteAutomation { get; set; }

        public List<NoteAutomation> GetToDoNoteAutomations(ToDo toDo)
        {
            var sql = "SELECT Id, NoteToInsert " +
                      "FROM ToDoUserNoteAutomations WHERE UserId = '" + _currentUser.UserId + "' " +
                      "AND ToDoId = '" + toDo.ActCode + "';";

            return _db.ReadPocos<NoteAutomation>(sql);
        }

        public void InsertNoteAutomation(NoteAutomation noteAutomation)
        {
            var sql = "INSERT INTO ToDoUserNoteAutomations (ToDoId, UserId, NoteToInsert) " + 
                      "VALUES ('" + noteAutomation.ToDoId + "', '" + noteAutomation.UserId + "', '" + noteAutomation.NoteToInsert + "' )";

            _db.Write(sql);
        }

        public void DeleteNoteAutomation(int id)
        {
            var sql = "DELETE FROM ToDoUserNoteAutomations WHERE Id = " + id + ";";

            _db.Write(sql);
        }

        public List<ConstantTranTextSegment> GetConstantTranTextSegments(string toDoId)
        {
            var sql = "SELECT Id, ToDoId, UserId, SegmentType, SegmentOrder, ConstantText " +
                      "FROM ToDoNoteAutomationTranTextSegments " +
                      "WHERE UserId = '" + _currentUser.UserId + 
                      "' AND ToDoID = '" + toDoId + "' AND SegmentType = 'Constant';";

            return _db.ReadPocos<ConstantTranTextSegment>(sql);
        }
        public List<UserInputTranTextSegment> GetUserInputTranTextSegments(string toDoId)
        {
            var sql = "SELECT Id, ToDoId, UserId, SegmentType, SegmentOrder, InputLabel, InputValue " +
                      "FROM ToDoNoteAutomationTranTextSegments " +
                      "WHERE UserId = '" + _currentUser.UserId + 
                      "' AND ToDoID = '" + toDoId + "' AND SegmentType = 'UserInput';";

            return _db.ReadPocos<UserInputTranTextSegment>(sql);
        }
        public List<ToDoPropertyTranTextSegment> GetToDoPropertyTranTextSegments(string toDoId)
        {
            var sql = "SELECT Id, ToDoId, UserId, SegmentType, SegmentOrder, ToDoPropertyName " +
                      "FROM ToDoNoteAutomationTranTextSegments " +
                      "WHERE UserId = '" + _currentUser.UserId + 
                      "' AND ToDoID = '" + toDoId + "' AND SegmentType = 'ToDoProperty';";

            return _db.ReadPocos<ToDoPropertyTranTextSegment>(sql);
        }
        public List<TranTextSegment> GetUserTranTextSegments(string toDoId)
        {
            var allSegments = new List<TranTextSegment>();

            allSegments.AddRange(GetConstantTranTextSegments(toDoId));
            allSegments.AddRange(GetUserInputTranTextSegments(toDoId));
            allSegments.AddRange(GetToDoPropertyTranTextSegments(toDoId));

            return allSegments.OrderBy(x => x.Id).ToList();
        }
        public void InsertNewSegment(ConstantTranTextSegment newSegment)
        {
            var sql = "INSERT INTO ToDoNoteAutomationTranTextSegments (ToDoId, UserId, SegmentType, SegmentOrder, ConstantText) " +
                      "VALUES ('" + newSegment.ToDoId + "', '" + newSegment.UserId + "', '" + newSegment.SegmentType + "', " + newSegment.SegmentOrder + ", '" +
                      newSegment.ConstantText + "');";

            _db.Write(sql);
        }
        public void InsertNewSegment(UserInputTranTextSegment newSegment)
        {
            var sql = "INSERT INTO ToDoNoteAutomationTranTextSegments (ToDoId, UserId, SegmentType, SegmentOrder, InputLabel) " +
                      "VALUES ('" + newSegment.ToDoId + "', '" + newSegment.UserId + "', '" + newSegment.SegmentType + "', " + newSegment.SegmentOrder + ", '" +
                      newSegment.InputLabel + "');";

            _db.Write(sql);
        }
        public void InsertNewSegment(ToDoPropertyTranTextSegment newSegment)
        {
            var sql = "INSERT INTO ToDoNoteAutomationTranTextSegments (ToDoId, UserId, SegmentType, SegmentOrder, ToDoPropertyName) " +
                      "VALUES ('" + newSegment.ToDoId + "', '" + newSegment.UserId + "', '" + newSegment.SegmentType + "', " + newSegment.SegmentOrder + ", '" +
                      newSegment.ToDoPropertyName + "');";

            _db.Write(sql);
        }
        public void DeleteSegment(int id)
        {
            var sql = "DELETE FROM ToDoNoteAutomationTranTextSegments WHERE Id = " + id.ToString();

            _db.Write(sql);
        }
        public List<ToDoUserFieldSetting> GetUserFieldSettings(string toDoId, string questionniare)
        {
            var sql = "SELECT td.Id, td.ToDoId, td.UserId, td.FieldNum, td.LabelName, td.Questionnaire " +
                      "FROM ToDoUserFieldSettings td " + 
                      "INNER JOIN Uqobj u ON td.Questionnaire = u.QLabel " +
                      "WHERE td.UserId = '" + _currentUser.UserId + "' " +
                      "AND td.ToDoId = '" + toDoId + "' " +
                      "AND td.Questionnaire = '" + questionniare + "' " +
                      "AND td.FieldNum = u.FieldNum;";

            return _db.ReadPocos<ToDoUserFieldSetting>(sql);
        }
        public void InsertToDoUserFieldSetting(ToDoUserFieldSetting newToDoUserFieldSetting)
        {
            var n = newToDoUserFieldSetting;
            var sql = "INSERT INTO ToDoUserFieldSettings (ToDoId, UserId, FieldNum, LabelName, Questionnaire) " +
                      "VALUES ('" + n.ToDoId + "','" + n.UserId + "'," + n.FieldNum + ",'" + n.LabelName + "', '" + n.Questionnaire + "');";

            _db.Write(sql);
        }
        public void DeleteToDoUserFieldSetting(string id)
        {
            var sql = "DELETE FROM ToDoUserFieldSettings " +
                      "WHERE Id = " + id + ";";

            _db.Write(sql);
        }
        public void InsertNote(Note newNote)
        {
            var sql =
                "INSERT INTO tblAutoMerge_Input " +
                "(" +
                    "EntityNum, AccompBy, ReportedAs, OperatorNum, TranStatus, TranFlag, HoldTrans, BillType, " +
                    "StartDtAct, StartTmAct, ElapsTmAct, TimeBilled, AmountAct, AmountBilled, RateAct, BillRate, " +
                    "Transum, TranText, ActCode, BillNumber, BillOrder, TaxExempt, TaxablePrimary, TaxableSecondary, " +
                    "PaymentAct, BalanceAct, Priority, GeneratedBy, SeqNum, TranType, CloneNum, PlanNum, " +
                    "ReservedF1, ReservedF2, Boolean1, Boolean2, Boolean3, Boolean4, Boolean5, Boolean6, " +
                    "EnteredBy, DateEntered, TimeEntered, ChangeFlag, ProcessPriority" +
                ") " +
                "VALUES " +
                "(" +
                    newNote.EntityNum + ", " + //EntityNum
                    _currentUser.EntityNum + ", " + //--AccompBy 
                    _currentUser.EntityNum + ", " + //--ReportedAs
                    _currentUser.EntityNum + ", " + //--OperatorNum
                    "'OPEN', " + //--TranStatus
                    "0, " + //--TranFlag
                    "FALSE, " + // --HoldTrans
                    "0, " + // --BillType
                    "CURDATE(), " + // --StartDtAct
                    "CAST(CURTIME() AS SQL_CHAR(12)), " + //  --StartTmAct 
                    "0, " + //--ElapsTmAct
                    "0, " + //--TimeBilled 
                    "0, " + //--AmountAct 
                    newNote.AmountBilled +", " + //--AmountBilled 
                    "0, " + //--RateAct 
                    "0, " + //--BillRate 
                    "LEFT('" + newNote.TranText + "',32) , '" + //--Transum 
                    newNote.TranText + "', '" + //--TranText 
                    newNote.ActCode + "', " + //--ActCode 
                    "0, " + //--BillNumber 
                    "0, " + //--BillOrder 
                    "TRUE, " + // --TaxExempt 
                    "FALSE, " + //--TaxablePrimary 
                    "FALSE, " + //--TaxableSecondary 
                    "0, " + //--PaymentAct 
                    "0, " + //--BalanceAct 
                    "0, " + //--Priority 
                    "8, " + //--GeneratedBy 
                    "0, " + //--SeqNum 
                    "0, " + //--TranType 
                    "0, " + //--CloneNum 
                    "0, " + //--PlanNum 
                    "'Entered Transaction', " + //--ReservedF1 
                    newNote.ReservedF2 +", " + //--ReservedF2 
                    "FALSE, " + //--Boolean1 
                    "FALSE, " + //--Boolean2 
                    "FALSE, " + //--Boolean3 
                    "FALSE, " + //--Boolean4 
                    "FALSE, " + //--Boolean5 
                    "FALSE, " + //--Boolean6 
                    "999999, " + //--EnteredBy 
                    "CURDATE(), " + //--DateEntered 
                    "CAST(CURTIME() AS SQL_CHAR(12)), " + //--TimeEntered 
                    "'C', " + //--ChangeFlag   
                    "105 " + //--ProcessPriority
                ");";

            _db.Write(sql);
        }
        public List<ToDoUserNoteSetting> GetUserNoteSettings(string toDoId)
        {
            var sql = "SELECT Id, ToDoId, UserId, CodeLabel as ActCode " + 
                      "FROM ToDoUserNoteSettings " +
                      "WHERE UserId = '" + _currentUser.UserId + "' " +
                      "AND ToDoId = '" + toDoId + "';";

            return _db.ReadPocos<ToDoUserNoteSetting>(sql);
        }

        public List<Note> GetNotesMetaData()
        {
            var sql = "SELECT CodeLabel as ActCode FROM Ucodes WHERE CodeClass = '0' ORDER BY CodeLabel;";

            return _db.ReadPocos<Note>(sql);
        }

        public void InsertToDoUserNoteSetting(ToDoUserNoteSetting newToDoUserNoteSetting)
        {
            var n = newToDoUserNoteSetting;
            var sql = "INSERT INTO ToDoUserNoteSettings (ToDoId, UserId, CodeLabel) " +
                      "VALUES ('" + n.ToDoId + "','" + n.UserId + "','" + n.ActCode + "');";

            _db.Write(sql);
        }

        public void DeleteToDoUserNoteSetting(string id)
        {
            var sql = "DELETE FROM ToDoUserNoteSettings " +
                      "WHERE Id = " + id + ";";

            _db.Write(sql);
        }

        public void InsertCustomUserNote(string actCode)
        {
            var sql = "INSERT INTO ToDoCustomUserNotes (UserId, ActCode) " +
                      "VALUES ('" + _currentUser.UserId + "','" + actCode + "');";

            _db.Write(sql);
        }

        public void DeleteCustomUserNote(string id)
        {
            var sql = "DELETE FROM ToDoCustomUserNotes " +
                      "WHERE Id = " + id + ";";

            _db.Write(sql);
        }

        public List<Note> GetCustomUserNotes()
        {
            var sql = "SELECT Id AS CustomUserNoteId, ActCode " + 
                      "FROM ToDoCustomUserNotes WHERE UserId = '" + _currentUser.UserId + "';";

            return _db.ReadPocos<Note>(sql);
        }

        public void PopulateDocSentField(int tranNum)
        {
            var sql =
                "insert into tblAutomerge_EQAnswer (Entitynum, Entityrole, actiongroup,labelname, " +
                "fieldnum, memotext, processpriority, honortransAction) " +
                "select td.entitynum, left(sc.qlabel,16) as Entityrole, sg.actiongroup, sc.fieldname, " +
                "sc.fieldnum, (RIGHT('0' + trim(CAST(MONTH(CURDATE()) AS SQL_CHAR(2))),2)) + '/' + " +
                "RIGHT('0' + TRIM(CAST(DAYOFMONTH(CURDATE()) AS SQL_CHAR)),2) + '/' + " +
                "TRIM(CAST(YEAR(CURDATE()) as SQL_CHAR)) as memotext, 5 as processpriority, " +
                "true as HonorTransAction " +
                "from ToDoAppt td " +
                "inner join tblScanGroup sg on td.reservedf2 = sg.scangroupid " +
                "inner join tblScanConfig sc on sg.DocType = sc.DocType " +
                "where sc.OutputType = 2 and " +
                "(coalesce(sc.defaultquest,'') <> '' or  sc.QLabel = sg.Questionnaire) " + 
                "AND td.TranNum = " + tranNum + ";";

            _db.Write(sql);
        }

        public User Login(string userId, string ecrypted)
        {
            var sql = "SELECT UserId, EntityNum FROM uUsers WHERE UserId = '" + userId + 
                      "' AND PasswordFd = '" + ecrypted + "';";

            var result = _db.ReadPocos<User>(sql);

            if(result.Count == 1)
            {
                return result.First();
            }

            return new User();
        }

#region Filters

        public List<ToDo> DefaultToDoFilter()
        {
#if DEBUG
            var sql = "SELECT TOP 20 e.EntityId, td.TranNum, td.EntityNum, td.StartDtAct, td.StartTmAct, td.TranText, " +
                      "LEFT(td.TranText, LOCATE('Doc:',td.TranText) - 2) as TranTextAlias, td.ActCode, " +
                      "td.LocCode, td.Priority, td.ReservedF2, e.TheStatus, e.DocLoc, ReportedAs.EntityId as AssignedTo " +
                      "FROM ToDoAppt td " +
                      "INNER JOIN Entities e ON td.EntityNum = e.EntityNum " +
                      "INNER JOIN Entities ReportedAs ON td.ReportedAs = ReportedAs.EntityNum " +
                      "WHERE ReportedAs.EntityNum = " + _currentUser.EntityNum + " AND td.TranStatus = 'FINISH' " +
                      "ORDER BY td.DateEntered DESC;";
#else
             var sql = "SELECT e.EntityId, td.TranNum, td.EntityNum, td.StartDtAct, td.StartTmAct, td.TranText, " +
                       //"REPLACE(td.TranText, LEFT(td.TranText, LOCATE('Doc:',td.TranText) - 2), '') as TranText, " +
                       "LEFT(td.TranText, LOCATE('Doc:',td.TranText) - 2) as TranTextAlias, td.ActCode, " +
                       "td.LocCode, td.Priority, td.ReservedF2, e.TheStatus, e.DocLoc, ReportedAs.EntityId as AssignedTo " +
                       "FROM ToDoAppt td " +
                       "INNER JOIN Entities e ON td.EntityNum = e.EntityNum " +
                       "INNER JOIN Entities ReportedAs ON td.ReportedAs = ReportedAs.EntityNum " +
                       "WHERE ReportedAs.EntityNum = " + _currentUser.EntityNum + " AND td.TranStatus = 'OPEN' " + 
                       "ORDER BY td.DateEntered DESC;";
#endif

            return GetFilteredToDosWithScanItems(sql);
        }       

        public List<ToDo> UnassignedEfilingsToDoFilter()
        {
            var sql = "SELECT e.EntityId, td.TranNum, td.EntityNum, td.StartDtAct, td.StartTmAct, td.TranText as TranTextAlias, " +
                      //"LEFT(td.TranText, LOCATE('Doc:',td.TranText) - 1) as TranText , 
                      "td.LocCode, td.Priority, td.ReservedF2, e.TheStatus, e.DocLoc, ReportedAs.EntityId as AssignedTo " +
                      "FROM ToDoAppt td " +
                      "INNER JOIN Entities e ON td.EntityNum = e.EntityNum " +
                      "INNER JOIN Entities ReportedAs ON td.ReportedAs = ReportedAs.EntityNum " +
                      "WHERE e.EntityID NOT LIKE 'TEST%' " +
                      "AND (td.ActCode LIKE 'EFILE%' OR td.ActCode = 'AUDIT_ECOURTESY') " +
                      "AND td.TranStatus = 'OPEN' AND ReportedAs.EntityID = 'AUTOMATION2' " +
                      "ORDER BY td.DateEntered DESC;";

            return _db.ReadPocos<ToDo>(sql);

            //return GetFilteredToDosWithScanItems(sql);
        }

        public List<ToDo> AssignedEfilingsToDoFilter()
        {
            var sql = "SELECT e.EntityId, td.TranNum, td.EntityNum, td.StartDtAct, td.StartTmAct, td.TranText as TranTextAlias, " +
                      //"LEFT(td.TranText, LOCATE('Doc:',td.TranText) - 1) as TranText , 
                      "td.LocCode, td.Priority, td.ReservedF2, e.TheStatus, e.DocLoc, ReportedAs.EntityId as AssignedTo " +
                      "FROM ToDoAppt td " +
                      "INNER JOIN Entities e ON td.EntityNum = e.EntityNum " +
                      "INNER JOIN Entities ReportedAs ON td.ReportedAs = ReportedAs.EntityNum " +
                      "WHERE e.EntityID NOT LIKE 'TEST%' " +
                      "AND (td.ActCode LIKE 'EFILE%' OR td.ActCode = 'AUDIT_ECOURTESY') " +
                      "AND td.TranStatus = 'OPEN' AND ReportedAs.EntityID <> 'AUTOMATION2' " +
                      "ORDER BY td.DateEntered DESC;";

            return _db.ReadPocos<ToDo>(sql);

            //return GetFilteredToDosWithScanItems(sql);
        }

        public List<ToDo> CancelAuditToDoFilter()
        {
            var sql = "SELECT e.EntityId, td.TranNum, td.EntityNum, td.StartDtAct, td.StartTmAct, td.TranText as TranTextAlias, " +
                      //"LEFT(td.TranText, LOCATE('Doc:',td.TranText) - 1) as TranText , 
                      "td.LocCode, td.Priority, td.ReservedF2, e.TheStatus, e.DocLoc, ReportedAs.EntityId as AssignedTo " +
                      "FROM ToDoAppt td " +
                      "INNER JOIN Entities e ON td.EntityNum = e.EntityNum " +
                      "INNER JOIN Entities ReportedAs ON td.ReportedAs = ReportedAs.EntityNum " +
                      "WHERE td.TranStatus = 'CANCEL' " +
                      "AND (ReportedAs.EntityID <> 'AUTOMATION2' OR ReportedAs.EntityID IS NULL) " +
                      "AND (td.ActCode LIKE 'EFILE%' OR td.ActCode LIKE 'ESERVE%') AND td.StartDtAct > CURDATE() - 8 " +
                      "ORDER BY td.DateEntered DESC;";

            return _db.ReadPocos<ToDo>(sql);

            //return GetFilteredToDosWithScanItems(sql);
        }

        public List<ToDo> EfileTodayToDoFilter()
        {
            var sql = "SELECT e.EntityId, td.TranNum, td.EntityNum, td.StartDtAct, td.StartTmAct, td.TranText as TranTextAlias, " +
                      //"LEFT(td.TranText, LOCATE('Doc:',td.TranText) - 1) as TranText , 
                      "td.TranText, td.ActCode, " +
                      "td.LocCode, td.Priority, td.ReservedF2, e.TheStatus, e.DocLoc, ReportedAs.EntityId as AssignedTo " +
                      "FROM ToDoAppt td " +
                      "INNER JOIN Entities e ON td.EntityNum = e.EntityNum " +
                      "INNER JOIN Entities ReportedAs ON td.ReportedAs = ReportedAs.EntityNum " +
                      "WHERE (td.TranStatus = 'OPEN' OR td.TranStatus = 'FINISH') " +
                      "AND td.StartDtAct = CURDATE() AND ActCode LIKE 'EFILE%' " +
                      "ORDER BY td.DateEntered DESC;";

            return _db.ReadPocos<ToDo>(sql);

            //return GetFilteredToDosWithScanItems(sql);
        }

        public List<ToDo> RtsMailToDoFilter()
        {
            var sql = "SELECT e.EntityId, td.TranNum, td.EntityNum, td.StartDtAct, td.StartTmAct, td.TranText as TranTextAlias, " +
                      //"LEFT(td.TranText, LOCATE('Doc:',td.TranText) - 1) as TranText , 
                      "td.LocCode, td.Priority, td.ReservedF2, e.TheStatus, e.DocLoc, ReportedAs.EntityId as AssignedTo " +
                      "FROM ToDoAppt td " +
                      "INNER JOIN Entities e ON td.EntityNum = e.EntityNum " +
                      "INNER JOIN Entities ReportedAs ON td.ReportedAs = ReportedAs.EntityNum " +
                      "WHERE td.TranStatus = 'OPEN' AND td.ActCode LIKE 'RTS MAIL%' " +
                      "ORDER BY td.DateEntered DESC;";

            return _db.ReadPocos<ToDo>(sql);

            //return GetFilteredToDosWithScanItems(sql);
        }

        private List<ToDo> GetFilteredToDosWithScanItems(string sql)
        {
            var results = _db.ReadPocos<ToDo>(sql);

            foreach (var toDo in results)
            {
                toDo.ScanItems = GetScanItems(toDo);
            }           

            return results;
        }

        private List<ScanItem> GetScanItems(ToDo toDo)
        {
            // get scanIds from tblScanGroup
            var sql = "SELECT ScanId, ActionGroup FROM tblScanGroup WHERE ScanGroupId = " + toDo.ReservedF2;
            var scanGroups = _db.ReadPocos<ScanGroup>(sql);

            // create IN list for sql
            var inList = "(";
            foreach (var scanGroup in scanGroups)
            {
                inList += scanGroup.ScanId.ToString() + ",";
                toDo.ActionGroup = scanGroup.ActionGroup;
            }

            inList = inList.TrimEnd(',');
            inList += ")";

            // use those scanIds to generate a list of doc locations
            sql = "SELECT ScanId, DocLoc, ActionGroup, " +
                  @"REVERSE(LEFT(REVERSE(DocLoc), CHARINDEX('\', REVERSE(DocLoc)) - 1)) as DocLocAlias " +
                  @"FROM tblScanItems WHERE ScanId IN " + inList + " " +
                  @"AND CHARINDEX('\', REVERSE(DocLoc)) <> 0 AND DocLoc IS NOT NULL;";

            return _scannerDb.ReadPocos<ScanItem>(sql);
        }

#endregion
    }
}
