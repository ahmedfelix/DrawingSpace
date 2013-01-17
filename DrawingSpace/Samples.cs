using System;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;

namespace DrawingSpace
{
    public class Samples
    {
        [CommandMethod("DSADDENTITY")]
        public static void AddEntity()
        {
            Line line = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
            DrawingSpace.AddEntity(line);
        }

        [CommandMethod("DSADDENTITYTRANS")]
        public static void AddEntityWithTransaction()
        {
            Transaction transaction = HostApplicationServices.WorkingDatabase.TransactionManager
                .StartTransaction();
            Line line = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
            DrawingSpace.AddEntity(line, transaction);

            // Since the transaction is still open we can continue to manipulate the object
            // and get access to properties that are set only after adding it to the database.
            line.Color = Color.FromColorIndex(ColorMethod.ByAci, 1);
            transaction.Commit();
        }

        [CommandMethod("DSADDLAYER")]
        public static void AddLayer()
        {
            DrawingSpace.AddLayer("1");
        }

        [CommandMethod("DSGETDOUBLE")]
        public static void GeDouble()
        {
            PromptStatus status = new PromptStatus();
            double userInput = DrawingSpace.GetDouble("Input double:", ref status, false);

            Editor command = Application.DocumentManager.MdiActiveDocument.Editor;
            command.WriteMessage("Result: " + userInput);
        }

        [CommandMethod("DSGETENTITY")]
        public static void GetEntity()
        {
            Transaction transaction = HostApplicationServices.WorkingDatabase.TransactionManager
                .StartTransaction();
            PromptStatus status = new PromptStatus();
            Entity entity = DrawingSpace.GetEntity("Select object:", ref status, transaction,
                OpenMode.ForWrite);

            // The status lets us know if the user actually selected something or cancelled.
            if (status == PromptStatus.OK)
            {
                entity.Color = Color.FromColorIndex(ColorMethod.ByAci, 2);
                transaction.Commit();
            }
            else
            {
                transaction.Dispose();
            }
        }

        [CommandMethod("DSGETENTITYBYHANDLE")]
        public static void GetEntityByHandle()
        {
            Transaction transaction = HostApplicationServices.WorkingDatabase.TransactionManager
                .StartTransaction();
            string handle = "188"; // Usually the handle assigned to the first object drawn in AutoCAD 2010.

            try
            {
                Entity entity = DrawingSpace.GetEntityByHandle(handle, transaction, OpenMode.ForWrite);

                if (entity != null)
                {
                    entity.Color = Color.FromColorIndex(ColorMethod.ByAci, 3);
                    transaction.Commit();
                }
            }

            catch (System.Exception)
            {
                throw;
            }

            finally
            {
                transaction.Dispose();
            }

        }

        [CommandMethod("DSGETKEYWORD")]
        public static void GetKeyword()
        {
            PromptStatus status = new PromptStatus();
            string[] keywords = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
            string userInput = DrawingSpace.GetKeyword("Select a day:", ref status, keywords);

            Editor command = Application.DocumentManager.MdiActiveDocument.Editor;

            if (status == PromptStatus.OK)
            {
                if (userInput != "")
                {
                    command.WriteMessage("You selected: " + userInput);
                }
            }
            else
            {
                command.WriteMessage("No selection");
            }
        }

        [CommandMethod("DSGETSELECTION")]
        public static void GetSelection()
        {
            Transaction transaction = HostApplicationServices.WorkingDatabase.TransactionManager
                .StartTransaction();
            PromptStatus status = new PromptStatus();
            DBObjectCollection selection = DrawingSpace.GetSelection("Select objects:", ref status,
                transaction, OpenMode.ForWrite);

            // Another way of dealing with the canceling of the operation or empty selection.
            if (status != PromptStatus.OK)
            {
                transaction.Dispose();
                return;
            }

            foreach (Entity entity in selection)
            {
                entity.Color = Color.FromColorIndex(ColorMethod.ByAci, 4);
            }

            transaction.Commit();
        }

        [CommandMethod("DSGETSTRING")]
        public static void GetString()
        {
            PromptStatus status = new PromptStatus();
            string userInput = DrawingSpace.GetString("Input string:", ref status, true);

            if (status == PromptStatus.OK)
            {
                Editor command = Application.DocumentManager.MdiActiveDocument.Editor;

                if (userInput != "")
                {
                    command.WriteMessage("You wrote: " + userInput);
                }
                else
                {
                    command.WriteMessage("No input");
                }
            }
        }
    }
}
